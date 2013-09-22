using PolymeliaDeploy.Agent.Activity;
using PolymeliaDeploy.Agent.Configuration;
using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using PolymeliaDeploy.DeployController;
using System;

namespace PolymeliaDeploy.Agent
{
    using System.Collections.Generic;

    public class AgentService : IAgentService
    {
        private bool _isDisposed;

        private readonly IDeployControllerClient _deployControllerClient;
        private readonly IAgentConfigurationSettings _agentConfig;
        private readonly ITaskActivityExecutioner _taskActivityExecutioner;
        private readonly IRecordLatestTask _recordLatestTask;

        private AgentStatus _agentStatus = AgentStatus.Ready;

        public AgentService(
                            IDeployControllerClient deployControllerClient,
                            IAgentConfigurationSettings agentConfig,
                            ITaskActivityExecutioner taskActivityExecutioner,
                            IRecordLatestTask recordLatestTask)
        {
            if (deployControllerClient == null) throw new ArgumentNullException("deployControllerClient");
            if (agentConfig == null) throw new ArgumentNullException("agentConfig");
            if (taskActivityExecutioner == null) throw new ArgumentNullException("taskActivityExecutioner");
            if (recordLatestTask == null) throw new ArgumentNullException("recordLatestTask");

            _deployControllerClient = deployControllerClient;
            _agentConfig = agentConfig;
            _taskActivityExecutioner = taskActivityExecutioner;
            _recordLatestTask = recordLatestTask;
        }


        public void Start()
        {
            _deployControllerClient.OnConnected = OnConnectedToController();
            _deployControllerClient.OnRunActivity = RunActivities;

            _deployControllerClient.Connect(
                                            _agentConfig.DeployControllerUrl,
                                            _agentConfig.ServerRole);
        }


        private Action OnConnectedToController()
        {
            return () =>
            {
                if (_agentStatus == AgentStatus.Ready)
                    _deployControllerClient.AgentIsReadyForNewTasks(_recordLatestTask.GetValue());
            };
        }


        private void RunActivities(IEnumerable<ActivityTaskDto> activities)
        {
            _agentStatus = AgentStatus.InProgress;

            var lastestExecutedTaskId = _taskActivityExecutioner.ExecuteTasks(
                                                                              activities,
                                                                              ActivityTaskSucceded,
                                                                              ActivityTaskFailed);

            if (lastestExecutedTaskId.HasValue)
                _recordLatestTask.SetValue(lastestExecutedTaskId.Value);

            _agentStatus = AgentStatus.Ready;
            _deployControllerClient.AgentIsReadyForNewTasks(_recordLatestTask.GetValue());
        }

     
        private void ActivityTaskFailed(ActivityTaskDto activityTask, string errorMsg)
        {
            _deployControllerClient.UpdateActivityTaskStatus(activityTask.Id, ActivityStatus.Failed);

            Report(CreateActiveReport(activityTask, errorMsg, ReportStatus.Error));
        }


        private void ActivityTaskSucceded(ActivityTaskDto activityTask)
        {
            _deployControllerClient.UpdateActivityTaskStatus(activityTask.Id, ActivityStatus.Completed);

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
                TaskId = activityTask.TaskId,
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