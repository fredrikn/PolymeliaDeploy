using System.Collections.Generic;

namespace PolymeliaDeploy.Controller
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Formatting;
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


        public Agent Add(Agent agent)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                try
                {
                    var response = client.PutAsync("/agents/", new ObjectContent(
                                            typeof(Agent),
                                            agent,
                                            new JsonMediaTypeFormatter()
                                            )).Result;

                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<Agent>().Result;

                    throw new HttpRequestException(
                        string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Can't access deploy controller or read its results. " + e);
                    throw;
                }
            }
        }
    }
}
