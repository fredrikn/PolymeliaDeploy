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

        Task ActivityCompleted(long activityTaskId);

        Task ActivityFailed(long activityTaskId);
        
        Task Report(ActivityReport report);

        Task AgentIsReadyForNewTasks();

        Action<IEnumerable<ActivityTaskDto>> OnRunActivity { get; set; }

        Action OnConnected { get; set; }

        void Disconnect();
    }
}
