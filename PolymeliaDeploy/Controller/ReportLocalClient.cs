using System;

namespace PolymeliaDeploy.Controller
{
    using System.Collections.Generic;

    using PolymeliaDeploy.Data;


    public class ReportLocalClient : IReportClient
    {
        public void Report(
                           long taskId,
                           string serverRole, 
                           string message, 
                           string activityName, 
                           ReportStatus reportStatus = ReportStatus.Information)
        {
            var report = new ActivityReport
            {
                TaskId = taskId,
                LocalCreated = DateTime.Now,
                MachineName = System.Environment.MachineName,
                Message = message,
                ServerRole = serverRole,
                ActivityName = activityName,
                Status = reportStatus,
                ActivityTaskId = null
            };

            using (var db = new PolymeliaDeployDbContext())
            {
                db.ActivityReports.Add(report);
                db.SaveChanges();
            } 
        }


        public IEnumerable<ActivityReport> GetReports(long taskId, long? fromLatestTaskId)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}