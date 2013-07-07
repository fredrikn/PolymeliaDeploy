namespace PolymeliaDeploy.Activities
{
    using System.Activities;
    using System.ComponentModel;

    using PolymeliaDeploy.Management;
    using System.Activities.Tracking;
    using System.Diagnostics;

    public enum VirtualMachineState
    {
        Start,
        Stop
    }

    [DisplayName("Start/Stop Virtual Machine"), Category("Actions")]
    public class VirtualMachine : PolymeliaNativiveActivity
    {
        [RequiredArgument]
        [Description("The name of the VirtualMachin")]
        public InArgument<string> VirtualMachineName { get; set; }


        [RequiredArgument]
        [Description("The name of the VirtualMachin")]
        public InArgument<VirtualMachineState> SetState { get; set; }


        HyperVManager _hyperVManger;

        public VirtualMachine()
        {
            _hyperVManger = new HyperVManager();
        }


        protected override void Execute(NativeActivityContext context)
        {
            if (context.GetValue(SetState) == VirtualMachineState.Start)
            {
                ReportInfo(string.Format("Starting virutal machine '{0}'", context.GetValue(VirtualMachineName)), context);

                //TODO: Change the ReportInfo and ReportError for the HyperVManager
                _hyperVManger.StartMachine(context.GetValue(VirtualMachineName), ReportInfo, ReportError);

                ReportInfo(string.Format("Virtual machine '{0}' is started", context.GetValue(VirtualMachineName)), context);

            }
            else if (context.GetValue(SetState) == VirtualMachineState.Stop)
            {
                ReportInfo(string.Format("Stopping virutal machine '{0}'", context.GetValue(VirtualMachineName)), context);

                //TODO: Change the ReportInfo and ReportError for the HyperVManager
                _hyperVManger.StopMachine(context.GetValue(VirtualMachineName), ReportInfo, ReportError);

                ReportInfo(string.Format("Virtual machine '{0}' is stopped", context.GetValue(VirtualMachineName)), context);
            }
        }
    }
}
