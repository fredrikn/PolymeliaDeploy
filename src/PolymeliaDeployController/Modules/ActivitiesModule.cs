﻿namespace PolymeliaDeployController.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Nancy;
    using Nancy.ModelBinding;

    using PolymeliaDeploy;
    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;
    using PolymeliaDeploy.Workflow;

    public class ActivitiesModule : NancyModule
    {
        public ActivitiesModule()
        {
            Get["deploys/last/{ProjectId}/{EnvironmentId}"] = param =>
            {
                using (var db = new PolymeliaDeployDbContext())
                {
                    int environmentId = int.Parse(param.EnvironmentId);
                    int projectId = int.Parse(param.ProjectId);

                    var maintask = db.MainActivities.Where(m => m.ProjectId == projectId &&
                                                                m.EnvironmentId == environmentId)
                                                    .OrderByDescending(m => m.Id)
                                                    .FirstOrDefault();

                    return Response.AsJson(maintask);
                }
            };


            Get["deploys/history/{ProjectId}/{EnvironmentId}"] = param =>
            {
                using (var db = new PolymeliaDeployDbContext())
                {
                    int environmentId = int.Parse(param.EnvironmentId);
                    int projectId = int.Parse(param.ProjectId);

                    var maintasks = db.MainActivities.Where(m => m.ProjectId == projectId  &&
                                                                 m.EnvironmentId == environmentId)
                                                    .OrderByDescending(m => m.Id)
                                                    .Take(5).ToList();

                    return Response.AsJson(maintasks);
                }
            };


             // POST /10/Completed
            Post["activity/{ActivityTaskId}/{Status}"] = param =>
            {
                using (var db = new PolymeliaDeployDbContext())
                {

                    long activityTaskId = param.ActivityTaskId;
                    ActivityStatus status = Enum.Parse(typeof(ActivityStatus), param.Status, true);

                    var activityTask = db.ActivityTasks.SingleOrDefault(t => t.Id == activityTaskId);

                    activityTask.Status = status;

                    if (status == ActivityStatus.Failed)
                    {
                        var mainActivity = db.MainActivities.SingleOrDefault(t => t.Id == activityTask.TaskId);
                        mainActivity.Status = ActivityStatus.Failed;
                    }

                    db.SaveChanges();

                    return Response.AsJson(activityTask);
                }
            };


            Put["deploy/"] = _ =>
            {
                var mainActivity = this.Bind<MainActivity>();

                DeployServices.ReportClient = new ReportLocalClient();

                var deployVariables = new Collection<DeployVariable>();
                
                using (var db = new PolymeliaDeployDbContext())
                {
                    db.MainActivities.Add(mainActivity);
                    db.SaveChanges();
                
                    foreach (var variable in db.Variables.Where( v => v.EnvironmentId == mainActivity.EnvironmentId))
                    {
                        var deployVariable = CreateDeployVariable(mainActivity, variable);
                        deployVariables.Add(deployVariable);

                        db.DeployVariables.Add(deployVariable);
                    }

                    db.SaveChanges();
                }


                var parameters = new Dictionary<string, object>
                                 {
                                     { "DeployTaskId", mainActivity.Id },
                                     { "DeployTaskVersion", mainActivity.Version },
                                     { "DeployVariables", deployVariables },
                                     { "DeployEnvironment", mainActivity.Environment }
                                 };


                new WorkflowRunner().Run(mainActivity.DeployActivity, parameters);

                return Response.AsJson(mainActivity.Id);
            };
        }

        private static DeployVariable CreateDeployVariable(MainActivity mainActivity, Variable variable)
        {
            return new DeployVariable
                   {
                       TaskId = mainActivity.Id,
                       VariableKey = variable.VariableKey,
                       VariableValue = variable.VariableValue,
                       Scope = variable.Scope
                   };
        }

        private static IDictionary<string, string> GetEnvironmentVariables(PolymeliaDeployDbContext db, MainActivity maintask)
        {
            var variables = db.Variables.Where(v => v.EnvironmentId == maintask.EnvironmentId);

            IDictionary<string, string> variablesForEnvironment = new Dictionary<string, string>();

            foreach (var variable in variables)
                variablesForEnvironment.Add(variable.VariableKey, variable.VariableValue);

            return variablesForEnvironment;
        }
    }
}