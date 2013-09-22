using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;

namespace PolymeliaDeployController.Hub
{
    using PolymeliaDeploy.Data.Repositories;

    public class DeployControllerHub : Microsoft.AspNet.SignalR.Hub
    {
        private readonly IDictionary<string, string> _connectedAgents = new Dictionary<string, string>();

        private readonly IReportRepository _reportRepository;
        private readonly IActivityRepository _activityRepository;

        public DeployControllerHub(
                                   IReportRepository reportRepository,
                                   IActivityRepository activityRepository)
        {
            if (reportRepository == null) throw new ArgumentNullException("reportRepository");
            if (activityRepository == null) throw new ArgumentNullException("activityRepository");

            _reportRepository = reportRepository;
            _activityRepository = activityRepository;
        }
        

        //TODO: Add some sort of key/certificate, authentication
        public void Connect(string roleName, string connectionId)
        {
            RegisterAgent(roleName, connectionId);

            Console.WriteLine("Agent from IP: '{0}' with role: '{1}' is now connected", GetAgentIpAddress(), roleName);
        }


        public void Deploy()
        {
            //Make sure Client connects to this hub and call this deploy method

            //Run Workflow

            //Get Agents for each ServerRole, send activities, register for completion. When all agents are green, the mark deployment as succeeded.
        }


        public async Task AgentIsReadyForNewTasks(long lastTaskId, string roleName, string connectionId)
        {
            await CheckForAgentActivityAndRunActivities(roleName, lastTaskId, connectionId);
        }


        public async Task Report(ActivityReport report)
        {
            await _reportRepository.AddReport(report);
        }


        public async Task UpdateActivityTaskStatus(long activityTaskId, ActivityStatus status)
        {
            await _activityRepository.UpdateActivityTaskStatus(activityTaskId, status);
        }


        public override Task OnDisconnected()
        {
            var agentKey = _connectedAgents.Where(a => a.Value == Context.ConnectionId)
                                           .Select( a => a.Key).FirstOrDefault();

            if (agentKey != null)
                _connectedAgents.Remove(agentKey);

            Console.WriteLine(string.Format("An Agent is now disconnected", GetAgentIpAddress()));

            return base.OnDisconnected();
        }


        protected string GetAgentIpAddress()
        {
            var env = Get<IDictionary<string, object>>(Context.Request.Items, "owin.environment");

            return env == null ? null : Get<string>(env, "server.RemoteIpAddress");
        }


        private void RegisterAgent(string roleName, string connectionId)
        {
            var agentId = string.Format("{0}_{1}", roleName, connectionId);

            if (!_connectedAgents.ContainsKey(agentId))
                _connectedAgents.Add(agentId, Context.ConnectionId);
        }


        private async Task CheckForAgentActivityAndRunActivities(string roleName, long lastTaskId, string connectionId)
        {
            var activityTasks = await GetActivityTasks(lastTaskId, roleName);

            var activityTaskDtos = activityTasks as IList<ActivityTaskDto> ?? activityTasks.ToList();

            if (activityTaskDtos.Any())
                await Clients.Client(connectionId).RunActivities(activityTaskDtos);
        }

        private async Task<IEnumerable<ActivityTaskDto>> GetActivityTasks(long lastTaskId, string serverRole)
        {
            var activites = await _activityRepository.GetActivityTasks(lastTaskId, serverRole);

            var actititiesToRun = activites.ToList().Select(a => new ActivityTaskDto
            {
                TaskId = a.TaskId,
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


        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}