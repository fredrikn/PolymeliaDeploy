using System;
using System.Configuration;
using System.Globalization;
using System.ServiceProcess;
using PolymeliaDeploy.Controller;

namespace PolymeliaDeploy.Agent
{
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
            var reportClient = new ReportRemoteClient();
            var activityClient = new ActivityRemoteClient();
            var variableClient = new VariableRemoteClient();
            var latestTask = new FileRecordLatestTask("lastTask.dat");

            DeployServices.ReportClient = reportClient;
            DeployServices.ActivityClient = activityClient;

            return new DeployPoller(reportClient, activityClient, variableClient, latestTask)
                {
                    PollerInterval = TimeSpan.FromSeconds(ConfigurationHelper.GetInt32("TaskPollerTime", @default: 1)),
                    ServerRoleName = ConfigurationHelper.GetString("ServerRoleName")
                };
        }

        private static class ConfigurationHelper
        {
            public static int GetInt32(string key, int @default = 0)
            {
                var value = GetString(key);
                if (value == null) return @default;

                int result;
                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                    return @default;

                return result;
            }

            public static string GetString(string key, string @default = null)
            {
                return ConfigurationManager.AppSettings[key] ?? @default;
            }
        }
    }
}
