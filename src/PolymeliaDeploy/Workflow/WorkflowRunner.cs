namespace PolymeliaDeploy.Workflow
{
    using System;
    using System.Activities;
    using System.Activities.Tracking;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    public class WorkflowRunner : IWorkflowRunner
    {
        private TrackingParticipant _trackingParticipant;

        public WorkflowRunner()
        {
            TrackingParticipant = null;
        }


        public WorkflowRunner(TrackingParticipant trackinParticipant)
        {
            if (trackinParticipant == null) throw new ArgumentNullException("trackinParticipant");

            TrackingParticipant = trackinParticipant;
        }


        public TrackingParticipant TrackingParticipant
        {
            get { return _trackingParticipant; }
            set { _trackingParticipant = value; }
        }


        public void Run(
                        string workflowContent,
                        Dictionary<string, object> parameters = null,
                        bool waitForCompletion = false,
                        Action<string> onSucceed = null,
                        Action<string> onFailure = null)
        {
            var completion = false;

            var wfApp = CreateWorkflowApplication(workflowContent, parameters);

            if (_trackingParticipant != null)
                wfApp.Extensions.Add(TrackingParticipant);

            wfApp.Completed = delegate(WorkflowApplicationCompletedEventArgs e)
            {
                OnCompleted(e, onSucceed, onFailure);
                completion = true;
            };

            wfApp.Aborted = delegate(WorkflowApplicationAbortedEventArgs e)
            {
                OnAborted(e);
                completion = true;
            };

            wfApp.Idle = e => Console.WriteLine("Workflow {0} Idle.", e.InstanceId);
            wfApp.PersistableIdle = delegate { return PersistableIdleAction.Unload; };

            wfApp.Unloaded = e => Console.WriteLine("Workflow {0} Unloaded.", e.InstanceId);

            wfApp.OnUnhandledException = delegate(WorkflowApplicationUnhandledExceptionEventArgs e)
            {
                OnUnhandledException(e, onFailure);

                completion = true;

                return UnhandledExceptionAction.Terminate;
            };

            wfApp.Run();

            if (!waitForCompletion) return;

            while (!completion)
            {
                Thread.Sleep(0);
            }
        }


        private static void OnUnhandledException(
                                                 WorkflowApplicationUnhandledExceptionEventArgs e,
                                                 Action<string> onFailure = null)
        {
            var msg = string.Format("OnUnhandledException in Workflow {0}\n{1}\nExceptionSource: {2} - {3}",
                                    e.InstanceId,
                                    e.UnhandledException.Message,
                                    e.ExceptionSource.DisplayName,
                                    e.ExceptionSourceInstanceId);

            Console.WriteLine(msg);

            if (onFailure != null)
                onFailure(msg);
        }

        private static void OnAborted(WorkflowApplicationAbortedEventArgs e)
        {
            Console.WriteLine("Workflow {0} Aborted.", e.InstanceId);
            Console.WriteLine("Exception: {0}\n{1}", e.Reason.GetType().FullName, e.Reason.Message);
        }

        private static void OnCompleted(
                                        WorkflowApplicationCompletedEventArgs e,
                                        Action<string> onSucceed = null,
                                        Action<string> onFailure = null)
        {
            if (e.CompletionState == ActivityInstanceState.Faulted)
            {
                Console.WriteLine("Workflow {0} Terminated.", e.InstanceId);
                
                
                var msg = string.Format(
                                        "Exception: {0}\n{1}",
                                        e.TerminationException.GetType().FullName,
                                        e.TerminationException.Message);
                Console.WriteLine(msg);

                if (onFailure != null)
                    onFailure(msg);
            }

            else if (e.CompletionState == ActivityInstanceState.Canceled)
            {
                Console.WriteLine("Workflow {0} Canceled.", e.InstanceId);

                if (onFailure != null)
                    onFailure("Canceled");
            }
            else
            {
                Console.WriteLine("Workflow {0} Completed.", e.InstanceId);

                // Outputs can be retrieved from the Outputs dictionary, 
                // keyed by argument name. 
                // Console.WriteLine("The winner is {0}.", e.Outputs["Winner"]);

                if (onSucceed != null)
                    onSucceed("");
            }
        }

        private static WorkflowApplication CreateWorkflowApplication(
                                                                     string workflowContent,
                                                                     IDictionary<string, object> parameters)
        {
            var wf = ActivityXamlServices.Load(new StringReader(workflowContent));

            WorkflowApplication wfApp;

            if (parameters != null && parameters.Any())
                wfApp = new WorkflowApplication(wf, parameters);
            else
                wfApp = new WorkflowApplication(wf);
            
            return wfApp;
        }
    }
}