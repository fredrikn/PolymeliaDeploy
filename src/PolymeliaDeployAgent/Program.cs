using System;
using System.Linq;
using System.ServiceProcess;

namespace PolymeliaDeployAgent
{
    using PolymeliaDeploy;
    using PolymeliaDeploy.Controller;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var service = new Service();

            DeployServices.ReportClient = new ReportRemoteClient();
            DeployServices.ActivityClient = new ActivityRemoteClient();

            if (System.Environment.UserInteractive)
            {
                Console.WriteLine("Start polling deploy controller for tasks...");
                
                using(var poller = new DeployPoller())
                {
                    poller.StartPoll();
                    Console.ReadLine();
                }

                return;
            }

            ServiceBase.Run(service);
        }
    }
}
