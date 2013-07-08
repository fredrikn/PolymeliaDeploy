namespace PolymeliaDeploy.Data.Repositories
{
    using System.Threading.Tasks;

    public interface IReportRepository
    {
        Task AddReport(ActivityReport report);
    }
}