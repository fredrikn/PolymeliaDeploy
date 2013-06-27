namespace PolymeliaDeploy.Controller
{
    using System.Net.Http;

    public interface IControllerClientFactory
    {
        HttpClient CreateWebHttpClient();
    }
}