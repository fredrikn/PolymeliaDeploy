﻿using PolymeliaDeploy.Agent.Activity;
using PolymeliaDeploy.Agent.Configuration;
using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using PolymeliaDeploy.DeployController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolymeliaDeploy.Agent
{
    public class AgentService : IAgentService
    {
        private bool _isDisposed;

        private IDeployControllerClient _deployControllerClient;
        private IAgentConfigurationSettings _agentConfig;
        private ITaskActivityExecutioner _taskActivityExecutioner;
        private IRecordLatestTask _recordLatestTask;

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
            _deployControllerClient.Connect(
                                            _agentConfig.DeployControllerUrl,
                                            _agentConfig.ServerRole)
                                   .Wait();

            ExecuteActivityTaskIfAvailable();
        }


        private void ExecuteActivityTaskIfAvailable()
        {
            var lastTaskId = _recordLatestTask.GetValue();

            var tasks = _deployControllerClient.GetActivityTasks(lastTaskId).Result;

            if (tasks.Any())
            {
                var lastestExecutedTaskId = _taskActivityExecutioner.ExecuteTasks(
                                                                                  tasks,
                                                                                  Report,
                                                                                  ActivityTaskSucceded,
                                                                                  ActivityTaskFailed);

                if (lastestExecutedTaskId.HasValue)
                    _recordLatestTask.SetValue(lastestExecutedTaskId.Value);
            }
        }


        private void ActivityTaskFailed(ActivityTaskDto activityTask, Exception e)
        {
            _deployControllerClient.UpdateActivityTaskStatus(activityTask.Id, ActivityStatus.Failed);

            Report(new ActivityReport
                {
                    TaskId = activityTask.TaskId,
                    ServerRole = activityTask.ServerRole,
                    MachineName = System.Environment.MachineName,
                    Status = ReportStatus.Error,
                    ActivityName = activityTask.ActivityName,
                    Message = e.ToString(),
                });
        }


        private void ActivityTaskSucceded(ActivityTaskDto activityTask)
        {
            _deployControllerClient.UpdateActivityTaskStatus(activityTask.Id, ActivityStatus.Completed);

           Report(new ActivityReport
                {
                    TaskId = activityTask.TaskId,
                    ServerRole = activityTask.ServerRole,
                    MachineName = System.Environment.MachineName,
                    Status = ReportStatus.Information,
                    ActivityName = activityTask.ActivityName,
                    Message = string.Format("Activity '{0}' completed!", activityTask.ActivityName)
                });
        }


        private void Report(ActivityReport ar)
        {
            _deployControllerClient.Report(ar);
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