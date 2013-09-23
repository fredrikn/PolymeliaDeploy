using System.Linq;
using System.Threading.Tasks;

namespace PolymeliaDeploy.Data.Repositories
{
    using System.Collections.Generic;

    public class ActivityRepository : IActivityRepository
    {
        public async Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status)
        {
            await Task.Run(() =>
            {
                using (var db = new PolymeliaDeployDbContext())
                {
                    var activityTask = db.ActivityTasks.Single(t => t.Id == activityTaskId);

                    activityTask.Status = status;

                    if (status == ActivityStatus.Failed)
                    {
                        var mainActivity = db.Deployments.Single(t => t.Id == activityTask.DeploymentId);
                        mainActivity.Status = ActivityStatus.Failed;
                    }

                    db.SaveChanges();
                }
            });
        }


        public async Task<IEnumerable<ActivityTask>> GetActivityTasks(long lastTaskId, string serverRole)
        {
            return await Task.Run(() =>
            {
                using (var db = new PolymeliaDeployDbContext())
                {
                    var maintask = db.Deployments.Where(t => t.Id > lastTaskId &&
                                                          (t.Status != ActivityStatus.Failed &&
                                                           t.Status != ActivityStatus.Canceled))
                                                    .OrderByDescending(t => t.Id)
                                                    .FirstOrDefault();

                    if (maintask != null)
                    {
                        return db.ActivityTasks.Where(t => t.ServerRole == serverRole &&
                                                           t.DeploymentId == maintask.Id)
                                               .OrderBy(t => t.Id)
                                               .ToList();
                    }

                    return new List<ActivityTask>();
                }
            });
        }
    }
}