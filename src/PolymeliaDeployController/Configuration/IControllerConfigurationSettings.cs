namespace PolymeliaDeployController.Configuration
{
    public interface IControllerConfigurationSettings
    {
        string ControllerHostUrl { get; }
        string ControllerKey { get; }
    }
}