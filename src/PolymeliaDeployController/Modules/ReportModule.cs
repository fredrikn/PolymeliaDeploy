namespace PolymeliaDeployController.Modules
{
    using System.Linq;

    using Nancy;
    using Nancy.ModelBinding;

    using PolymeliaDeploy.Data;

    public class ReportModule : NancyModule
    {
        public ReportModule()
        {

            Get["reports/{DeploymentId}/{FromLastKnownId?}"] = param =>
            {
                long taskId = long.Parse(param.DeploymentId);
                long? fromLastKnownId = param.FromLastKnownId == null ? null : long.Parse(param.FromLastKnownId);

                using (var db = new PolymeliaDeployDbContext())
                {
                    var query = db.ActivityReports.Where(a => a.DeploymentId == taskId);

                    if (fromLastKnownId.HasValue)
                        query = query.Where(a => a.Id > fromLastKnownId);

                    query = query.OrderBy(a => a.LocalCreated);

                    return this.Response.AsJson(query.ToList());
                }
            };


            // PUT /report
            Put["report/"] = _ =>
            {
                var report = this.Bind<ActivityReport>();

                using (var db = new PolymeliaDeployDbContext())
                {
                    db.ActivityReports.Add(report);
                    db.SaveChanges();

                    return Response.AsJson(true);
                }
            };

        }
    }
}
