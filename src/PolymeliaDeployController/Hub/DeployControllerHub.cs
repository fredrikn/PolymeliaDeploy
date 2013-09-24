using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;

namespace PolymeliaDeployController.Hub
{
    using System.Security;
    using System.Threading;

    using Microsoft.AspNet.SignalR.Hubs;

    using PolymeliaDeploy.Data.Repositories;
    using PolymeliaDeploy.Security;

    using PolymeliaDeployController.Configuration;

    public class DeployControllerHub : Microsoft.AspNet.SignalR.Hub
    {
        private readonly static IDictionary<string, Agent> _connectedAgents = new Dictionary<string, Agent>();
        private static readonly object _lock = new object();

        private readonly IReportRepository _reportRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IControllerConfigurationSettings _controllerSettings;

        public DeployControllerHub(
                                   IReportRepository reportRepository,
                                   IActivityRepository activityRepository,
                                   IAgentRepository agentRepository,
                                   IControllerConfigurationSettings controllerSettings)
        {
            if (reportRepository == null) throw new ArgumentNullException("reportRepository");
            if (activityRepository == null) throw new ArgumentNullException("activityRepository");
            if (agentRepository == null) throw new ArgumentNullException("agentRepository");
            if (controllerSettings == null) throw new ArgumentNullException("controllerSettings");

            _reportRepository = reportRepository;
            _activityRepository = activityRepository;
            _agentRepository = agentRepository;
            _controllerSettings = controllerSettings;
        }


        public override Task OnConnected()
        {
            var roleName = Context.Headers["AgentRoleName"];
            var serverName = Context.Headers["AgentServerName"];
            var controllerKey = Context.Headers["ControllerKey"];

            if (string.IsNullOrWhiteSpace(controllerKey))
                throw new UnauthorizedAccessException("Access denied!");

            if (!controllerKey.Equals(TokenGenerator.Generate(_controllerSettings.ControllerKey)))
                throw new UnauthorizedAccessException("Access denied!");

            RegisterAgent(roleName, GetAgentIpAddress(), serverName);
            Console.WriteLine("Agent from server '{0}' and IP '{1}' with role: '{2}' is now connected", serverName, GetAgentIpAddress(), roleName);

            return base.OnConnected().ContinueWith(
                t =>
                {
                    SetCurrentAgentToBeAvailable();
                    Task.Run( async () => await CheckForAgentActivityAndRunActivities(roleName));
                });
        }


        public void Deploy()
        {
            //Make sure Client connects to this hub and call this deploy method

            //Run Workflow

            //Get Agents for each ServerRole, send activities, register for completion. When all agents are green, the mark deployment as succeeded.
        }


        public async Task Report(ActivityReport report)
        {
            EnsureAgentIsAuthorized();

            await _reportRepository.AddReport(report);
        }


        public async Task ActivityFailed(long activityTaskId)
        {
            EnsureAgentIsAuthorized();

            await _activityRepository.UpdateActivityTaskStatus(activityTaskId, ActivityStatus.Failed);
        }


        public async Task ActivityCompleted(long activityTaskId)
        {
            EnsureAgentIsAuthorized();

            if (AreAllAgentsDoneWithActivity(activityTaskId))
                await _activityRepository.UpdateActivityTaskStatus(activityTaskId, ActivityStatus.Completed);
        }


        public async Task AgentIsReadyForNewTasks(string roleName)
        {
            EnsureAgentIsAuthorized();

            SetCurrentAgentToBeAvailable();

            await CheckForAgentActivityAndRunActivities(roleName);
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
            var agent = _agentRepository.Get(roleName, serverName);

            if (agent == null)
                agent = RegisterNewAgent(roleName, agentIpAddress, serverName);

            if (!agent.Confirmed.HasValue || !agent.IsActive)
            {
                Console.WriteLine("Agent from server '{0}' and with the role '{1}' is not confirmed or is inactive. The agent will not be used", serverName, roleName);
                throw new SecurityException("Access denied!");
            }

            lock (_lock)
            {
                if (!_connectedAgents.ContainsKey(Context.ConnectionId))
                    _connectedAgents.Add(Context.ConnectionId, agent);
            }
        }


        private void EnsureAgentIsAuthorized()
        {
            var agent = GetCurrentAgent();

            if (agent.IsActive && agent.Confirmed != null)
                return;

            throw new NotAuthorizedException("Agent isn't authorized");

        }


        private Agent RegisterNewAgent(string roleName, string agentIpAddress, string serverName)
        {
            //TODO: Auto activate agent based on some creteria?!
            var agent = new Agent
                    {
                        Confirmed = null,
                        ConfirmedBy = Thread.CurrentPrincipal.Identity.Name,
                        IsActive = false,
                        Role = roleName,
                        ServerName = serverName,
                        IpAddress = agentIpAddress
                    };

            _agentRepository.RegisterAgent(agent);

            return agent;
        }

        private async Task CheckForAgentActivityAndRunActivities(string roleName)
        {
            var activityTasks = await GetActivityTasks(roleName);

            var activityTaskDtos = activityTasks as IList<ActivityTaskDto> ?? activityTasks.ToList();

            if (activityTaskDtos.Any())
            {
                SetCurrentAgentToBusy();

                await Clients.Client(Context.ConnectionId).RunActivities(activityTaskDtos);

                UpdateAgentsLastDeployment(activityTaskDtos);
            }
        }


        private async Task<IEnumerable<ActivityTaskDto>> GetActivityTasks(string serverRole)
        {
            var agent = GetCurrentAgent();

            var activites = await _activityRepository.GetActivityTasks(agent.LastDeploymentId == null ? 0 : agent.LastDeploymentId.Value, serverRole);

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


        private void UpdateAgentsLastDeployment(IEnumerable<ActivityTaskDto> activityTaskDtos)
        {
            var agent = GetCurrentAgent();
            agent.LastDeploymentId = activityTaskDtos.First().DeploymentId;

            _agentRepository.Update(agent);
        }


        private Agent GetCurrentAgent()
        {
            Agent agent = null;

            if (_connectedAgents.ContainsKey(Context.ConnectionId))
                agent = _connectedAgents[Context.ConnectionId];

            if (agent == null)
                throw new NotAuthorizedException("Agent isn't authorized");

            return agent;
        }


        private void SetCurrentAgentToBeAvailable()
        {
            var agent = GetCurrentAgent();
            agent.IsBusy = false;
        }


        private void SetCurrentAgentToBusy()
        {
            var agent = GetCurrentAgent();
            agent.IsBusy = true;
        }


        private static bool AreAllAgentsDoneWithActivity(long activityTaskId)
        {
            return _connectedAgents.Values.Any(a => !a.IsBusy && a.IsActive && a.LastDeploymentId == activityTaskId);
        }


        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}