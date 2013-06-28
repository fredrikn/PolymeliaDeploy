namespace PolymeliaDeploy.Activities
{
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;

    [Designer("System.Activities.Core.Presentation.SequenceDesigner, System.Activities.Core.Presentation")]
    [ContentProperty("Activities")]
    public sealed class Start : PolymeliaNativiveActivity
    {
        private Collection<Activity> _activities = new Collection<Activity>();

        private Variable<int> lastIndexHint;

        private CompletionCallback onChildComplete;


        [DependsOn("Variables")]
        [Browsable(false)]
        public Collection<Activity> Activities
        {
            get { return this._activities; }
        }


        public Start()
        {
            this.lastIndexHint = new Variable<int>();
            this.onChildComplete = this.InternalExecute;
        }


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetChildrenCollection(Activities);
            metadata.SetVariablesCollection(Variables);
            metadata.SetArgumentsCollection(metadata.GetArgumentsWithReflection());
            metadata.AddImplementationVariable(lastIndexHint);
        }


        protected override void Execute(NativeActivityContext context)
        {
            if (this._activities == null || this.Activities.Count <= 0)
                return;

            var activity = this.Activities[0];
            context.ScheduleActivity(activity, this.onChildComplete);
        }


        private void InternalExecute(NativeActivityContext context, ActivityInstance completedInstance)
        {
            var index1 = this.lastIndexHint.Get(context);

            if (index1 >= this.Activities.Count || this.Activities[index1] != completedInstance.Activity)
                index1 = this.Activities.IndexOf(completedInstance.Activity);

            var index2 = index1 + 1;
            if (index2 == this.Activities.Count)
                return;

            var activity = this.Activities[index2];
            context.ScheduleActivity(activity, this.onChildComplete);
            this.lastIndexHint.Set(context, index2);
        }
    }
}