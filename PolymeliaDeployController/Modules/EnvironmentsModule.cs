namespace PolymeliaDeployController.Modules
{
    using System.Linq;

    using Nancy;
    using Nancy.ModelBinding;

    using PolymeliaDeploy.Data;

    public class EnvironmentsModule : NancyModule
    {
        public EnvironmentsModule()
        {

            Get["/environments/{ProjectId}"] = param =>
            {
                int projectId = int.Parse(param.ProjectId);

                using (var db = new PolymeliaDeployDbContext())
                {
                    return Response.AsJson(db.Environments.Where(e => e.ProjectId == projectId && !e.Deleted).ToList());
                }
            };


            Get["/environment/{EnvironmentId}"] = param =>
            {
                int environmentId = int.Parse(param.EnvironmentId);

                using (var db = new PolymeliaDeployDbContext())
                {
                    return Response.AsJson(db.Environments.Single(e => e.Id == environmentId && !e.Deleted));
                }
            };


            Post["/environments/"] = _ =>
            {
                var environment = this.Bind<Environment>();

                using (var db = new PolymeliaDeployDbContext())
                {
                    var env = db.Environments.Single(e => e.Id == environment.Id);

                    env.Name = environment.Name;
                    env.WorkflowContent = environment.WorkflowContent;
                    env.Description = environment.Description;

                    db.SaveChanges();

                    return this.Response.AsJson(true);
                }
            };


            Put["/environments/"] = _ =>
            {
                var environment = this.Bind<Environment>();

                using (var db = new PolymeliaDeployDbContext())
                {
                    db.Environments.Add(environment);
                    db.SaveChanges();

                    return this.Response.AsJson(environment);
                }
            };


            Delete["/environments/{EnvironmentId}"] = param =>
            {
                int environmentId = int.Parse(param.EnvironmentId);

                using (var db = new PolymeliaDeployDbContext())
                {
                    var environment = db.Environments.Single(e => e.Id == environmentId);
                    environment.Deleted = true;
                    db.SaveChanges();

                    return this.Response.AsJson(true);
                }
            };
        }
    }
}
