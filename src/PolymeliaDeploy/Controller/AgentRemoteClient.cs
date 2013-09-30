using System.Collections.Generic;

namespace PolymeliaDeploy.Controller
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using PolymeliaDeploy.Data;

    public class AgentRemoteClient : IAgentRemoteClient
    {
        public async Task<IEnumerable<Agent>> GetAll()
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                var requestUrl = string.Format("agents");

                var response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsAsync<IEnumerable<Agent>>();

                throw new HttpRequestException(
                    string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
        }


        public async Task<IEnumerable<Agent>> GetAllUnassigned()
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                var requestUrl = string.Format("agents/unassigned");

                var response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsAsync<IEnumerable<Agent>>();

                throw new HttpRequestException(
                    string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
        }
    }
}
