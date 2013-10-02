using System.Linq;

namespace PolymeliaDeployController.Modules
{
    using Nancy;
    using Nancy.ModelBinding;

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


            Get["agents/unassigned"] = _ =>
                {
                    using (var db = new PolymeliaDeployDbContext())
                    {
                        var agents =
                            db.Agents.Where(a => a.EnvironmentId == null && a.Confirmed != null)
                                     .OrderBy(a => a.Role)
                                     .ToList();

                        return Response.AsJson(agents);
                    }
                };


            Get["agents/environment/{EnvironmentId}"] = param =>
            {
                int environmentId = int.Parse(param.EnvironmentId);

                using (var db = new PolymeliaDeployDbContext())
                {
                    var agents = db.Agents.Where(a => a.EnvironmentId == environmentId)
                                          .OrderBy(a => a.Role).ToList();
                    
                    return Response.AsJson(agents);
                }
            };


            Put["agents"] = _ =>
            {
                var agent = this.Bind<Agent>();

                using (var db = new PolymeliaDeployDbContext())
                {
                    db.Agents.Add(agent);
                    db.SaveChanges();

                    return Response.AsJson(agent);
                }
            };
        }
    }
}