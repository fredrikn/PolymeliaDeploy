using System.Activities;
using System.ComponentModel;

namespace PolymeliaDeploy.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.ServiceProcess;

    using PolymeliaDeploy.Process;

    public enum StartupOption
    {
        Boot,
        System,
        Auto,
        Demand,
        Disabled
    }


    [DisplayName("Install Windows Service"), Category("Actions")]
    public class InstallWindowsService : PolymeliaNativiveActivity
    {
        private IProcessRunner _processerRunner;

        [RequiredArgument]
        public InArgument<string> ServiceDisplayName { get; set; }

        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        [RequiredArgument]
        public InArgument<string> BinaryPath { get; set; }

        public InArgument<string> DependsOn { get; set; }

        public InArgument<StartupOption> StartupOption { get; set; }

        [DescriptionAttribute("The name of the account running the service, default is 'LocalSystem'")]
        public InArgument<string> AccountName { get; set; }

        public InArgument<string> Password { get; set; }


        public InstallWindowsService()
        {
            StartupOption = new InArgument<StartupOption>(Activities.StartupOption.Auto);
            _processerRunner = new ProcessRunner();
        }


        protected override void Execute(NativeActivityContext context)
        {
            var serviceName = context.GetValue(ServiceName);
            var serviceDisplayName = context.GetValue(ServiceDisplayName);
            var startupOption = context.GetValue(StartupOption);
            var binaryPath = context.GetValue(BinaryPath);
            var accountName = context.GetValue(AccountName);
            var password = context.GetValue(Password);
            var serviceDependencies = context.GetValue(DependsOn);

            if (DoesServiceExist(serviceName))
            {
                StopService(serviceName);

                ConfigService(
                              serviceName, 
                              serviceDisplayName,
                              accountName,
                              password,
                              binaryPath,
                              startupOption,
                              serviceDependencies);
            }
            else
            {
                CreateService(
                              serviceName,
                              serviceDisplayName,
                              binaryPath,
                              startupOption,
                              accountName, 
                              password,
                              serviceDependencies);
            }

            StartService(serviceName);
        }

        private void ConfigService(
                                    string serviceName,
                                    string displayName,
                                    string accountName,
                                    string password,
                                    string binaryPath,
                                    StartupOption startupOption,
                                    string serviceDependencies)
        {
            var args = new ProcessArgument()
                           {
                               CommandToRun = "sc.exe",
                               CommandArguments =
                                   ConfigServerCommandArgs(
                                                           serviceName,
                                                           displayName,
                                                           accountName,
                                                           password,
                                                           binaryPath,
                                                           startupOption,
                                                           serviceDependencies),
                           };

            var exitCode = _processerRunner.RunSilience(args, ReportInfo, ReportError);

            if (exitCode != 0)
            {
                throw new ApplicationException(
                    string.Format("Can't create service '{0}', error code '{1}'", serviceName, exitCode));
            }
        }


        private void CreateService(
                                    string serviceName,
                                    string serviceDisplayName,
                                    string binaryPath,
                                    StartupOption startupOption,
                                    string accountName,
                                    string password,
                                    string serviceDependencies)
        {
            var args = new ProcessArgument
            {
                CommandToRun = "sc.exe",
                CommandArguments =
                    CreateServerCommandArgs(
                                            serviceName,
                                            serviceDisplayName,
                                            accountName,
                                            password,
                                            binaryPath,
                                            startupOption,
                                            serviceDependencies),
            };

            var exitCode = _processerRunner.RunSilience(args, ReportInfo, ReportError);

            if (exitCode != 0)
            {
                throw new ApplicationException(
                    string.Format("Can't create service '{0}', error code '{1}'", serviceName, exitCode));
            }
        }


        private static Collection<string> ConfigServerCommandArgs(
                                                string serviceName,
                                                string displayName,
                                                string accountName,
                                                string password,
                                                string binaryPath,
                                                StartupOption startupOption,
                                                string serviceDependencies)
        {
            var commandArgs = new Collection<string>();

            commandArgs.Add("config \"" + serviceName + "\"");

            AddStandardServiceCommandArguments(
                                                displayName,
                                                accountName,
                                                password,
                                                binaryPath,
                                                startupOption,
                                                serviceDependencies,
                                                commandArgs);

            return commandArgs;
        }


        private static Collection<string> CreateServerCommandArgs(
                                                string serviceName,
                                                string displayName,
                                                string accountName,
                                                string password,
                                                string binaryPath,
                                                StartupOption startupOption,
                                                string serviceDependencies)
        {
            var commandArgs = new Collection<string>();

            commandArgs.Add("create \"" + serviceName + "\"");

            AddStandardServiceCommandArguments(
                                                displayName,
                                                accountName,
                                                password,
                                                binaryPath,
                                                startupOption,
                                                serviceDependencies,
                                                commandArgs);

            return commandArgs;
        }


        private static void AddStandardServiceCommandArguments(
            string displayName,
            string accountName,
            string password,
            string binaryPath,
            StartupOption startupOption,
            string serviceDependencies,
            Collection<string> commandArgs)
        {
            if (!string.IsNullOrWhiteSpace(binaryPath))
                commandArgs.Add("binpath= \"" + binaryPath + "\"");

            if (!string.IsNullOrWhiteSpace(accountName))
                commandArgs.Add("obj= " + accountName);

            if (!string.IsNullOrWhiteSpace(password))
                commandArgs.Add("password= " + password);

            if (!string.IsNullOrWhiteSpace(displayName))
                commandArgs.Add("displayName= \"" + displayName + "\"");

            commandArgs.Add("start= " + startupOption.ToString().ToLower());

            if (!string.IsNullOrWhiteSpace(serviceDependencies))
                commandArgs.Add("depend= " + serviceDependencies);
        }


        private void StopService(string serviceName, int timeoutMilliseconds = 30000)
        {
            try
            {
                var service = new ServiceController(serviceName, Environment.MachineName);

                if (service.Status == ServiceControllerStatus.Stopped)
                    return;

                ReportInfo(string.Format("Stopping service '{0}'", serviceName));

                service.Stop();

                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(timeoutMilliseconds));

                ReportInfo(string.Format("Service '{0}' stopped", serviceName));
            }
            catch (Exception e)
            {
                ReportError(string.Format("Can't Stop the service '{0}', Error '{1}'", serviceName, e));
                throw;
            }
        }


        public void StartService(string serviceName, int timeoutMilliseconds = 30000)
        {
            try
            {
                var service = new ServiceController(serviceName, Environment.MachineName);

                if (service.Status == ServiceControllerStatus.Running)
                    return;

                ReportInfo(string.Format("Starting service '{0}'", serviceName));

                service.Start();

                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(timeoutMilliseconds));

                ReportInfo(string.Format("Service '{0}' started", serviceName));

            }
            catch (Exception e)
            {
                ReportError(string.Format("Service '{0}' can't be started, Error '{1}'", serviceName, e));
                throw;
            }
        }


        private bool DoesServiceExist(string serviceName)
        {
            var services = ServiceController.GetServices(Environment.MachineName);
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);
            return service != null;
        }
    }
}