namespace PolymeliaDeployController
{
    using Microsoft.AspNet.SignalR;
    using Owin;

    using PolymeliaDeploy.Data.Repositories;

    using PolymeliaDeployController.Hub;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Turn cross domain on 
            var config = new HubConfiguration { EnableCrossDomain = true };

            GlobalHost.DependencyResolver.Register(
                                                   typeof(DeployControllerHub),
                                                   () => new DeployControllerHub(new ReportRepository()));

            app.MapHubs(config);

            app.UseNancy();
        }
    }
}
