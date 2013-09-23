using System;

namespace PolymeliaDeploy.ApiDto
{
    using PolymeliaDeploy.Data;

    public class ActivityTaskDto
    {
        public long Id { get; set; }

        public long DeploymentId { get; set; }

        public string DeployVersion { get; set; }

        public string Environment { get; set; }

        public string ServerRole { get; set; }

        public string ActivityCode { get; set; }

        public string CreatedBy { get; set; }

        public DateTime Created { get; set; }

        public string ActivityName { get; set; }

        public ActivityStatus Status { get; set; }
    }
}
