namespace PolymeliaDeploy.Data.Repositories
{
    using System.Data;
    using System.Linq;

    public class AgentRepository : IAgentRepository
    {
        public void RegisterAgent(Agent agent)
        {
            using (var db = new PolymeliaDeployDbContext())
            {
                db.Agents.Add(agent);
                db.SaveChanges();
            }
        }


        public Agent Get(string role, string serverName)
        {
            using (var db = new PolymeliaDeployDbContext())
            {
                return db.Agents.FirstOrDefault(a => a.ServerName == serverName &&
                                                     a.Role == role);
            }
        }


        public void Update(Agent agent)
        {
            using (var db = new PolymeliaDeployDbContext())
            {
                db.Agents.Attach(agent);
                db.Entry(agent).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
