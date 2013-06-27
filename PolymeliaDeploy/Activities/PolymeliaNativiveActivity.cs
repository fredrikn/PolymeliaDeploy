namespace PolymeliaDeploy.Activities
{
    using System.Collections.Generic;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    using Variable = System.Activities.Variable;

    public abstract class PolymeliaNativiveActivity : NativeActivity, IUseDeployVariables
    {
        private Collection<Variable> _variables = new Collection<Variable>();
        private Dictionary<string, object> _deployVariables = new Dictionary<string, object>();

        private IReportClient reportRemoteClient;

        [Browsable(false)]
        public long TaskId { get; set; }


        [Browsable(false)]
        public string DeployVersion { get; set; }


        [Browsable(false)]
        public Dictionary<string, object> DeployVariables
        {
            get { return this._deployVariables; }
        }


        [Browsable(false)]
        public Collection<Variable> Variables
        {
            get { return this._variables; }
        }


        public PolymeliaNativiveActivity()
        {
            this.reportRemoteClient = DeployServices.ReportClient;
        }


        protected override void Execute(NativeActivityContext context)
        {
        }


        protected void AddDeployableVariablesToActivitySupportingIt(
                                                                    Collection<Activity> activities, 
                                                                    NativeActivityContext context)
        {
            foreach (var activity in activities)
            {
                var activitySupporingDeployVaraibles = activity as IUseDeployVariables;

                if (activitySupporingDeployVaraibles == null)
                    continue;

                activitySupporingDeployVaraibles.TaskId = TaskId;
                activitySupporingDeployVaraibles.DeployVersion = DeployVersion;

                foreach (var variable in this.Variables)
                {
                    if (activitySupporingDeployVaraibles.DeployVariables.ContainsKey(variable.Name))
                        activitySupporingDeployVaraibles.DeployVariables[variable.Name] = variable.Default;
                    else
                        activitySupporingDeployVaraibles.DeployVariables.Add(variable.Name, variable.Default);
                }

                foreach (var variable in this.DeployVariables)
                {
                    if (activitySupporingDeployVaraibles.DeployVariables.ContainsKey(variable.Key))
                        activitySupporingDeployVaraibles.DeployVariables[variable.Key] = variable.Value;
                    else
                        activitySupporingDeployVaraibles.DeployVariables.Add(variable.Key, variable.Value);
                }
            }
        }


        protected void ReportWarning(string msg)
        {
            this.reportRemoteClient.Report(TaskId, AgentEnvironment.ServerRole, msg, this.DisplayName, ReportStatus.Warning);
        }

        protected void ReportDebug(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            this.reportRemoteClient.Report(TaskId, AgentEnvironment.ServerRole, msg, this.DisplayName, ReportStatus.Debug);
        }

        protected void ReportInfo(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            this.reportRemoteClient.Report(TaskId, AgentEnvironment.ServerRole, msg, this.DisplayName, ReportStatus.Information);
        }


        protected void ReportError(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            this.reportRemoteClient.Report(TaskId, AgentEnvironment.ServerRole, msg, this.DisplayName, ReportStatus.Error);
        }
    }
}