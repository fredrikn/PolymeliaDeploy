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
    public class DeployControllerClient : IDeployControllerClient, IDeployConrollerReportClient
    {
        private bool _isRunning;
        private bool _isDesposed;
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private string _serverRole;


        public Action<IEnumerable<ActivityTaskDto>> OnRunActivity { get; set; }


        public async Task Connect(string url, long lastTaskId, string serverRole)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(serverRole)) throw new ArgumentNullException("serverRole");

            _serverRole = serverRole;

            if (_isRunning)
                return;

            await ConnectToDeployControllerHub(url, lastTaskId, serverRole);
        }


        public void RunActivities(IEnumerable<ActivityTaskDto> activitiesToRun)
        {
            if (OnRunActivity != null)
                OnRunActivity(activitiesToRun);
        }


        public async Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status)
        {
            await _hubProxy.Invoke("UpdateActivityTaskStatus", activityTaskId, status);
        }


        public async Task Report(ActivityReport report)
        {
            if (report == null) throw new ArgumentNullException("report");

            await _hubProxy.Invoke("Report", report);
        }


        public void Disconnect()
        {
            if (_hubConnection != null && _hubConnection.State != ConnectionState.Disconnected)
                _hubConnection.Stop();

            _isRunning = false;
        }


        public void Dispose()
        {
            if (_isDesposed) throw new ObjectDisposedException("DeployControllerClient is already disposed");

            Disconnect();

            _isRunning = false;
            _isDesposed = true;
        }


        private async Task ConnectToDeployControllerHub(string url, long lastTaskId,  string serverRole)
        {
            _hubConnection = new HubConnection(url);
            _hubProxy = _hubConnection.CreateHubProxy("DeployControllerHub");

           _hubProxy.On<IEnumerable<ActivityTaskDto>>("RunActivities", RunActivities);

            await _hubConnection.Start();

            await _hubProxy.Invoke("Connect", serverRole, lastTaskId, _hubConnection.ConnectionId);
        }
    }
}