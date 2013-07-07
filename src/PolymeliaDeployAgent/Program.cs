using System;
using System.Configuration;
using System.Globalization;
using System.ServiceProcess;
using PolymeliaDeploy.Controller;
using Microsoft.AspNet.SignalR.Client.Hubs;
using PolymeliaDeploy.Agent.Configuration;
using System.Threading.Tasks;
using PolymeliaDeploy.Agent.Activity;

namespace PolymeliaDeploy.Agent
{
    internal static class Program
    {
        private static void Main()
        {
            using (var agentService = CreateAgentService())
            {
                if (Environment.UserInteractive)
                    RunInConsole(agentService);
                else
                    RunAsService(agentService);
            }
        }


        private static AgentService CreateAgentService()
        {
            var agentConfig = new AgentConfigurationSettings();
            var deployControllerClient = new DeployControllerClient();
            var recordLastTaskId = new FileRecordLatestTask("lasttaskid.dat");
            var taskExecutioner = new TaskActivityExecutioner();

            return new AgentService(
                                    deployControllerClient,
                                    agentConfig,
                                    taskExecutioner,
                                    recordLastTaskId);
        }


        private static void RunInConsole(IAgentService agentService)
        {
            Console.WriteLine("Connect to DeployController");

            agentService.Start();

            Console.WriteLine("Press any key to exit");

            Console.ReadKey();
        }


        private static void RunAsService(IAgentService agentService)
        {
            using (var service = new Service(agentService))
                ServiceBase.Run(service);
        }
    }
}