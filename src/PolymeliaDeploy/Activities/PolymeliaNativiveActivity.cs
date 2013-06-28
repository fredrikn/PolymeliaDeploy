namespace PolymeliaDeploy.Activities
{
    using System.Collections.Generic;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    using Variable = System.Activities.Variable;

    public abstract class PolymeliaNativiveActivity : NativeActivity
    {
        private Collection<Variable> _variables = new Collection<Variable>();


        private IReportClient reportRemoteClient;


        [Browsable(false)]
        public Collection<Variable> Variables
        {
            get { return this._variables; }
        }


        public PolymeliaNativiveActivity()
        {
            reportRemoteClient = DeployServices.ReportClient;
        }


        protected override void Execute(NativeActivityContext context)
        {
        }


        protected void ReportWarning(string msg)
        {
            Report(msg, ReportStatus.Warning);
        }


        protected void ReportDebug(string msg)
        {
            Report(msg,ReportStatus.Debug);
        }


        protected void ReportInfo(string msg)
        {
            Report(msg,ReportStatus.Information);
        }


        protected void ReportError(string msg)
        {
            Report(msg, ReportStatus.Error);
        }


        private void Report(string msg, ReportStatus status)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            reportRemoteClient.Report(
                                      AgentEnvironment.TaskId,
                                      AgentEnvironment.ServerRole,
                                      msg,
                                      DisplayName,
                                      status);
        }
    }
}