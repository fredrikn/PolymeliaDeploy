namespace PolymeliaDeploy.Process
{
    using System;

    public interface IProcessRunner
    {
        int RunSilience(
            ProcessArgument processArgument,
            Action<string> reportOutput,
            Action<string> reportErrorOutput);
    }
}