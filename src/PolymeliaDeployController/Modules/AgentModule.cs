using System.Linq;

namespace PolymeliaDeployController.Modules
{
    using Nancy;

    using PolymeliaDeploy.Data;

    public class AgentModule : NancyModule
    {
        public AgentModule()
        {
            Get["agents"] = _ =>
            {
                using (var db = new PolymeliaDeployDbContext())
                {
                    var agents = db.Agents.OrderBy(a => a.Role).ToList();
                    return Response.AsJson(agents);
                }
            };
        }
    }
}
