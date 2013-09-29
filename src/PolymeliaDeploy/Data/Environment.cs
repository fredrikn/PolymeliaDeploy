namespace PolymeliaDeploy.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Environment
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string WorkflowContent { get; set; }

        public string CreatedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Created { get; set; }

        public bool Deleted { get; set; }

        [NotMapped]
        public bool HasChanges { get; set; }

        [NotMapped]
        public IEnumerable<int> AssingedAgentIds { get; set; }
    }
}
