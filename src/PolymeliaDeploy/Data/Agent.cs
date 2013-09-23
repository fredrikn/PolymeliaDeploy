namespace PolymeliaDeploy.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Net.Sockets;

    public class Agent
    {
        public int Id { get; set; }

        public string Role { get; set; }
        
        public string ServerName { get; set; }

        public string IpAddress { get; set; }

        public string ConfirmedBy { get; set; }
        
        public DateTime? Confirmed { get; set; }

        public bool IsActive { get; set; }


        public long? LastDeploymentId { get; set; }

        [NotMapped]
        public bool IsBusy { get; set; }
    }
}
