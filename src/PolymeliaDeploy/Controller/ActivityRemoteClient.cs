using System;
using System.Collections.Generic;

namespace PolymeliaDeploy.Controller
{
    using System.Configuration;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using PolymeliaDeploy.ApiDto;
    using PolymeliaDeploy.Data;


    public class ActivityRemoteClient : IActivityClient
    {
        HttpClient _client = null;


        public MainActivity LatestDeployedActivity(int projectId, int environmentId)
        {
             var client = GetDeployWebHttpClient();

            var requestUrl = string.Format("deploys/last/{0}/{1}", projectId, environmentId);

            try
            {
                var response = client.GetAsync(requestUrl).Result;

                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsAsync<MainActivity>().Result;

                throw new HttpRequestException(
                    string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            catch (Exception e)
            {
                Console.WriteLine("Can't access deploy controller or read its results. " + e);
                throw;
            }
        }


        public async Task<IEnumerable<MainActivity>> GetDeployHistory(int projectId, int environmentId)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                var requestUrl = string.Format("deploys/history/{0}/{1}", projectId, environmentId);

                var response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsAsync<IEnumerable<MainActivity>>();

                throw new HttpRequestException(
                    string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
        }


        public long Deploy(MainActivity mainActivity)
        {
            var client = GetDeployWebHttpClient();

            var response = client.PutAsync("/deploy/", 
                new ObjectContent(typeof(MainActivity), mainActivity, new JsonMediaTypeFormatter())).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<long>().Result;

            throw new HttpRequestException(
                string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
        }


        public IEnumerable<ActivityTaskDto> GetActivityTasksFromDeployController(long latestTaskRunId, string serverRoleName)
        {
            var client = GetDeployWebHttpClient();

            var requestUrl = string.Format("/activities/{0}/{1}", serverRoleName, latestTaskRunId);

            try
            {
                var response = client.GetAsync(requestUrl).Result;

                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsAsync<IEnumerable<ActivityTaskDto>>().Result;

                throw new HttpRequestException(
                    string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            catch (Exception e)
            {
                Console.WriteLine("Can't access deploy controller or read its results. " + e);
                throw;
            }
        }


        public void UpdateActivityTaskStatus(ActivityTaskDto activityTask)
        {
            var requestUrl = string.Format(
                "/activity/{0}/{1}", activityTask.Id, activityTask.Status.ToString());

            var client = GetDeployWebHttpClient();

            var response = client.PostAsync(requestUrl, null).Result;

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
        }


        private HttpClient GetDeployWebHttpClient()
        {
            if (_client == null)
            {
                var baseUri = ConfigurationManager.AppSettings["PolymeliaDeployBaseWebUri"];

                var client = new HttpClient { BaseAddress = new Uri(baseUri) };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _client = client;
            }

            return _client;
        }


        public void Dispose()
        {
            _client.Dispose();
        }
    }
}