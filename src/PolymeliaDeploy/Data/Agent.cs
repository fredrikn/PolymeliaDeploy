namespace PolymeliaDeploy.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    using PolymeliaDeploy.Agent;

    public class Agent
    {
        public int Id { get; set; }

        public string Role { get; set; }
        
        public string ServerName { get; set; }

        public int? EnvironmentId { get; set; }

        public string IpAddress { get; set; }

        public string ConfirmedBy { get; set; }
        
        public DateTime? Confirmed { get; set; }

        public bool IsActive { get; set; }

        public long? LastDeploymentId { get; set; }

        [NotMapped]
        public bool IsBusy { get; set; }

        [NotMapped]
        public AgentStatus Status
        {
            get
            {
                if (!Confirmed.HasValue)
                    return AgentStatus.NotConfirmed;

                if (!IsActive)
                    return AgentStatus.NotActive;

                return IsBusy ? AgentStatus.InProgress : AgentStatus.Ready;
            }
        }
    }
}