namespace PolymeliaDeploy.Data.Repositories
{
    using System.Collections.Generic;

    public interface IAgentRepository
    {
        void RegisterAgent(Agent agent);

        Agent Get(string role, string ipAddress);

        void Update(Agent agent);

        IEnumerable<Agent> GetAll();
    }
}