using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client;

using PolymeliaDeploy.DeployController;

namespace PolymeliaDeploy.Agent
{
    using System.Linq;
    using System.Threading;

    public class DeployControllerClient : IDeployControllerClient, IDeployConrollerReportClient
    {
        private bool _isRunning;

        private bool _isDesposed;

        private readonly IList<ActivityReport> _tempActivityReport = new List<ActivityReport>();

        private HubConnection _hubConnection;

        private IHubProxy _hubProxy;

        private string _serverRole;


        public Action<IEnumerable<ActivityTaskDto>> OnRunActivity { get; set; }


        public Action OnConnected { get; set; }

        public void Connect(string url, string serverRole)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(serverRole)) throw new ArgumentNullException("serverRole");

            _serverRole = serverRole;

            if (_isRunning) return;

            _isRunning = true;

            ConnectToDeployControllerHub(url, serverRole);
        }


        public void RunActivities(IEnumerable<ActivityTaskDto> activitiesToRun)
        {
            if (OnRunActivity != null)
                Task.Run( ()=> OnRunActivity(activitiesToRun));
        }


        public async Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status)
        {
            //TODO: Make sure ConnectionId is sent, so Controller can know when all agents are done.
            await _hubProxy.Invoke("UpdateActivityTaskStatus", activityTaskId, status);
        }


        public async Task AgentIsReadyForNewTasks(long lastTaskId)
        {
            await _hubProxy.Invoke("AgentIsReadyForNewTasks", lastTaskId, _serverRole, _hubConnection.ConnectionId);
        }


        public async Task Report(ActivityReport report)
        {
            if (report == null)
                throw new ArgumentNullException("report");
            
            try
            {
                await _hubProxy.Invoke("Report", report);
            }
            catch (Exception)
            {
                _tempActivityReport.Add(report);
            }
        }


        public void Disconnect()
        {
            if (_hubConnection != null && _hubConnection.State != ConnectionState.Disconnected) _hubConnection.Stop();

            _isRunning = false;
        }


        public void Dispose()
        {
            if (_isDesposed) throw new ObjectDisposedException("DeployControllerClient is already disposed");

            Disconnect();

            _isRunning = false;
            _isDesposed = true;
        }


        private void ConnectToDeployControllerHub(string url, string serverRole)
        {
            _hubConnection = new HubConnection(url);
            _hubProxy = _hubConnection.CreateHubProxy("DeployControllerHub");

            _hubConnection.StateChanged += async change => await HandleConnectionStates(url, serverRole, change);

            _hubProxy.On<IEnumerable<ActivityTaskDto>>("RunActivities", RunActivities);

            HandleConnectionStart(_hubConnection.Start(), url);
        }


        private void HandleConnectionStart(Task startTask, string url)
        {
            if (!_isRunning) return;

            startTask.ContinueWith(
                task =>
                {
                    try
                    {
                        if (!task.IsFaulted)
                            return;

                        // make sure to observe the exception or we can get an aggregate exception
                        foreach (var e in task.Exception.Flatten().InnerExceptions)
                            Console.WriteLine("Observed exception trying to handle connection start: " + e.Message);

                        Console.WriteLine("Unable to connect to url {0}, retrying every 5 seconds", url);
                        Thread.Sleep(5000);
                        HandleConnectionStart(_hubConnection.Start(), url);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error handling connection start, retrying", ex);
                        Thread.Sleep(5000);
                        HandleConnectionStart(_hubConnection.Start(), url);
                    }
                });
        }


        private async Task HandleConnectionStates(string url, string serverRole, StateChange change)
        {
            Console.WriteLine("Connection state to controller '{0}' is changed '{1}'", url, change.NewState);

            if (change.NewState == ConnectionState.Connected)
            {
                Thread.Sleep(2000);
                await _hubProxy.Invoke("Connect", serverRole, _hubConnection.ConnectionId);

                Console.WriteLine("Connection to controller '{0}'", url);

                ReportTemporaryReportsIfAvailable(_tempActivityReport);

                if (OnConnected != null)
                    OnConnected();
            }

            //Retry to connect to the Controller each 5 seconds if SignalR reconnecting is timed out.
            if (change.OldState == ConnectionState.Reconnecting && change.NewState == ConnectionState.Disconnected)
                HandleConnectionStart(_hubConnection.Start(), url);
        }


        private void ReportTemporaryReportsIfAvailable(ICollection<ActivityReport> activityReports)
        {
            while (true)
            {
                var report = activityReports.FirstOrDefault();

                if (report == null)
                    return;

                activityReports.Remove(report);

                Report(report).Wait();

                Thread.Sleep(100);

                if (activityReports.Any())
                    continue;

                break;
            }
        }
    }
}