using System;
namespace PolymeliaDeploy.Agent
{
    public interface IAgentService : IDisposable
    {
        void Start();
        void Stop();
    }
}
