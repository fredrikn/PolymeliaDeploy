using System;
using System.ServiceProcess;

namespace PolymeliaDeployAgent
{
    using PolymeliaDeploy;
    using PolymeliaDeploy.Controller;

    internal static class Program
    {
        private static void Main()
        {
            using (var poller = NewPoller())
            {
                if (Environment.UserInteractive)
                {
                    RunInConsole(poller);
                }
                else
                {
                    RunAsService(poller);
                }
            }
        }

        private static void RunInConsole(DeployPoller poller)
        {
            Console.WriteLine("Start polling deploy controller for tasks...");
            poller.StartPoll();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void RunAsService(DeployPoller poller)
        {
            using (var service = new Service(poller))
            {
                ServiceBase.Run(service);
            }
        }

        private static DeployPoller NewPoller()
        {
            // TODO: Dependency injection!
            DeployServices.ReportClient = new ReportRemoteClient();
            DeployServices.ActivityClient = new ActivityRemoteClient();
            return new DeployPoller();
        }
    }
}
