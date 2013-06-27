using System.Collections.Generic;

namespace PolymeliaDeploy.Controller
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;

    using Environment = PolymeliaDeploy.Data.Environment;

    public class EnvironmentRemoteClient : IEnvironmentClient
    {

        public Environment AddEnvironment(Environment environment)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                try
                {
                    var response = client.PutAsync("/environments/", new ObjectContent(
                                            typeof(Environment),
                                            environment,
                                            new JsonMediaTypeFormatter()
                                            )).Result;

                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<Environment>().Result;

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


        public void UpdateEnvironment(Environment environment)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                try
                {
                    var response = client.PostAsync("/environments/", new ObjectContent(
                                            typeof(Environment),
                                            environment,
                                            new JsonMediaTypeFormatter()
                                            )).Result;

                    if (response.IsSuccessStatusCode)
                        return;

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


        public async Task<IEnumerable<Environment>> GetProjectsEnvironments(int projectId)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                var requestUrl = string.Format("/environments/{0}", projectId);

                var response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsAsync<IEnumerable<Environment>>();

                throw new HttpRequestException(
                   string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
        }


        public async Task<Environment> GetEnvironment(int environmentId)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                var requestUrl = string.Format("/environment/{0}", environmentId);

                var response = client.GetAsync(requestUrl).Result;
                
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsAsync<Environment>();
                
                throw new HttpRequestException(
                   string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
        }


        public void DeleteEnvironment(Environment environment)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                var requestUrl = string.Format("/environments/{0}", environment.Id);

                try
                {
                    var response = client.DeleteAsync(requestUrl).Result;

                    if (response.IsSuccessStatusCode)
                        return;

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
