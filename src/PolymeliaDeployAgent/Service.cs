using System.ServiceProcess;

namespace PolymeliaDeployAgent
{
    public partial class Service : ServiceBase
    {
        DeployPoller _deployPoller = new DeployPoller();

        public Service()
        {
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
