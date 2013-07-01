namespace PolymeliaDeployController.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.IO;

    using PolymeliaDeploy.Data;
    using PolymeliaDeploy;

    internal class WorkflowRunner
    {
        public void Run(MainActivity mainActivity)
        {
            //var parameters = new Dictionary<string, object>
            //                     {
            //                         { "DeployTaskId", mainActivity.Id },
            //                         { "DeployTaskVersion", mainActivity.Version }
            //                     };

            AgentEnvironment.Current.TaskId = mainActivity.Id;
            AgentEnvironment.Current.DeployVersion = mainActivity.Version;

            var wf = ActivityXamlServices.Load(new StringReader(mainActivity.DeployActivity));
            var wfApp = new WorkflowApplication(wf);

            wfApp.Completed = delegate(WorkflowApplicationCompletedEventArgs e)
            {
                if (e.CompletionState == ActivityInstanceState.Faulted)
                {
                    Console.WriteLine("Workflow {0} Terminated.", e.InstanceId);
                    Console.WriteLine("Exception: {0}\n{1}",
                        e.TerminationException.GetType().FullName,
                        e.TerminationException.Message);
                }
                else if (e.CompletionState == ActivityInstanceState.Canceled)
                {
                    Console.WriteLine("Workflow {0} Canceled.", e.InstanceId);
                }
                else
                {
                    Console.WriteLine("Workflow {0} Completed.", e.InstanceId);

                    // Outputs can be retrieved from the Outputs dictionary, 
                    // keyed by argument name. 
                    // Console.WriteLine("The winner is {0}.", e.Outputs["Winner"]);
                }
            };

            wfApp.Aborted = delegate(WorkflowApplicationAbortedEventArgs e)
            {
                Console.WriteLine("Workflow {0} Aborted.", e.InstanceId);
                Console.WriteLine("Exception: {0}\n{1}",
                    e.Reason.GetType().FullName,
                    e.Reason.Message);
            };

            wfApp.Idle = delegate(WorkflowApplicationIdleEventArgs e)
            {
                // Perform any processing that should occur 
                // when a workflow goes idle. If the workflow can persist, 
                // both Idle and PersistableIdle are called in that order.
                Console.WriteLine("Workflow {0} Idle.", e.InstanceId);
            };

            wfApp.PersistableIdle = delegate
                {
                // Instruct the runtime to persist and unload the workflow 
                return PersistableIdleAction.Unload;
            };

            wfApp.Unloaded = e => Console.WriteLine("Workflow {0} Unloaded.", e.InstanceId);

            wfApp.OnUnhandledException = delegate(WorkflowApplicationUnhandledExceptionEventArgs e)
            {
                // Display the unhandled exception.
                Console.WriteLine("OnUnhandledException in Workflow {0}\n{1}",
                    e.InstanceId, e.UnhandledException.Message);

                Console.WriteLine("ExceptionSource: {0} - {1}",
                    e.ExceptionSource.DisplayName, e.ExceptionSourceInstanceId);

                // Instruct the runtime to terminate the workflow. 
                // Other choices are Abort and Cancel 
                return UnhandledExceptionAction.Terminate;
            };

            wfApp.Run();
        }
    }
}