namespace PolymeliaDeploy.Controller
{
    using System;
    using System.Collections.Generic;

    using PolymeliaDeploy.ApiDto;
    using PolymeliaDeploy.Data;

    public interface IActivityClient : IDisposable
    {
        IEnumerable<ActivityTaskDto> GetActivityTasksFromDeployController(long latestTaskRunId, string serverRoleName);

        MainActivity LatestDeployedActivity(int projectId, int environmentId);

        void UpdateActivityTaskStatus(ActivityTaskDto activityTask);

        long Deploy(MainActivity mainActivity);
    }
}