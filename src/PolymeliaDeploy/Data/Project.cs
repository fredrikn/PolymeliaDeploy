namespace PolymeliaDeploy.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string CreatedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime Created { get; set; }

        public bool Deleted { get; set; }
    }
}
