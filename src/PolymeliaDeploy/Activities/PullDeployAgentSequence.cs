using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xaml;

namespace PolymeliaDeploy.Activities
{
    using System.Threading;
    using PolymeliaDeploy.Data;
    using PolymeliaDeploy.Workflow;

    [Designer("System.Activities.Core.Presentation.SequenceDesigner, System.Activities.Core.Presentation")]
    [DisplayName("Deploy to Agent")]
    public class PullDeployAgentSequence : PolymeliaNativiveActivity
    {
        private readonly Collection<Activity> _activities = new Collection<Activity>();


        [RequiredArgument]
        [DescriptionAttribute("The name of the server role to deploy to")]
        public InArgument<string> ServerRole { get; set; }

 
        [Browsable(false)]
        public Collection<Activity> Activities
        {
            get { return _activities; }
        }


        protected override void Execute(NativeActivityContext context)
        {
            var s = new StringBuilder();
            var sw = new StringWriter(s);

            var xw2 = ActivityXamlServices.CreateBuilderWriter(new IgnorableXamlXmlWriter(sw, new XamlSchemaContext()));
            XamlServices.Save(xw2, CreateSequence(context));
            sw.Close();

            PullDeploy(context, s.ToString());
        }


        private void PullDeploy(NativeActivityContext context, string activity)
        {
            var task = new ActivityTask
                           {
                                TaskId = PolymeliaActivityContext.Current.TaskId,
                                Environment = PolymeliaActivityContext.Current.Environment,
                                ServerRole = ServerRole.Get(context),
                                ActivityCode = activity,
                                CreatedBy = Thread.CurrentPrincipal.Identity.Name,
                                ActivityName = DisplayName,
                                DeployVersion = PolymeliaActivityContext.Current.DeployVersion,
                                Status = ActivityStatus.New
                           };

            using (var dbContext = new PolymeliaDeployDbContext())
            {
                dbContext.ActivityTasks.Add(task);
                dbContext.SaveChanges();
            }
        }


        private StartAgentActivity CreateSequence(NativeActivityContext context)
        {
            var sequence = new StartAgentActivity
                            {
                                DeployTaskId = PolymeliaActivityContext.Current.TaskId,
                                DeployTaskVersion = PolymeliaActivityContext.Current.DeployVersion,
                                DeployVariables = PolymeliaActivityContext.Current.Variables,
                                ServerRole = ServerRole.Get(context),
                                Environment = PolymeliaActivityContext.Current.Environment
                            };

            foreach (var activity in Activities)
                sequence.Activities.Add(activity);

            foreach (var variable in Variables)
                sequence.Variables.Add(variable);

            return sequence;
        }
    }
}