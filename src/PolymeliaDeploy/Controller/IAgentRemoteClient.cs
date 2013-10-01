﻿namespace PolymeliaDeploy.Controller
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using PolymeliaDeploy.Data;

    public interface IAgentRemoteClient
    {
        Task<IEnumerable<Agent>> GetAll();

        Task<IEnumerable<Agent>> GetAllUnassigned();

        Agent Add(Agent agent);
    }
}