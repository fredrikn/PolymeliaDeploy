namespace PolymeliaDeploy.Data
{
    using System.Data.Entity;

    public class PolymeliaDeployDbContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<PolymeliaDeployWeb.Models.PolymeliaDeployDbContext>());

        public PolymeliaDeployDbContext() : base("name=PolymeliaDeployDbContext")
        {
            Database.SetInitializer<PolymeliaDeployDbContext>(null);
        }

        public DbSet<ActivityTask> ActivityTasks { get; set; }

        public DbSet<MainActivity> MainActivities { get; set; }

        public DbSet<ActivityReport> ActivityReports { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Environment> Environments { get; set; }

        public DbSet<Variable> Variables { get; set; }
    }
}
