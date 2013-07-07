using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Controller;
using PolymeliaDeploy.Data;
using Microsoft.AspNet.SignalR.Client.Hubs;
using PolymeliaDeploy.Agent.Activity;
using Microsoft.AspNet.SignalR.Client;
using System.Configuration;
using PolymeliaDeploy.DeployController;

namespace PolymeliaDeploy.Agent
{
    public class DeployControllerClient : IDeployControllerClient
    {
        private bool _isRunning;
        private bool _isDesposed;
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private string _serverRole;


        public async Task Connect(string url, string serverRole)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(serverRole)) throw new ArgumentNullException("serverRole");

            _serverRole = serverRole;

            if (_isRunning)
                return;

            await ConnectToDeployControllerHub(url, serverRole);

            _isRunning = true;
        }


        public async Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status)
        {
            EnsureClientIsConnected();

            await _hubProxy.Invoke("UpdateActivityTaskStatus", activityTaskId, status);
        }


        public async Task<IEnumerable<ActivityTaskDto>> GetActivityTasks(long lastTaskId)
        {
            EnsureClientIsConnected();

            return await _hubProxy.Invoke<IEnumerable<ActivityTaskDto>>("GetActivityTasks", lastTaskId, _serverRole);
        }


        public async Task Report(ActivityReport report)
        {
            if (report == null) throw new ArgumentNullException("report");

            EnsureClientIsConnected();

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


        private async Task ConnectToDeployControllerHub(string url, string serverRole)
        {
            _hubConnection = new HubConnection(url);
            _hubProxy = _hubConnection.CreateHubProxy("DeployControllerHub");

            //_hubProxy.On("broadCastToClients", message => System.Console.WriteLine(message));

            await _hubConnection.Start();

            await _hubProxy.Invoke("Connect", serverRole, "Test");
        }

        private void EnsureClientIsConnected()
        {
            if (!_isRunning) throw new ApplicationException("You must first call Connect to connect to the Deploy Controller");
        }
    }
}