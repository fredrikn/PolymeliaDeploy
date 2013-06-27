using System;
using System.ServiceProcess;

namespace PolymeliaDeployController
{
    public partial class Service : ServiceBase
    {
        private IDisposable _webHost;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _webHost = Program.CreateServiceHost();
        }

        protected override void OnStop()
        {
            if (_webHost != null)
                _webHost.Dispose();
        }
    }
}
