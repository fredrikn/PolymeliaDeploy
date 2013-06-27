namespace PolymeliaDeploy.Controller
{
    using System;
    using System.Configuration;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public class ControllerClientFactory : IControllerClientFactory
    {
        public HttpClient CreateWebHttpClient()
        {
            var baseUri = ConfigurationManager.AppSettings["PolymeliaDeployBaseWebUri"];

            var client = new HttpClient { BaseAddress = new Uri(baseUri) };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}
