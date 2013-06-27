namespace PolymeliaDeployController.Modules
{
    using System;
    using System.Linq;
    using System.Transactions;

    using Nancy;
    using Nancy.ModelBinding;

    using PolymeliaDeploy.Data;

    using Environment = PolymeliaDeploy.Data.Environment;

    public class ProjectsModule : NancyModule
    {
        public ProjectsModule()
        {
            Get["projects"] = _ =>
            {
                using (var db = new PolymeliaDeployDbContext())
                {
                    return this.Response.AsJson(db.Projects.Where( p => p.Deleted == false).ToList());
                }
            };


            Put["projects/{CopyProjectId?}"] = param =>
            {
                try
                {
                    int? copyProjectId = param.CopyProjectId == null ? null : int.Parse(param.CopyProjectId);

                    var project = this.Bind<Project>();

                    using (var tran = new TransactionScope())
                    {
                        using (var db = new PolymeliaDeployDbContext())
                        {
                            db.Projects.Add(project);
                            db.SaveChanges();

                            if (copyProjectId.HasValue)
                                CopyAnotherProjectsEnvironments(db, copyProjectId.Value, project);

                            db.SaveChanges();
                            tran.Complete();

                            return this.Response.AsJson(project);
                        }
                    }


                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            };


            Delete["projects/{ProjectId}"] = param =>
            {
                int projectId = int.Parse(param.ProjectId);

                using (var db = new PolymeliaDeployDbContext())
                {
                    var project = db.Projects.Single(p => p.Id == projectId);

                    project.Deleted = true;

                    db.SaveChanges();

                    return this.Response.AsJson(true);
                }
            };
        }


        private static void CopyAnotherProjectsEnvironments(
                                                            PolymeliaDeployDbContext db,
                                                            int copyProjectId,
                                                            Project project)
        {
            var environmentsToCopy = db.Environments.Where(e => e.ProjectId == copyProjectId && !e.Deleted)
                                                    .ToList();

            foreach (var environment in environmentsToCopy)
            {
                var newEnv = new Environment
                                  {
                                      Description = environment.Description,
                                      Name = environment.Name,
                                      WorkflowContent = environment.WorkflowContent,
                                      ProjectId = project.Id,
                                      CreatedBy = project.CreatedBy
                                  };

                db.Environments.Add(newEnv);
                db.SaveChanges();

                var variables = db.Variables.Where(v => v.EnvironmentId == environment.Id).ToList();

                foreach (var variable in variables)
                {
                    var newVar = new Variable {
                                                  Scope = variable.Scope,
                                                  VariableKey = variable.VariableKey,
                                                  VariableValue = variable.VariableValue,
                                                  EnvironmentId = newEnv.Id
                                              };

                    db.Variables.Add(newVar);
                }
            }
        }
    }
}
