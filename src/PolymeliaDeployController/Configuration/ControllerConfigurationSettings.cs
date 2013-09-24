namespace PolymeliaDeployController.Configuration
{
    using System.Configuration;

    public class ControllerConfigurationSettings : IControllerConfigurationSettings
    {
  
        public string ControllerHostUrl
        {
            get
            {
                var serverRole = GetAppSettings("ControllerHostUrl");

                if (string.IsNullOrWhiteSpace(serverRole))
                    throw new ConfigurationErrorsException("The ControllerHostUrl can't be empty, make sure the ControllerHostUrl is added to the application configuration file appSettings.");

                return serverRole;
            }
        }


        public string ControllerKey
        {
            get
            {
                var controllerKey = GetAppSettings("ControllerKey");

                if (string.IsNullOrWhiteSpace(controllerKey))
                    throw new ConfigurationErrorsException("The ControllerKey can't be empty, make sure the ControllerKey is added to the application configuration file appSettings.");

                return controllerKey;
            }
        }


        private string GetAppSettings(string key, string defaultValue = null)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }
    }
}
