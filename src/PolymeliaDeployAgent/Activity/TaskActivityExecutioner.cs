using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using PolymeliaDeploy.Workflow;
using System;
using System.Collections.Generic;

namespace PolymeliaDeploy.Agent.Activity
{
    public class TaskActivityExecutioner : ITaskActivityExecutioner
    {
        private readonly IWorkflowRunner _workflowRunner;
        
        public TaskActivityExecutioner(IWorkflowRunner workflowRunner)
        {
            if (workflowRunner == null) throw new ArgumentNullException("workflowRunner");

            _workflowRunner = workflowRunner;
        }


        public void ExecuteTasks(
                                 IEnumerable<ActivityTaskDto> tasks,
                                 Action<ActivityTaskDto> activitySucceededAction = null,
                                 Action<ActivityTaskDto, string> activityFailedAction = null)
        {
            foreach (var task in tasks)
            {
                if (!ExecuteTask(task, activitySucceededAction, activityFailedAction))
                    break;
            }
        }


        private bool ExecuteTask(
                                 ActivityTaskDto activityTask,
                                 Action<ActivityTaskDto> activitySucceededAction = null,
                                 Action<ActivityTaskDto, string> activityFailedAction = null)
        {
            try
            {
                var agentEnvironment = PolymeliaActivityContext.Current;
                agentEnvironment.CurrentActivityId = activityTask.Id;
                agentEnvironment.ServerRole = activityTask.ServerRole;

                return InvokeWorkflowActivity(
                                              activityTask,
                                              activitySucceededAction,
                                              activityFailedAction);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }


        private bool InvokeWorkflowActivity(ActivityTaskDto activityTask,
                                            Action<ActivityTaskDto> activitySucceededAction = null,
                                            Action<ActivityTaskDto, string> activityFailedAction = null)
        {
            var succeeded = true;

            _workflowRunner.Run(
                                activityTask.ActivityCode,
                                waitForCompletion: true,
                                onSucceed: _ =>
                                {
                                    succeeded = true;
                                    activityTask.Status = ActivityStatus.Completed;
                                    
                                    if (activitySucceededAction != null)
                                        activitySucceededAction(activityTask);
                                },
                                onFailure: errorMsg =>
                                {
                                    activityTask.Status = ActivityStatus.Failed;
                                    succeeded = false;

                                    if (activityFailedAction != null)
                                        activityFailedAction(activityTask, errorMsg);

                                    Console.WriteLine(errorMsg);
                                });

            return succeeded;
        }
    }
}