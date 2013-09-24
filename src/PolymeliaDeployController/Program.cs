using System;
using System.ServiceProcess;

namespace PolymeliaDeployController
{
    using System.Configuration;
    using System.Data.Entity;

    using Microsoft.Owin.Hosting;

    using PolymeliaDeploy;
    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    static class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer<PolymeliaDeployDbContext>(null);
            DeployServices.ReportClient = new ReportLocalClient();

            var service = new Service();

            if (System.Environment.UserInteractive)
            {
                using (CreateServiceHost())
                {
                    Console.WriteLine("Started");
                    Console.ReadKey();
                    Console.WriteLine("Stopping");
                    return;
                }
            }

            ServiceBase.Run(service);
        }


        internal static IDisposable CreateServiceHost()
        {
            var controllUri = ConfigurationManager.AppSettings["ControllerHostUrl"];

            return WebApplication.Start<Startup>(controllUri);
        }
    }
}
