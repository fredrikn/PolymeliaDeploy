namespace PolymeliaDeploy.Data.Repositories
{
    using System.Threading.Tasks;

    public class ReportRepository : IReportRepository
    {
        public async Task AddReport(ActivityReport report)
        {
            await Task.Run(() =>
            {
                using (var db = new PolymeliaDeployDbContext())
                {
                    db.ActivityReports.Add(report);
                    db.SaveChanges();
                }
            });
        }

    }
}
