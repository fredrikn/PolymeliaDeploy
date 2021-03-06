﻿using PolymeliaDeploy.Agent.Activity;
using PolymeliaDeploy.Agent.Configuration;
using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using PolymeliaDeploy.DeployController;
using System;

namespace PolymeliaDeploy.Agent
{
    using System.Collections.Generic;
    using PolymeliaDeploy.Security;

    public class AgentService : IAgentService
    {
        private bool _isDisposed;

        private readonly IDeployControllerClient _deployControllerClient;
        private readonly IAgentConfigurationSettings _agentConfig;
        private readonly ITaskActivityExecutioner _taskActivityExecutioner;
        private readonly ITokenManagement _tokenManagement;

        private AgentStatus _agentStatus = AgentStatus.Ready;

        public AgentService(
                            IDeployControllerClient deployControllerClient,
                            IAgentConfigurationSettings agentConfig,
                            ITaskActivityExecutioner taskActivityExecutioner,
                            ITokenManagement tokenManagement)
        {
            if (deployControllerClient == null) throw new ArgumentNullException("deployControllerClient");
            if (agentConfig == null) throw new ArgumentNullException("agentConfig");
            if (taskActivityExecutioner == null) throw new ArgumentNullException("taskActivityExecutioner");
            if (tokenManagement == null) throw new ArgumentNullException("tokenManagement");

            _deployControllerClient = deployControllerClient;
            _agentConfig = agentConfig;
            _taskActivityExecutioner = taskActivityExecutioner;
            _tokenManagement = tokenManagement;
        }


        public void Start()
        {
            _deployControllerClient.OnRunActivity = RunActivities;

            _deployControllerClient.Connect(
                                            _agentConfig.DeployControllerUrl,
                                            _agentConfig.ServerRole,
                                            _tokenManagement.Generate(_agentConfig.ControllerKey));
        }


        private void RunActivities(IEnumerable<ActivityTaskDto> activities)
        {
            _agentStatus = AgentStatus.InProgress;

            _taskActivityExecutioner.ExecuteTasks(
                                                  activities,
                                                  ActivityTaskSucceded,
                                                  ActivityTaskFailed);

            _agentStatus = AgentStatus.Ready;
            _deployControllerClient.AgentIsReadyForNewTasks();
        }

     
        private void ActivityTaskFailed(ActivityTaskDto activityTask, string errorMsg)
        {
            _deployControllerClient.ActivityFailed(activityTask.Id);

            Report(CreateActiveReport(activityTask, errorMsg, ReportStatus.Error));
        }


        private void ActivityTaskSucceded(ActivityTaskDto activityTask)
        {
            _deployControllerClient.ActivityCompleted(activityTask.Id);

            Report(CreateActiveReport(
                                      activityTask,
                                      string.Format("Activity '{0}' completed!", activityTask.ActivityName),
                                      ReportStatus.Information));
        }


        private void Report(ActivityReport ar)
        {
            _deployControllerClient.Report(ar);
        }


        private static ActivityReport CreateActiveReport(ActivityTaskDto activityTask, string msg, ReportStatus reportStatus)
        {
            return new ActivityReport
            {
                DeploymentId = activityTask.DeploymentId,
                ServerRole = activityTask.ServerRole,
                MachineName = System.Environment.MachineName,
                Status = reportStatus,
                ActivityName = activityTask.ActivityName,
                Message = msg,
                ActivityTaskId = activityTask.Id,
                Environment = activityTask.Environment
            };
        }


        public void Stop()
        {
            _deployControllerClient.Disconnect();
        }


        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("AgentService is already disposed");

            if (_deployControllerClient != null)
                _deployControllerClient.Dispose();

            _isDisposed = true;
        }
    }
}