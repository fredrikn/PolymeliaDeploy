using System;

namespace PolymeliaDeploy.ApiDto
{
    using PolymeliaDeploy.Data;
    using System.Collections.Generic;

    public class ActivityTaskDto
    {
        public long Id { get; set; }

        public long TaskId { get; set; }

        public string DeployVersion { get; set; }

        public string ServerRole { get; set; }

        public string ActivityCode { get; set; }

        public string CreatedBy { get; set; }

        public DateTime Created { get; set; }

        public string ActivityName { get; set; }

        public IDictionary<string, string> Variables { get; set; }

        public ActivityStatus Status { get; set; }
    }
}
