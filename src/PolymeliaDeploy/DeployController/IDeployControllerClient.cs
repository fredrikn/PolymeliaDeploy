using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolymeliaDeploy.DeployController
{
    public interface IDeployControllerClient : IDisposable
    {
        void Connect(string url, string serverRole, string controllerKey);

        Task ActivityCompleted(long activityTaskId);

        Task ActivityFailed(long activityTaskId);

        Task AgentIsReadyForNewTasks();

        Task Report(ActivityReport report);

        Action<IEnumerable<ActivityTaskDto>> OnRunActivity { get; set; }

        void Disconnect();
    }
}
