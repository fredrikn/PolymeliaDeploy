namespace PolymeliaDeployController
{
    using Microsoft.AspNet.SignalR;
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Turn cross domain on 
            var config = new HubConfiguration { EnableCrossDomain = true };

            app.MapHubs(config);

            app.UseNancy();
        }
    }
}
