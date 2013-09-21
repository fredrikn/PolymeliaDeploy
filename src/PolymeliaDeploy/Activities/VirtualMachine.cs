namespace PolymeliaDeploy.Activities
{
    using System.Activities;
    using System.ComponentModel;

    using PolymeliaDeploy.Management;

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

        readonly HyperVManager _hyperVManger;

        public VirtualMachine()
        {
            _hyperVManger = new HyperVManager();
        }


        protected override void Execute(NativeActivityContext context)
        {
            if (context.GetValue(SetState) == VirtualMachineState.Start)
            {
                ReportInfo(string.Format("Starting virtual machine '{0}'", context.GetValue(VirtualMachineName)), context);

                _hyperVManger.StartMachine(context.GetValue(VirtualMachineName), msg => ReportInfo(msg, context));

                ReportInfo(string.Format("Virtual machine '{0}' is started", context.GetValue(VirtualMachineName)), context);
            }
            else if (context.GetValue(SetState) == VirtualMachineState.Stop)
            {
                ReportInfo(string.Format("Stopping virtual machine '{0}'", context.GetValue(VirtualMachineName)), context);

                _hyperVManger.StopMachine(context.GetValue(VirtualMachineName), msg => ReportInfo(msg, context));

                ReportInfo(string.Format("Virtual machine '{0}' is stopped", context.GetValue(VirtualMachineName)), context);
            }
        }
    }
}
