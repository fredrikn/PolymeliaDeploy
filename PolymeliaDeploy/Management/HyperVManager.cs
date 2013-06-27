using System;

namespace PolymeliaDeploy.Management
{
    using System.Management;

    public class HyperVManager
    {
        public void StartMachine(
                                 string vmName,
                                 Action<string> infortmationReport,
                                 Action<string> errorReport)
        {
            RequestStateChange(vmName, "start", infortmationReport, errorReport);
        }

        public void StopMachine(
                         string vmName,
                         Action<string> infortmationReport,
                         Action<string> errorReport)
        {
            RequestStateChange(vmName, "stop", infortmationReport, errorReport);
        }


        private void RequestStateChange(
                                       string vmName,
                                       string action,
                                       Action<string> infortmationReport,
                                       Action<string> errorReport)
        {
            var serverPath = string.Format(@"\\{0}\root\virtualization", Environment.MachineName);

            var scope = new ManagementScope(serverPath, null);
            var vm = Utility.GetTargetComputer(vmName, scope);

            if (null == vm)
                throw new ArgumentException(string.Format("The virtual machine '{0}' could not be found.", vmName));

            var inParams = vm.GetMethodParameters("RequestStateChange");

            const int Enabled = 2;
            const int Disabled = 3;
            
            var enabledState = (UInt16)vm["EnabledState"];

            switch (action.ToLower())
            {
                case "start":
                    if (enabledState == EnabledState.Enabled)
                    {
                        infortmationReport(string.Format("The Virtual Machine '{0}' is already started.", vmName));
                        return;
                    }
                    inParams["RequestedState"] = Enabled;
                    break;
                case "stop":
                    if (enabledState == EnabledState.Disabled)
                    {
                        infortmationReport(string.Format("The Virtual Machine '{0}' is already stopped.", vmName));
                        return;
                    }
                    inParams["RequestedState"] = Disabled;
                    break;
                default:
                    throw new Exception("Wrong action is specified");
            }

            var outParams = vm.InvokeMethod("RequestStateChange", inParams, null);

            switch ((UInt32)outParams["ReturnValue"])
            {
                case ReturnCode.Started:
                    if (Utility.JobCompleted(outParams, scope, errorReport))
                    {
                        if (infortmationReport != null)
                            infortmationReport(string.Format("{0} state was changed successfully.", vmName));

                        Console.WriteLine("{0} state was changed successfully.", vmName);
                    }
                    else
                    {
                        if (errorReport != null)
                            errorReport(string.Format("Failed to change virtual system state for '{0}'", vmName));

                        throw new ApplicationException(string.Format("Failed to change virtual system state for '{0}'", vmName));
                    }
                    break;
                case ReturnCode.Completed:
                    {
                        if (infortmationReport != null)
                            infortmationReport(string.Format("{0} state was changed successfully.", vmName));

                        Console.WriteLine("{0} state was changed successfully.", vmName);
                        break;
                    }
                default:
                    {
                        if (errorReport != null)
                            errorReport(string.Format("Change virtual system state failed with error {0}", outParams["ReturnValue"]));

                        throw new ApplicationException(string.Format("Change virtual system state failed with error {0}", outParams["ReturnValue"]));
                        break;
                    }
            }
        }
    }
}
