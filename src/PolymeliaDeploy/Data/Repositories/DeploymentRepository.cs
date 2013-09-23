using System.Linq;

namespace PolymeliaDeploy.Data.Repositories
{
    public class DeploymentRepository : IDeploymentRepository
    {
        public void UpdateDeploymentStatus(long deploymentId, ActivityStatus status)
        {
            using (var db = new PolymeliaDeployDbContext())
            {
                var mainActivity = db.Deployments.Single(t => t.Id == deploymentId);
                mainActivity.Status = ActivityStatus.Failed;

                db.SaveChanges();
            }
        }
    }
}
