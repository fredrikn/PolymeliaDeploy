namespace PolymeliaDeploy.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    using Ionic.Zip;

    using Microsoft.Web.XmlTransform;

    using NuGet;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;
    using PolymeliaDeploy.Powershell;


    [DisplayName("Deploy NuGet Package"), Category("Actions")]
    public sealed class InstallNuGetPackage : PolymeliaNativiveActivity
    {
        [RequiredArgument]
        public InArgument<string> PackageName { get; set; }

        public InArgument<string> Version { get; set; }

        [RequiredArgument]
        public InArgument<string> NuGetServerPath { get; set; }

        public InArgument<string> NuGetServerApiKey { get; set; }

        public InArgument<string> NuGetPackageDropFolder { get; set; }

        [RequiredArgument]
        [Description("Which environment configuration file that should be used in a transformation.")]
        public InArgument<string> ConfigurationEnvironment { get; set; }

        private string _packageVersion;

        private string _destinationFolder;

        private ReportRemoteClient reportRemoteClient;


        public InstallNuGetPackage()
        {
            reportRemoteClient = new ReportRemoteClient();
        }


        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(NativeActivityContext context)
        {
            SetDestinationFolder(context);

            if (!Directory.Exists(_destinationFolder))
                Directory.CreateDirectory(_destinationFolder);

            var packageSource = RetrieveNuGetPackage(context);

            DecompressAndRemovePacakgeFile(packageSource);

            var packageDirectory = new FileInfo(packageSource).Directory.ToString();

            TransformationConfigurationFiles(packageDirectory, context);

            SetupPackage(context);
        }


        private void SetDestinationFolder(NativeActivityContext context)
        {
            _destinationFolder = context.GetValue(NuGetPackageDropFolder);

            if (string.IsNullOrWhiteSpace(_destinationFolder))
                _destinationFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DropFolder");
        }


        private void SetupPackage(NativeActivityContext context)
        {
            try
            {
                ExecuteDeployPowerShellScript("Init.ps1", context);
                ExecuteDeployPowerShellScript("Install.ps1", context);
                ExecuteDeployPowerShellScript("AfterInstall.ps1", context);
            }
            catch (Exception)
            {
                ExecuteDeployPowerShellScript("Rollback.ps1", context);
                throw;
            }
        }


        private void ExecuteDeployPowerShellScript(string scriptName, NativeActivityContext context, bool removeScript = true)
        {
            var deployPowerShellScript = Path.Combine(_destinationFolder, scriptName);

            if (!File.Exists(deployPowerShellScript))
                return;

            var variables = CreatePowerShellScriptVariables(context);

            try
            {
                LogInformation("Start to call PowerShell scriptName '{0}' for package '{1}'", scriptName, context.GetValue(PackageName));

                var output = PowerShellInvoker.RunPowerShellScript(
                                                                   string.Format(".\\{0}", scriptName),
                                                                   _destinationFolder,
                                                                   variables);

                LogInformation("Succeed to call PowerShell scriptName '{0}' for package '{1}' with the output: {2}{3}", scriptName, context.GetValue(this.PackageName), System.Environment.NewLine, output);
            }
            catch (Exception e)
            {
                LogError("Failed to call PowerShell scriptName '{0}' for package '{1}' with the output: {2}{3}", scriptName, context.GetValue(this.PackageName), System.Environment.NewLine, e.Message);
                throw;
            }
            finally
            {
                if (removeScript)
                    File.Delete(deployPowerShellScript);
            }
        }

        private Dictionary<string, object> CreatePowerShellScriptVariables(NativeActivityContext context)
        {
            var variables = new Dictionary<string, object>(DeployVariables)
                                {
                                    { "PolymeliaVersion", Version },
                                    { "PolymeliaEnvironment", context.GetValue(this.ConfigurationEnvironment) },
                                    { "PolymeliaMachineName", System.Environment.MachineName },
                                    { "PolymeliaTaskId", TaskId },
                                    { "PolymeliaDestinationPath", _destinationFolder },
                                    { "PolymeliaPackageName", context.GetValue(PackageName) },
                                    { "PolymeliaPackageVersion", _packageVersion }
                                };

            foreach (var variable in this.Variables)
                variables.Add(variable.Name, variable.Default);

            return variables;
        }

        private void DecompressAndRemovePacakgeFile(string fileToUnzip)
        {
            var fileInfo = new FileInfo(fileToUnzip);

            using (var zip = ZipFile.Read(fileToUnzip))
            {
                foreach (var e in zip)
                    e.Extract(fileInfo.DirectoryName, ExtractExistingFileAction.OverwriteSilently);
            }

            RemovePackageFile(fileToUnzip);
        }


        private static void RemovePackageFile(string packageSource)
        {
            try
            {
                File.Delete(packageSource);
            }
            catch
            {
            }
        }


        private string RetrieveNuGetPackage(NativeActivityContext context)
        {
            var packageRepository = new DataServicePackageRepository(new Uri(context.GetValue(this.NuGetServerPath)));

            DataServicePackage nugetPackage;

            if (!string.IsNullOrWhiteSpace(context.GetValue(this.Version)))
            {
                nugetPackage = packageRepository.FindPackage(
                                                            context.GetValue(this.PackageName),
                                                            new SemanticVersion(context.GetValue(this.Version))) as DataServicePackage;
            }
            else
            {
                nugetPackage = packageRepository.FindPackage(context.GetValue(this.PackageName)) as DataServicePackage;
            }

            if (nugetPackage == null)
            {
                throw new ApplicationException(
                    string.Format(
                        "Can't find package '{0}' version '{1}' in the artifact repository on URI '{2}'",
                        context.GetValue(this.PackageName),
                        context.GetValue(this.Version),
                        context.GetValue(this.NuGetServerPath)));
            }

            var packageSource = this.DownloadPackage(nugetPackage, context);

            this._packageVersion = nugetPackage.Version;

            return packageSource;
        }


        private string DownloadPackage(DataServicePackage nugetPackage, NativeActivityContext context)
        {
            var packageDestinationPath = Path.Combine(this._destinationFolder, nugetPackage.GetFullName() + ".nupkg");

            var fileStream = new FileStream(packageDestinationPath, FileMode.Create);

            var packageDownloader = new PackageDownloader();

            packageDownloader.DownloadPackage(
                                              new Uri(nugetPackage.DownloadUrl.AbsoluteUri),
                                              nugetPackage,
                                              fileStream);

            fileStream.Flush();
            fileStream.Close();

            return packageDestinationPath;
        }

        private void TransformationConfigurationFiles(string packageDirectory, NativeActivityContext context)
        {
            var files = GetAllApplicationConfigFiles(packageDirectory, context);

            if (!string.IsNullOrWhiteSpace(context.GetValue(ConfigurationEnvironment)))
                TransformEnvironmentConfigurationFiles(files, context.GetValue(ConfigurationEnvironment), context );

            ReplaceConfigValuesWithDeployVariables(files, context);
        }


        private void TransformEnvironmentConfigurationFiles(IEnumerable<FileInfo> files, string environment, NativeActivityContext context)
        {
            foreach (var environmentConfigFile in files.Where(file => file.Name.ToLower().Contains("." + environment + ".")))
            {
                var rootApplicationConfigFilePath = GetRootApplicationConfigFilePath(environmentConfigFile, environment);

                if (!File.Exists(rootApplicationConfigFilePath))
                {
                    LogInformation("There was no root configuration file '{0}' for the package '{1}', no transformation will take place.", rootApplicationConfigFilePath, context.GetValue(this.PackageName));
                    continue;
                }

                TransformConfigFiles(rootApplicationConfigFilePath, environmentConfigFile, context);
            }
        }


        private void ReplaceConfigValuesWithDeployVariables(IEnumerable<FileInfo> files, NativeActivityContext context)
        {
            foreach (var rootConfigurationFile in files.Where(
                                                              file => file.Name.ToLower().Contains(".exe.config") || 
                                                              file.Name.ToLower().Contains("web.config")))
            {
                var rootApplicationConfigFile = new XmlDocument();
                
                rootApplicationConfigFile.Load(rootConfigurationFile.FullName);

                ReplaceConfigValuesWithVariables(rootApplicationConfigFile, context);
                
                rootApplicationConfigFile.Save(rootConfigurationFile.FullName);
            }
        }


        private void TransformConfigFiles(string rootApplicationConfigFilePath, FileInfo environmentConfigFile, NativeActivityContext context)
        {
            var rootApplicationConfigFile = new XmlDocument();
            rootApplicationConfigFile.Load(rootApplicationConfigFilePath);

            this.LogInformation("Start to transform config '{0}' with config '{1}' for the package '{2}'", environmentConfigFile.FullName, rootApplicationConfigFilePath, context.GetValue(this.PackageName));

            this.TransformConfig(environmentConfigFile, rootApplicationConfigFile);

            this.LogInformation("Succeed to transform config '{0}' with config '{1}' for the package '{2}'", environmentConfigFile.FullName, rootApplicationConfigFilePath, context.GetValue(this.PackageName));

            rootApplicationConfigFile.Save(rootApplicationConfigFilePath);
        }


        private void TransformConfig(FileInfo environmentConfigFile, XmlDocument doc)
        {
            var transform = new XmlTransformation(environmentConfigFile.FullName);
            transform.Apply(doc);
        }


        private IEnumerable<FileInfo> GetAllApplicationConfigFiles(string packageDirectory, NativeActivityContext context)
        {
            var directory = new DirectoryInfo(packageDirectory);

            LogInformation("Start to locate configuration files at path '{0}' for the package '{1}'", packageDirectory, context.GetValue(this.PackageName));

            var files = directory.GetFiles(string.Format("*.config"), SearchOption.AllDirectories);

            if (files == null || files.Length == 0)
                this.LogInformation("No configuration files at path '{0}' for the package '{1}' was found", packageDirectory, context.GetValue(this.PackageName));
            else
                this.LogInformation("Succeeded to find {0} configuration files at path '{1}' for the package '{2}' was found", files.Length, packageDirectory, context.GetValue(this.PackageName));

            return files;
        }


        private static string GetRootApplicationConfigFilePath(FileInfo fileInfo, string environment)
        {
            return fileInfo.FullName.Replace(string.Format(".{0}.config", environment), ".config");
        }


        private void ReplaceConfigValuesWithVariables(XmlDocument doc, NativeActivityContext context)
        {
            //TODO: Replace with markers/substitute $(key
            
            this.LogInformation("Start to replace configuration file values for the package '{0}'", context.GetValue(this.PackageName));

            this.UpdateConfigSection("//appSettings/add", "key", "value", doc);
            this.UpdateConfigSection("//connectionStrings/add", "name", "connectionString", doc);
            this.UpdateConfigSection("//system.serviceModel/client/endpoint", "name", "address", doc);
            this.UpdateConfigSection("//system.serviceModel/services/service/endpoint", "name", "address", doc);

            this.LogInformation("Succeed to replace configuration file values for the package '{0}'", context.GetValue(this.PackageName));
        }


        private void UpdateConfigSection(string path, string attributeNameToFind, string attributeNameToChange, XmlDocument doc)
        {
            var xmlNodeList = doc.SelectNodes(path);

            if (xmlNodeList == null)
                return;

            foreach (XmlElement node in xmlNodeList)
            {
                foreach (var variable in this.DeployVariables)
                {
                    if (node.Attributes[attributeNameToFind].Value.ToLower() != variable.Key.ToLower())
                        continue;

                    node.SetAttribute(attributeNameToChange, variable.Value.ToString());

                    this.LogInformation("Changed the '{0}' attribute of the '{1}' with the attribute {2} to value '{3}'", attributeNameToChange, path, attributeNameToFind, variable.Value);
                }
            }
        }

        private async void LogInformation(string message, params object[] arguments)
        {
            var msg = string.Format(message, arguments);

            await reportRemoteClient.Report(TaskId, AgentEnvironment.ServerRole, msg, DisplayName);

            Console.WriteLine(msg);
        }


        private async void LogError(string message, params object[] arguments)
        {
            var msg = string.Format(message, arguments);

            await reportRemoteClient.Report(TaskId, AgentEnvironment.ServerRole , msg, DisplayName, ReportStatus.Error);

            Console.WriteLine(msg);
        }
    }
}