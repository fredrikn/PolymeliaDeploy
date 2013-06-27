namespace PolymeliaDeploy.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public enum ActivityStatus
    {
        New,
        Completed,
        Failed,
        Canceled
    }

    public class ActivityTask
    {
        public long Id { get; set; }

        public long TaskId { get; set; }

        public string ServerRole { get; set; }

        public string ActivityCode { get; set; }

        public string CreatedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Created { get; set; }

        public string ActivityName { get; set; }

        public ActivityStatus Status { get; set; }
    }
}