using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;

namespace PolymeliaDeployController.Hub
{
    using System.Threading;

    using PolymeliaDeploy.Data.Repositories;

    public class DeployControllerHub : Microsoft.AspNet.SignalR.Hub
    {
        private readonly static IDictionary<string, Agent> _connectedAgents = new Dictionary<string, Agent>();
        private static readonly object _lock = new object();

        private readonly IReportRepository _reportRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IAgentRepository _agentRepository;

        public DeployControllerHub(
                                   IReportRepository reportRepository,
                                   IActivityRepository activityRepository,
                                   IAgentRepository agentRepository)
        {
            if (reportRepository == null) throw new ArgumentNullException("reportRepository");
            if (activityRepository == null) throw new ArgumentNullException("activityRepository");
            if (agentRepository == null) throw new ArgumentNullException("agentRepository");

            _reportRepository = reportRepository;
            _activityRepository = activityRepository;
            _agentRepository = agentRepository;
        }


        //TODO: Add some sort of key/certificate, authentication
        public void Connect(string roleName, string serverName)
        {
            RegisterAgent(roleName, GetAgentIpAddress(), serverName);
            Console.WriteLine("Agent from server '{0}' and IP '{1}' with role: '{2}' is now connected", serverName, GetAgentIpAddress(), roleName);
        }


        public void Deploy()
        {
            //Make sure Client connects to this hub and call this deploy method

            //Run Workflow

            //Get Agents for each ServerRole, send activities, register for completion. When all agents are green, the mark deployment as succeeded.
        }


        public async Task AgentIsReadyForNewTasks(string roleName)
        {
            SetCurrentAgentToBeAvailable();

            await CheckForAgentActivityAndRunActivities(roleName);
        }


        public async Task Report(ActivityReport report)
        {
            await _reportRepository.AddReport(report);
        }


        public async Task ActivityFailed(long activityTaskId)
        {
            await _activityRepository.UpdateActivityTaskStatus(activityTaskId, ActivityStatus.Failed);
        }


        public async Task ActivityCompleted(long activityTaskId)
        {
            if (AreAllAgentsDoneWithActivity(activityTaskId))
                await _activityRepository.UpdateActivityTaskStatus(activityTaskId, ActivityStatus.Completed);
        }


        public override Task OnDisconnected()
        {
            lock (_lock)
            {
                if (_connectedAgents.ContainsKey(Context.ConnectionId))
                {
                    var agent = _connectedAgents[Context.ConnectionId];

                    SetCurrentAgentToBeAvailable();

                    _connectedAgents.Remove(Context.ConnectionId);

                    Console.WriteLine("An Agent with the role '{0}' on Server '{1}' is now disconnected", agent.Role, agent.ServerName);
                }
            }

            return base.OnDisconnected();
        }


        protected string GetAgentIpAddress()
        {
            var env = Get<IDictionary<string, object>>(Context.Request.Items, "owin.environment");

            return env == null ? null : Get<string>(env, "server.RemoteIpAddress");
        }


        private void RegisterAgent(string roleName, string agentIpAddress, string serverName)
        {
            var agent = _agentRepository.Get(roleName, agentIpAddress);

            if (agent == null)
            {
                //TODO: Auto activate agent based on some creteria
                agent = new Agent
                {
                    Confirmed = DateTime.Now,
                    ConfirmedBy = Thread.CurrentPrincipal.Identity.Name,
                    IsActive = true,
                    Role = roleName,
                    ServerName = serverName,
                    IpAddress = agentIpAddress
                };

                _agentRepository.RegisterAgent(agent);
            }

            if (!agent.Confirmed.HasValue || !agent.IsActive)
                Console.WriteLine("Agent from server '{0}' and with the role '{1}' is not confirmed or is inactive. The agent will not be used", serverName, roleName);

            lock (_lock)
            {
                if (!_connectedAgents.ContainsKey(Context.ConnectionId))
                    _connectedAgents.Add(Context.ConnectionId, agent);
            }
        }


        private async Task CheckForAgentActivityAndRunActivities(string roleName)
        {
            var activityTasks = await GetActivityTasks(roleName);

            var activityTaskDtos = activityTasks as IList<ActivityTaskDto> ?? activityTasks.ToList();

            if (activityTaskDtos.Any())
            {
                SetCurrentAgentToBusy();

                await Clients.Client(Context.ConnectionId).RunActivities(activityTaskDtos);
            }
        }


        private async Task<IEnumerable<ActivityTaskDto>> GetActivityTasks(string serverRole)
        {
            var agent = GetCurrentAgent();

            var activites = await _activityRepository.GetActivityTasks(agent.LastActivityId == null ? 0 : agent.LastActivityId.Value, serverRole);

            var actititiesToRun = activites.ToList().Select(a => new ActivityTaskDto
            {
                DeploymentId = a.DeploymentId,
                Id = a.Id,
                ActivityCode = a.ActivityCode,
                ActivityName = a.ActivityName,
                DeployVersion = a.DeployVersion,
                Created = a.Created,
                CreatedBy = a.CreatedBy,
                ServerRole = a.ServerRole,
                Status = a.Status,
                Environment = a.Environment
            });

            if (!actititiesToRun.Any(a => a.Status == ActivityStatus.Failed ||
                a.Status == ActivityStatus.Canceled))
                return actititiesToRun;

            return new List<ActivityTaskDto>();
        }


        private Agent GetCurrentAgent()
        {
            Agent agent = null;

            if (_connectedAgents.ContainsKey(Context.ConnectionId))
                agent = _connectedAgents[Context.ConnectionId];

            if (agent == null)
                throw new ArgumentOutOfRangeException("_connectedAgents");

            return agent;
        }


        private void SetCurrentAgentToBeAvailable()
        {
            var agent = GetCurrentAgent();
            agent.IsBusy = false;

            _agentRepository.Update(agent);
        }


        private void SetCurrentAgentToBusy()
        {
            var agent = GetCurrentAgent();
            agent.IsBusy = true;

            _agentRepository.Update(agent);
        }


        private static bool AreAllAgentsDoneWithActivity(long activityTaskId)
        {
            return _connectedAgents.Values.Any(a => !a.IsBusy && a.IsActive && a.LastActivityId == activityTaskId);
        }


        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}