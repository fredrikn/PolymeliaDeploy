﻿using PolymeliaDeploy.Network;

using System.Configuration;

namespace PolymeliaDeploy.Agent.Configuration
{
    public class AgentConfigurationSettings : IAgentConfigurationSettings
    {
        public string DeployControllerUrl
        {
            get
            {
                return GetAppSettings(
                                      "PolymeliaDeployBaseWebUri",
                                      string.Format("http://{0}:12345", IPAddressRetriever.LocalIPAddress()));
            }
        }


        public string ServerRole
        {
            get
            {
                var serverRole = GetAppSettings("ServerRoleName");

                if (string.IsNullOrWhiteSpace(serverRole))
                    throw new ConfigurationErrorsException("The ServerRoleName can't be empty, make sure the ServerRoleName is added to the application configuration file appSettings.");

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
