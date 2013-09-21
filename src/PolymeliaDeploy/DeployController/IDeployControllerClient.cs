using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolymeliaDeploy.DeployController
{
    public interface IDeployControllerClient : IDisposable
    {
        Task Connect(string url, long lastTaskId, string serverRole);

        Task Report(ActivityReport report);

        Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status);

        Action<IEnumerable<ActivityTaskDto>> OnRunActivity { get; set; }

        void Disconnect();
    }
}
