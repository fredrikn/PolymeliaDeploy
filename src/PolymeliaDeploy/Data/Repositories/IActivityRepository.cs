namespace PolymeliaDeploy.Data.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IActivityRepository
    {
        Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status);

        Task<IEnumerable<ActivityTask>> GetActivityTasks(long lastTaskId, string serverRole);
    }
}