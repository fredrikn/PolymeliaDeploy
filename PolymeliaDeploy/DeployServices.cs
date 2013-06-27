namespace PolymeliaDeploy
{
    using PolymeliaDeploy.Controller;

    public static class DeployServices
    {
        public static IActivityClient ActivityClient { get; set; }
        public static IReportClient ReportClient { get; set; }
    }
}