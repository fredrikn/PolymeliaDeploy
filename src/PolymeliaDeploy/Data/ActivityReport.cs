using System;

namespace PolymeliaDeploy.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    public enum ReportStatus
    {
        Debug,
        Information,
        Warning,
        Error
    }

    public class ActivityReport
    {
        public ActivityReport()
        {
            LocalCreated = DateTime.Now;
        }

        public long Id { get; set; }

        public long TaskId { get; set; }

        public long? ActivityTaskId { get; set; }

        public string ActivityName { get; set; }

        public string Message { get; set; }

        public DateTime LocalCreated { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Created { get; set; }

        public string ServerRole { get; set; }

        public string MachineName { get; set; }

        public ReportStatus Status { get; set; }
    }
}
