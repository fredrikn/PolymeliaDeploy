using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PolymeliaDeploy.DeployController
{
    public interface IDeployControllerClient : IDisposable
    {
        Task Connect(string url, string serverRole);

        Task<IEnumerable<ActivityTaskDto>> GetActivityTasks(long lastTaskId);

        Task Report(ActivityReport report);

        Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status);

        void Disconnect();
    }
}
