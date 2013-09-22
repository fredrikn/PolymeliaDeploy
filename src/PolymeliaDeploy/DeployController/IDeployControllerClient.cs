using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolymeliaDeploy.DeployController
{
    public interface IDeployControllerClient : IDisposable
    {
        void Connect(string url, string serverRole);

        Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status);

        Task Report(ActivityReport report);

        Task AgentIsReadyForNewTasks(long lastTaskId);

        Action<IEnumerable<ActivityTaskDto>> OnRunActivity { get; set; }

        Action OnConnected { get; set; }

        void Disconnect();
    }
}
