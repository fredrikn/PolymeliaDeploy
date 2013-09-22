namespace PolymeliaDeploy.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using PolymeliaDeploy.ApiDto;
    using PolymeliaDeploy.Data;

    public interface IActivityClient : IDisposable
    {
        IEnumerable<ActivityTaskDto> GetActivityTasksFromDeployController(long latestTaskRunId, string serverRoleName);

        Task<IEnumerable<MainActivity>> GetDeployHistory(int projectId, int environmentId);

        MainActivity LatestDeployedActivity(int projectId, int environmentId);

        void UpdateActivityTaskStatus(ActivityTaskDto activityTask);

        long Deploy(MainActivity mainActivity);
    }
}