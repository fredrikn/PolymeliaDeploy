namespace PolymeliaDeploy.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MainActivity
    {
        public long Id { get; set; }

        public int ProjectId { get; set; }

        public int EnvironmentId { get; set; }

        public string Environment { get; set; }

        public string DeployActivity { get; set; }

        public string CreatedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Created { get; set; }

        public string Version { get; set; }

        public ActivityStatus Status { get; set; }
    }
}