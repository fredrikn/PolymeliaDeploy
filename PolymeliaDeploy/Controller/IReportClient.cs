namespace PolymeliaDeploy.Controller
{
    using System;
    using System.Collections.Generic;

    using PolymeliaDeploy.Data;
    using System.Threading.Tasks;

    public interface IReportClient: IDisposable
    {
        Task Report(
            long taskId,
            string serverRole,
            string message,
            string activityName,
            ReportStatus reportStatus = ReportStatus.Information);

        Task<IEnumerable<ActivityReport>> GetReports(long taskId, long? fromLatestTaskId);
    }
}