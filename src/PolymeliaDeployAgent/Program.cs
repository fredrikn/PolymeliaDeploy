using System;
using System.ServiceProcess;

using PolymeliaDeploy.Agent.Configuration;
using PolymeliaDeploy.Agent.Activity;

namespace PolymeliaDeploy.Agent
{
    using PolymeliaDeploy.Workflow;

    using Environment = System.Environment;

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
            var workflowRunner = new WorkflowRunner(new PolymeliaTrackingParticipantFactory().CreateTrackingParticipant(deployControllerClient));
            var taskExecutioner = new TaskActivityExecutioner(workflowRunner);

            return new AgentService(
                                    deployControllerClient,
                                    agentConfig,
                                    taskExecutioner);
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