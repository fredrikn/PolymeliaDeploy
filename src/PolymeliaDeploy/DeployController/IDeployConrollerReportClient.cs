namespace PolymeliaDeploy.DeployController
{
    using System.Threading.Tasks;

    using PolymeliaDeploy.Data;

    public interface IDeployConrollerReportClient
    {
        Task Report(ActivityReport report);
    }
}