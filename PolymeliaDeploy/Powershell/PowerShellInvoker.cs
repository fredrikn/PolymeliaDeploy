namespace PolymeliaDeploy.Powershell
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public static class PowerShellInvoker
    {

        public static string RunPowerShellScript(string script, string workerPath, Dictionary<string, object> variables)
        {
            return Execute(script, workerPath, variables);
        }


        private static string Execute(string script, string workerDirectory, Dictionary<string, object> variables)
        {
            var bootstrapFile = CreatePowerShellBootstrapFile(script, workerDirectory, variables);

            try
            {
                var commandArguments = CreateProcessCommandArguments(bootstrapFile);
                return RunPowershellProcess(script, commandArguments, workerDirectory);
            }
            finally
            {
                File.Delete(bootstrapFile);
            }
        }


        private static StringBuilder CreateProcessCommandArguments(string bootstrapFile)
        {
            var commandArguments = new StringBuilder();

            commandArguments.Append("-NonInteractive ");
            commandArguments.Append("-NoLogo ");
            commandArguments.Append("-ExecutionPolicy ByPass ");
            commandArguments.AppendFormat("-File \"{0}\"", bootstrapFile);

            return commandArguments;
        }


        private static string CreatePowerShellBootstrapFile(string script, string workerPath, Dictionary<string, object> variables)
        {
            var bootstrapFile = Path.Combine(workerPath, "TempDeploy." + Guid.NewGuid() + ".ps1");

            using (var writer = new StreamWriter(bootstrapFile))
            {
                AddVariablesToScript(variables, writer);

                writer.WriteLine();

                writer.WriteLine("## Invoke Script:");
                writer.WriteLine(script);

                writer.Flush();
            }

            return bootstrapFile;
        }


        private static void AddVariablesToScript(Dictionary<string, object> variables, StreamWriter writer)
        {
            writer.WriteLine("## Polymelia variables:");

            AddSingleLineVariable(variables, writer);
            AddPolymeliaVariableDicrionary(variables, writer);
        }


        private static void AddSingleLineVariable(Dictionary<string, object> variables, StreamWriter writer)
        {
            foreach (var variable in variables)
            {
                WriteVariableLine(
                    TransformToValidPowerShellVariableName(variable.Key), CreatePowerShellEncondingText(variable.Value), writer);
            }
        }


        private static void AddPolymeliaVariableDicrionary(Dictionary<string, object> variables, StreamWriter writer)
        {
            var polymeliaVariables = new StringBuilder("$PolymeliaVariables = @{");

            foreach (var variable in variables)
            {
                var value = CreatePowerShellEncondingText(variable.Value);
                polymeliaVariables.Append(string.Format("\"{0}\" = {1};", variable.Key, value));
            }

            polymeliaVariables.Append("}");

            writer.WriteLine(polymeliaVariables.ToString());
        }


        private static string CreatePowerShellEncondingText(object value)
        {
            return String.Format(
                "[System.Text.Encoding]::Unicode.GetString( [Convert]::FromBase64String( \"{0}\" ) )",
                Convert.ToBase64String(Encoding.Unicode.GetBytes(value == null ? string.Empty : value.ToString())));
        }


        private static string TransformToValidPowerShellVariableName(string variableKey)
        {
            if (string.IsNullOrWhiteSpace(variableKey))
                throw new ArgumentNullException("variableKey");

            return variableKey.Replace(".", string.Empty)
                              .Replace("-", string.Empty);
        }

        private static void WriteVariableLine(string variableName, string value, StreamWriter writer)
        {
            writer.WriteLine("${0} = {1}", variableName, value);
        }


        private static string RunPowershellProcess(string originalScript, StringBuilder commandArguments, string workerDirectory)
        {
            var powerShellExe = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";

            var processInfo = SetupProcessStartInfo(commandArguments, powerShellExe, workerDirectory);

            //TODO: Make sure this process can report on each error and out put to the depoy controller.

            var process = Process.Start(processInfo);

            var error = process.StandardError.ReadToEnd();
            var output = process.StandardOutput.ReadToEnd();

            Console.WriteLine(error);
            Console.WriteLine(output);

            process.WaitForExit();

            if (process.ExitCode != 0 || !string.IsNullOrEmpty(error))
                throw new ApplicationException(string.Format("Error while executing PowerShell script '{0}' - {1}", originalScript, error));

            return output;
        }


        private static ProcessStartInfo SetupProcessStartInfo(StringBuilder commandArguments, string powerShellExe, string workerDirectory)
        {
            return new ProcessStartInfo(powerShellExe, commandArguments.ToString())
            {
                WorkingDirectory = workerDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        }
    }
}