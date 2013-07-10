﻿using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xaml;

namespace PolymeliaDeploy.Activities
{
    using System.Activities.Statements;
    using System.Collections.Generic;
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
            XamlServices.Save(xw2, CreateSequence());
            sw.Close();

            PullDeploy(context, s.ToString());
        }


        private void PullDeploy(NativeActivityContext context, string activity)
        {
            var task = new ActivityTask
                           {
                                TaskId = AgentEnvironment.Current.TaskId,
                                ServerRole = context.GetValue(ServerRole),
                                ActivityCode = activity,
                                CreatedBy = Thread.CurrentPrincipal.Identity.Name,
                                ActivityName = DisplayName,
                                DeployVersion = AgentEnvironment.Current.DeployVersion,
                                Status = ActivityStatus.New
                           };

            using (var dbContext = new PolymeliaDeployDbContext())
            {
                dbContext.ActivityTasks.Add(task);
                dbContext.SaveChanges();
            }
        }


        private Start CreateSequence()
        {
            var sequence = new Start
                            {
                                DeployTaskId = AgentEnvironment.Current.TaskId,
                                DeployTaskVersion = AgentEnvironment.Current.DeployVersion,
                                DeployVariables = new InArgument<ICollection<Variable>>(AgentEnvironment.Current.Variables),
                            };

            foreach (var activity in Activities)
                sequence.Activities.Add(activity);

            foreach (var variable in Variables)
                sequence.Variables.Add(variable);

            return sequence;
        }
    }
}