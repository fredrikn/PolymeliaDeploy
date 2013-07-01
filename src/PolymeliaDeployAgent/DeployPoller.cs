using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Configuration;
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

        private const string LAST_TASK_FILE = "lastTask.dat";

        private CancellationTokenSource cancellationTokenSource;

        private bool _isRunning = false;
        private bool _isDesposed = false;

        public DeployPoller(
            IReportClient reportClient,
            IActivityClient activityClient,
            IVariableClient variableClient)
        {
            _reportClient = reportClient;
            _activityClient = activityClient;
            _variableClient = variableClient;
        }

        public string ServerRoleName { get; set; }
        public TimeSpan PollerInterval { get; set; }

        public void StartPoll()
        {
            if (_isRunning)
                return;

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            
            Task.Factory.StartNew(async Delegate =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var latestTaskRunId = GetLatestCompletedTask();

                    var tasks = _activityClient.GetActivityTasksFromDeployController(latestTaskRunId, ServerRoleName).ToArray();

                    if (tasks.Any())
                        ExecuteTasks(latestTaskRunId, tasks);

                    await Task.Delay(PollerInterval, cancellationToken);
                }
            }, cancellationToken);
        }


        public void StopPoll()
        {
            cancellationTokenSource.Cancel();
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
                UpdateLastExecutedTaskId(lastExecutedTaskId);
        }


        private static void UpdateLastExecutedTaskId(long lastExecutedTaskId)
        {
            File.WriteAllText(LAST_TASK_FILE, lastExecutedTaskId.ToString());
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

        private long GetLatestCompletedTask()
        {
            if (!File.Exists(LAST_TASK_FILE))
                return 0;

            var taskId = File.ReadAllText(LAST_TASK_FILE);

            long latestTaskId = 0;

            return long.TryParse(taskId, out latestTaskId) ? latestTaskId : 0;
        }


        public void Dispose()
        {
            if (_isDesposed)
                throw new ObjectDisposedException("DeployPoller is already disposed");

            StopPoll();

            _isRunning = false;
            _isDesposed = true;
            this._reportClient.Dispose();
            this._activityClient.Dispose();
        }
    }
}