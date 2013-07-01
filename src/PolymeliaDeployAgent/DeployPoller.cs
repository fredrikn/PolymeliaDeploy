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

namespace PolymeliaDeploy.Agent
{
    public class DeployPoller : IDisposable
    {
        private readonly IReportClient _reportClient;
        private readonly IActivityClient _activityClient;
        private readonly IVariableClient _variableClient;
        private readonly IRecordLatestTask _latestTask;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private bool _isDesposed;

        public DeployPoller(
            IReportClient reportClient,
            IActivityClient activityClient,
            IVariableClient variableClient,
            IRecordLatestTask latestTask)
        {
            if (reportClient == null) throw new ArgumentNullException("reportClient");
            if (activityClient == null) throw new ArgumentNullException("activityClient");
            if (variableClient == null) throw new ArgumentNullException("variableClient");
            if (latestTask == null) throw new ArgumentNullException("latestTask");
            _reportClient = reportClient;
            _activityClient = activityClient;
            _variableClient = variableClient;
            _latestTask = latestTask;
        }

        public string ServerRoleName { get; set; }
        public TimeSpan PollerInterval { get; set; }

        public void StartPoll()
        {
            if (_isRunning)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            
            Task.Factory.StartNew(async Delegate =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var latestTaskRunId = _latestTask.GetValue();

                    var tasks = _activityClient.GetActivityTasksFromDeployController(latestTaskRunId, ServerRoleName).ToArray();

                    if (tasks.Any())
                        ExecuteTasks(latestTaskRunId, tasks);

                    await Task.Delay(PollerInterval, cancellationToken);
                }
            }, cancellationToken);
        }


        public void StopPoll()
        {
            _cancellationTokenSource.Cancel();
            _isRunning = false;
        }


        public void RestartPoll()
        {
            if (_isRunning)
                return;

            StartPoll();
        }


        private void ExecuteTasks(long latestTaskRunId, IEnumerable<ActivityTaskDto> tasks)
        {
            var lastExecutedTaskId = latestTaskRunId;

            foreach (var task in tasks)
            {
                lastExecutedTaskId = task.TaskId;

                if (!ExecuteTask(task))
                    break;
            }

            if (latestTaskRunId != lastExecutedTaskId)
                _latestTask.SetValue(lastExecutedTaskId);
        }

        private bool ExecuteTask(ActivityTaskDto activityTask)
        {
            try
            {
                AgentEnvironment.CurrentActivityId = activityTask.Id;
                AgentEnvironment.TaskId = activityTask.TaskId;
                AgentEnvironment.DeployVersion = activityTask.DeployVersion;
                AgentEnvironment.Variables = activityTask.Variables;
                AgentEnvironment.ServerRole = ServerRoleName;

                //var parameters = new Dictionary<string, object>
                //                 {
                //                     { "DeployTaskId", activityTask.TaskId },
                //                     { "DeployTaskVersion", activityTask.DeployVersion }
                //                 };

                WorkflowInvoker.Invoke(LoadActivity(activityTask));

                activityTask.Status = ActivityStatus.Completed;

                _activityClient.UpdateActivityTaskStatus(activityTask);

                _reportClient.Report(
                               activityTask.TaskId,
                               activityTask.ServerRole,
                               string.Format("Activity '{0}' completed!", activityTask.ActivityName),
                               activityTask.ActivityName);

                return true;
            }
            catch (Exception e)
            {
                activityTask.Status = ActivityStatus.Failed;

                _activityClient.UpdateActivityTaskStatus(activityTask);

                _reportClient.Report(
                       activityTask.TaskId,
                       activityTask.ServerRole,
                       e.ToString(),
                       activityTask.ActivityName,
                       ReportStatus.Error);
                
                Console.WriteLine(e.ToString());

                return false;
            }
        }

        private static Activity LoadActivity(ActivityTaskDto activityTask)
        {
            using (var reader = new StringReader(activityTask.ActivityCode))
                return ActivityXamlServices.Load(reader);
        }

        public void Dispose()
        {
            if (_isDesposed)
                throw new ObjectDisposedException("DeployPoller is already disposed");

            StopPoll();

            _isRunning = false;
            _isDesposed = true;
            _reportClient.Dispose();
            _activityClient.Dispose();
        }
    }
}