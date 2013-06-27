namespace PolymeliaDeployController.Modules
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Nancy;
    using Nancy.ModelBinding;

    using PolymeliaDeploy.Data;

    public class VariablesModule : NancyModule
    {
        public VariablesModule()
        {

            Get["/variables/{EnvironmentId}"] = param =>
            {
                int environmentId = int.Parse(param.EnvironmentId);

                using (var db = new PolymeliaDeployDbContext())
                {
                    return this.Response.AsJson(db.Variables.Where(e => e.EnvironmentId == environmentId).ToList());
                }
            };


            Put["/variables/{EnvironmentId}"] = param =>
            {
                try
                {
                    int environmentId = int.Parse(param.EnvironmentId);

                    var variables = this.Bind<Collection<Variable>>();

                    using (var db = new PolymeliaDeployDbContext())
                    {
                        db.Database.ExecuteSqlCommand("DELETE FROM Variables WHERE EnvironmentId = " + environmentId);

                        foreach (var variable in variables)
                        {
                            variable.EnvironmentId = environmentId;
                            db.Variables.Add(variable);
                        }

                        db.SaveChanges();

                        return this.Response.AsJson(true);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            };
        }
    }
}
