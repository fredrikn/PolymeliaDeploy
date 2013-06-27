namespace PolymeliaDeploy.Process
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    public class ProcessRunner : IProcessRunner
    {
        public int RunSilience(
                                ProcessArgument processArgument,
                                Action<string> reportOutput,
                                Action<string> reportErrorOutput)
        {
            if (processArgument == null)
                throw new ArgumentNullException("processArgument");

            var processInfo = SetupSilienceProcessStartInfo(processArgument);

            var process = Process.Start(processInfo);

            if (processArgument.MaxWaitTime.HasValue)
                process.WaitForExit(processArgument.MaxWaitTime.Value);
            else
                process.WaitForExit();

            var error = process.StandardError.ReadToEnd();
            var output = process.StandardOutput.ReadToEnd();

            if ((process.ExitCode != 0 || !string.IsNullOrEmpty(error)) && reportErrorOutput != null)
                reportErrorOutput(error);

            if (reportOutput != null)
                reportOutput(output);

            return process.ExitCode;
        }


        private static string CreateProcessCommandArguments(IEnumerable<string> commandArgs)
        {
            if (commandArgs == null || !commandArgs.Any())
                return string.Empty;

            var commandArguments = new StringBuilder();

            foreach (var commandArg in commandArgs)
                commandArguments.Append(commandArg).Append(" ");

            return commandArguments.ToString();
        }


        private static ProcessStartInfo SetupSilienceProcessStartInfo(ProcessArgument processArgument)
        {
            if (string.IsNullOrWhiteSpace(processArgument.CommandToRun))
                throw new ArgumentNullException("processArgument's CommandToRun is empty");

            return new ProcessStartInfo(processArgument.CommandToRun, CreateProcessCommandArguments(processArgument.CommandArguments))
            {
                WorkingDirectory = processArgument.WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        }
    }
}