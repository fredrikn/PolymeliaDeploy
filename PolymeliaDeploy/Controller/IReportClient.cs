namespace PolymeliaDeploy.Controller
{
    using System;
    using System.Collections.Generic;

    using PolymeliaDeploy.Data;

    public interface IReportClient: IDisposable
    {
        void Report(
            long taskId,
            string serverRole,
            string message,
            string activityName,
            ReportStatus reportStatus = ReportStatus.Information);

        IEnumerable<ActivityReport> GetReports(long taskId, long? fromLatestTaskId);
    }
}