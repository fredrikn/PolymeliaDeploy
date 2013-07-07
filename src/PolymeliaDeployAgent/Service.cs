using PolymeliaDeploy.Agent.Configuration;
using System;
using System.ServiceProcess;

namespace PolymeliaDeploy.Agent
{
    public partial class Service : ServiceBase
    {
        private IAgentService _agentService;

        public Service(IAgentService agentService)
        {
            if (agentService == null) throw new ArgumentNullException("agentService");

            _agentService = agentService;
            
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _agentService.Start();
        }


        protected override void OnStop()
        {
            _agentService.Stop();
        }


        protected override void OnShutdown()
        {
            _agentService.Dispose();
            base.OnShutdown();
        }
    }
}
