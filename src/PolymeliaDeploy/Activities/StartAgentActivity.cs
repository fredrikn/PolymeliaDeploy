namespace PolymeliaDeploy.Activities
{
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;

    using PolymeliaDeploy.Activities.Attributes;
    using PolymeliaDeploy.Data;

    [HideActivity]
    public sealed class StartAgentActivity : PolymeliaNativiveActivity
    {
        private readonly Collection<Activity> _activities = new Collection<Activity>();

        private readonly Variable<int> _lastIndexHint;

        private readonly CompletionCallback _onChildComplete;

        private Collection<DeployVariable> _deployVariables = new Collection<DeployVariable>();

        [Browsable(false)]
        public InArgument<long> DeploymentId { get; set; }

        [Browsable(false)]
        public InArgument<string> DeployTaskVersion { get; set; }

        [Browsable(false)]
        public InArgument<string> ServerRole { get; set; }

        [Browsable(false)]
        public InArgument<string> Environment { get; set; }


        [Browsable(false)]
        public Collection<DeployVariable> DeployVariables
        {
            get { return _deployVariables; }
            set { _deployVariables = value; }
        }


        [DependsOn("Variables")]
        [Browsable(false)]
        public Collection<Activity> Activities
        {
            get { return _activities; }
        }


        public StartAgentActivity()
        {
            _lastIndexHint = new Variable<int>();
            _onChildComplete = InternalExecute;
        }


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetChildrenCollection(Activities);
            metadata.SetVariablesCollection(Variables);
            metadata.SetArgumentsCollection(metadata.GetArgumentsWithReflection());
            metadata.AddImplementationVariable(_lastIndexHint);
        }


        protected override void Execute(NativeActivityContext context)
        {
            PolymeliaActivityContext.Current.DeploymentId = DeploymentId.Get(context);
            PolymeliaActivityContext.Current.DeployVersion = DeployTaskVersion.Get(context);
            PolymeliaActivityContext.Current.Variables = DeployVariables;
            PolymeliaActivityContext.Current.ServerRole = ServerRole.Get(context);
            PolymeliaActivityContext.Current.Environment = Environment.Get(context);

            if (_activities == null || Activities.Count <= 0)
                return;

            var activity = Activities[0];
            context.ScheduleActivity(activity, _onChildComplete);
        }


        private void InternalExecute(NativeActivityContext context, ActivityInstance completedInstance)
        {
            var index1 = _lastIndexHint.Get(context);

            if (index1 >= Activities.Count || Activities[index1] != completedInstance.Activity)
                index1 = Activities.IndexOf(completedInstance.Activity);

            var index2 = index1 + 1;
            if (index2 == Activities.Count)
                return;

            var activity = Activities[index2];
            context.ScheduleActivity(activity, _onChildComplete);
            _lastIndexHint.Set(context, index2);
        }
    }
}