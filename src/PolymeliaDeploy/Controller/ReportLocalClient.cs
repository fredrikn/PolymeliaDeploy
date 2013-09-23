using System;

namespace PolymeliaDeploy.Controller
{
    using System.Collections.Generic;

    using PolymeliaDeploy.Data;
    using System.Threading.Tasks;


    public class ReportLocalClient : IReportClient
    {
        public async Task Report(
                           long taskId,
                           string serverRole, 
                           string message, 
                           string activityName, 
                           ReportStatus reportStatus = ReportStatus.Information)
        {
            await Task.Run( () =>
                        {
                            var report = new ActivityReport
                            {
                                DeploymentId = taskId,
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
                        });
        }


        public async Task<IEnumerable<ActivityReport>> GetReports(long taskId, long? fromLatestTaskId)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}