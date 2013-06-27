using System;

namespace PolymeliaDeploy.Controller
{
    using System.Configuration;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;

    using PolymeliaDeploy.Data;
    using System.Collections.Generic;


    public class ReportRemoteClient : IReportClient
    {
        HttpClient _client = null;


        public IEnumerable<ActivityReport> GetReports(long taskId, long? fromLatestTaskId)
        {
            //"reports/{TaskId}/{FromLastKnownId?}"

            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                try
                {
                    var uri = string.Format("/reports/{0}/{1}", taskId, fromLatestTaskId.HasValue ? fromLatestTaskId.Value.ToString() : string.Empty);

                    var response = client.GetAsync(uri).Result;

                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<IEnumerable<ActivityReport>>().Result;

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


        public void Report(
                           long taskId,
                           string serverRole, 
                           string message, 
                           string activityName, 
                           ReportStatus reportStatus = ReportStatus.Information)
        {
            var report = new ActivityReport
            {
                TaskId = taskId,
                LocalCreated = DateTime.Now,
                MachineName = System.Environment.MachineName,
                Message = message,
                ServerRole = serverRole,
                ActivityName = activityName,
                Status = reportStatus,
                ActivityTaskId = AgentEnvironment.CurrentActivityId
            };

            var client = GetDeployWebHttpClient();

            var response = client.PutAsync("report",
                                    new ObjectContent(
                                        typeof(ActivityReport),
                                        report,
                                        new JsonMediaTypeFormatter()
                                        )).Result;

            if (!response.IsSuccessStatusCode)
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
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