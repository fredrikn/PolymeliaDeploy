namespace PolymeliaDeploy.Activities
{
    using System.Activities;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;

    using Variable = PolymeliaDeploy.Data.Variable;

    [Designer("System.Activities.Core.Presentation.SequenceDesigner, System.Activities.Core.Presentation")]
    [ContentProperty("Activities")]
    public sealed class Start : PolymeliaNativiveActivity
    {
        private Collection<Activity> _activities = new Collection<Activity>();

        private Variable<int> _lastIndexHint;

        private CompletionCallback _onChildComplete;

        private InArgument<ICollection<Variable>> _deployVariables = new InArgument<ICollection<Variable>>();

        [Browsable(false)]
        public InArgument<long> DeployTaskId { get; set; }

        [Browsable(false)]
        public InArgument<string> DeployTaskVersion { get; set; }


        [Browsable(false)]
        public InArgument<ICollection<Variable>> DeployVariables
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


        public Start()
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
            AgentEnvironment.Current.TaskId = DeployTaskId.Get(context);
            AgentEnvironment.Current.DeployVersion = DeployTaskVersion.Get(context);
            AgentEnvironment.Current.Variables = DeployVariables.Get(context);

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