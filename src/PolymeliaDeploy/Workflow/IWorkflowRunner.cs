namespace PolymeliaDeploy.Workflow
{
    using System;
    using System.Activities.Tracking;
    using System.Collections.Generic;

    public interface IWorkflowRunner
    {
        TrackingParticipant TrackingParticipant { get; set; }

        void Run(
                 string workflowContent,
                 Dictionary<string, object> parameters = null,
                 bool waitForCompletion = false,
                 Action<string> onSucceed = null,
                 Action<string> onFailure = null);
    }
}