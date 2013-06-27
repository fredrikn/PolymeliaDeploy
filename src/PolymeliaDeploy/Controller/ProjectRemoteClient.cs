using System;
using System.Collections.Generic;

namespace PolymeliaDeploy.Controller
{
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;

    using PolymeliaDeploy.Data;


    public class ProjectRemoteClient : IProjectClient
    {
        HttpClient _client = null;


        public async Task<IEnumerable<Project>> GetAllProjects()
        {
            var client = GetDeployWebHttpClient();
            
            var response = await client.GetAsync("/projects/");

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<IEnumerable<Project>>().Result;

            throw new HttpRequestException(
                string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
        }


        public void DeleteProject(Project project)
        {
            var client = GetDeployWebHttpClient();

            try
            {
                var response = client.DeleteAsync(string.Format("/projects/{0}",project.Id)).Result;

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


        public Project CreateProject(Project project, int? copyProjectId)
        {
            var client = GetDeployWebHttpClient();

            try
            {
                var uri = "/projects/";

                if (copyProjectId.HasValue)
                    uri = uri + copyProjectId.Value.ToString();

                var response = client.PutAsync(uri, new ObjectContent(
                                        typeof(Project),
                                        project,
                                        new JsonMediaTypeFormatter()
                                        )).Result;

                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsAsync<Project>().Result;

                throw new HttpRequestException(
                    string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            catch (Exception e)
            {
                Console.WriteLine("Can't access deploy controller or read its results. " + e);
                throw;
            }
        }


        private HttpClient GetDeployWebHttpClient()
        {
            if (_client == null)
                _client = new ControllerClientFactory().CreateWebHttpClient();

            return _client;
        }


        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}