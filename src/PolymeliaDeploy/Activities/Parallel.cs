namespace PolymeliaDeploy.Activities
{
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;

    [Designer("System.Activities.Core.Presentation.ParallelDesigner, System.Activities.Core.Presentation")]
    [ContentProperty("Branches")]
    public sealed class Parallel : PolymeliaNativiveActivity
    {
        private CompletionCallback<bool> onConditionComplete;
        private Collection<Activity> _branches = new Collection<Activity>();
        private Variable<bool> hasCompleted;

        [DependsOn("Variables")]
        [DefaultValue(null)]
        public Activity<bool> CompletionCondition { get; set; }


        [DependsOn("CompletionCondition")]
        [Browsable(false)]
        public Collection<Activity> Branches
        {
            get { return _branches; }
        }


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var children = new Collection<Activity>();

            foreach (var activity in Branches)
                children.Add(activity);

            if (CompletionCondition != null)
                children.Add(CompletionCondition);

            metadata.SetChildrenCollection(children);
            metadata.SetVariablesCollection(Variables);

            if (CompletionCondition == null)
                return;

            if (hasCompleted == null)
                hasCompleted = new Variable<bool>("hasCompletedVar");

            metadata.AddImplementationVariable(hasCompleted);
        }


        protected override void Execute(NativeActivityContext context)
        {
            if (this._branches == null || this.Branches.Count == 0)
                return;

            AddDeployableVariablesToActivitySupportingIt(Branches, context);

            var onCompleted = new CompletionCallback(this.OnBranchComplete);

            for (var index = this.Branches.Count - 1; index >= 0; --index)
                context.ScheduleActivity(this.Branches[index], onCompleted);
        }


        protected override void Cancel(NativeActivityContext context)
        {
            if (this.CompletionCondition == null)
                base.Cancel(context);
            else
                context.CancelChildren();
        }


        private void OnBranchComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (CompletionCondition == null || hasCompleted.Get(context))
                return;

            if (completedInstance.State != ActivityInstanceState.Closed && context.IsCancellationRequested)
            {
                context.MarkCanceled();
                hasCompleted.Set(context, true);
            }
            else
            {
                if (onConditionComplete == null)
                    onConditionComplete = OnConditionComplete;

                context.ScheduleActivity(CompletionCondition, onConditionComplete);
            }
        }


        private void OnConditionComplete(NativeActivityContext context, ActivityInstance completedInstance, bool result)
        {
            if (!result)
                return;

            context.CancelChildren();

            this.hasCompleted.Set(context, true);
        }
    }
}