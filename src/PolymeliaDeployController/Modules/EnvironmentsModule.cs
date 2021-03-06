﻿namespace PolymeliaDeployController.Modules
{
    using System;
    using System.Linq;

    using Nancy;
    using Nancy.ModelBinding;

    using PolymeliaDeploy.Data;

    using Environment = PolymeliaDeploy.Data.Environment;

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

                    foreach (var agent in environment.AssingedAgentIds.Select(agentId => db.Agents.SingleOrDefault(a => a.Id == agentId)).Where(agent => agent != null))
                        agent.EnvironmentId = environment.Id;

                    db.SaveChanges();

                    return Response.AsJson(environment);
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

                    return Response.AsJson(true);
                }
            };
        }
    }
}
