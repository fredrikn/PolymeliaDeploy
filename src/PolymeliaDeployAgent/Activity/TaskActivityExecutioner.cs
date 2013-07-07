using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using PolymeliaDeploy.Workflow;
using System;
using System.Activities;
using System.Activities.Tracking;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolymeliaDeploy.Agent.Activity
{
    public class TaskActivityExecutioner : ITaskActivityExecutioner
    {
        public long? ExecuteTasks(
                                 IEnumerable<ActivityTaskDto> tasks,
                                 Action<ActivityReport> reportAction = null,
                                 Action<ActivityTaskDto> activitySucceededAction = null,
                                 Action<ActivityTaskDto, Exception> activityFailedAction = null)
        {

            //TODO: Remove lastestTaskRunId
            long? lastExecutedTaskId = null;

            foreach (var task in tasks)
            {
                lastExecutedTaskId = task.TaskId;

                if (!ExecuteTask(task, reportAction, activitySucceededAction, activityFailedAction))
                    break;
            }

            return lastExecutedTaskId;
        }


        private bool ExecuteTask(
                                 ActivityTaskDto activityTask,
                                 Action<ActivityReport> reportAction = null,
                                 Action<ActivityTaskDto> activitySucceededAction = null,
                                 Action<ActivityTaskDto, Exception> activityFailedAction = null)
        {
            try
            {
                var agentEnvironment = AgentEnvironment.Current;
                agentEnvironment.CurrentActivityId = activityTask.Id;
                agentEnvironment.TaskId = activityTask.TaskId;
                agentEnvironment.DeployVersion = activityTask.DeployVersion;
                agentEnvironment.Variables = activityTask.Variables;
                agentEnvironment.ServerRole = activityTask.ServerRole;

                InvokeWorkflowActivity(activityTask, reportAction);

                if (activitySucceededAction != null)
                    activitySucceededAction(activityTask);

                return true;
            }
            catch (Exception e)
            {
                activityTask.Status = ActivityStatus.Failed;

                if (activityFailedAction != null)
                    activityFailedAction(activityTask, e);

                Console.WriteLine(e.ToString());

                return false;
            }
        }


        private static void InvokeWorkflowActivity(ActivityTaskDto activityTask, Action<ActivityReport> reportAction = null)
        {

            var parameters = new Dictionary<string, object>
                                 {
                                     { "DeployTaskId", activityTask.TaskId },
                                     { "DeployTaskVersion", activityTask.DeployVersion }
                                 };

            //TODO: The DeployController uses a WorkflowRunner, make it reusable and use it even here.
            var invoker = CreateWorkflowInvoker(activityTask, reportAction);
            invoker.Invoke(parameters);

            activityTask.Status = ActivityStatus.Completed;
        }


        private static WorkflowInvoker CreateWorkflowInvoker(ActivityTaskDto activityTask, Action<ActivityReport> reportAction)
        {
            WorkflowInvoker invoker = new WorkflowInvoker(LoadActivity(activityTask));
            invoker.Extensions.Add(CreateTrackingParticipant(reportAction));
            return invoker;
        }


        private static PolymeliaTrackingParticipant CreateTrackingParticipant(Action<ActivityReport> reportAction)
        {
            const String all = "*";

            return new PolymeliaTrackingParticipant(reportAction)
            {
                TrackingProfile = new TrackingProfile()
                {
                    Name = "CustomTrackingProfile",
                    Queries = 
                    {
                        new CustomTrackingQuery() { Name = all, ActivityName = all }
                    }
                }
            };
        }


        private static System.Activities.Activity LoadActivity(ActivityTaskDto activityTask)
        {
            using (var reader = new StringReader(activityTask.ActivityCode))
                return ActivityXamlServices.Load(reader);
        }
    }
}