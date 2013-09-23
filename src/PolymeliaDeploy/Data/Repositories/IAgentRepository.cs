namespace PolymeliaDeploy.Data.Repositories
{
    public interface IAgentRepository
    {
        void RegisterAgent(Agent agent);

        Agent Get(string role, string ipAddress);

        void Update(Agent agent);
    }
}