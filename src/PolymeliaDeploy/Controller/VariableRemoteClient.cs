using System.Collections.Generic;

namespace PolymeliaDeploy.Controller
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net.Http;
    using System.Net.Http.Formatting;

    using PolymeliaDeploy.Data;

    using Environment = PolymeliaDeploy.Data.Environment;

    public class VariableRemoteClient : IVariableClient
    {

        public IEnumerable<Variable> GetAllVariables(int environmentId)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                try
                {
                    var uri = string.Format("/variables/{0}", environmentId);

                    var response = client.GetAsync(uri).Result;

                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<IEnumerable<Variable>>().Result;

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


        public void AddVariables(IEnumerable<Variable> variables, int environmentId)
        {
            using (var client = new ControllerClientFactory().CreateWebHttpClient())
            {
                try
                {
                    var uri = string.Format("/variables/{0}", environmentId);

                    var response = client.PutAsync(uri, new ObjectContent(
                                            typeof(Collection<Variable>),
                                            variables,
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
    }
}
