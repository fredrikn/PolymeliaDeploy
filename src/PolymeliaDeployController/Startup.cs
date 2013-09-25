namespace PolymeliaDeployController
{
    using Microsoft.AspNet.SignalR;
    using Owin;

    using PolymeliaDeploy.Data.Repositories;
    using PolymeliaDeploy.Security;

    using PolymeliaDeployController.Configuration;
    using PolymeliaDeployController.Hub;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HubConfiguration { EnableCrossDomain = true };

            //TODO: Add a IoC Container
            GlobalHost.DependencyResolver.Register(
                                                   typeof(DeployControllerHub),
                                                   () => new DeployControllerHub(
                                                                               new ReportRepository(),
                                                                               new ActivityRepository(),
                                                                               new AgentRepository(),
                                                                               new ControllerConfigurationSettings(),
                                                                               new TokenManagement()));

            app.MapHubs(config);

            app.UseNancy();
        }
    }
}
