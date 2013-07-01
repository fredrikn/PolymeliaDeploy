using System;
using System.ServiceProcess;

namespace PolymeliaDeploy.Agent
{
    public partial class Service : ServiceBase
    {
        private readonly DeployPoller _deployPoller;

        public Service(DeployPoller deployPoller)
        {
            if (deployPoller == null) throw new ArgumentNullException("deployPoller");
            _deployPoller = deployPoller;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _deployPoller.StartPoll();
        }

        protected override void OnStop()
        {
            _deployPoller.StopPoll();
        }
    }
}
