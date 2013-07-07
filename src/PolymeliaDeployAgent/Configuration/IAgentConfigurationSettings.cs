using System;
namespace PolymeliaDeploy.Agent.Configuration
{
    public interface IAgentConfigurationSettings
    {
        string ServerRole { get; }
        string DeployControllerUrl { get; }
    }
}
