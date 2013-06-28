﻿using System;
using System.Linq;
using System.ServiceProcess;

namespace PolymeliaDeployController
{
    using System.Configuration;
    using System.Data.Entity;

    using Microsoft.Owin.Hosting;

    using PolymeliaDeploy;
    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;
    using PolymeliaDeploy.Network;

    static class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer<PolymeliaDeployDbContext>(null);
            DeployServices.ReportClient = new ReportLocalClient();

            var service = new Service();

            if (Environment.UserInteractive)
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
            var localIpAddress = IPAddressRetriever.LocalIPAddress();

            var portNumber = ConfigurationManager.AppSettings["ControllerPort"];

            var controllUri = string.Format("http://{0}:{1}", localIpAddress, portNumber);

            return WebApplication.Start<Startup>(controllUri);
        }
    }
}