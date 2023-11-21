using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DeviceInfoHub.DataModels;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;
using Azure.Identity;
using Microsoft.Graph.Devices;

namespace DeviceInfoHub.Function
{
    public class FetchData
    {
        [Function("FetchData")]
        public static async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpTriggerFunction");

            logger.LogInformation("C# HTTP trigger function processed a request.");

            var clientId = "4d96f2b9-eae1-4df2-8f49-67f589076813";
            var clientSecret = "l7-8Q~2cJJPrXWhfEPNmi4PM2.w87yHvO2Jfea20";
            var tenantId = "329ee4c5-0231-4d7a-9995-a4c64fa06a7b";

            var graphClient = CreateGraphClient(tenantId, clientId, clientSecret);
            var user = await graphClient.Users["info@janneoinas.fi"].GetAsync();
            var dispName = user?.DisplayName;
            var devices = await graphClient.Devices.GetAsync();

            IntuneDevice intDevice = new IntuneDevice();

            foreach (var device in devices.Value)
            {
                intDevice.Id = device.Id;
                intDevice.DisplayName = device.DisplayName;
                intDevice.EnrolledDateTime = device.RegistrationDateTime.Value.DateTime;
                intDevice.OperatingSystem = device.OperatingSystem + " " + device.OperatingSystemVersion;
            }

            using (var context = new IntuneDeviceDbContext())
            {
                context.Database.EnsureCreated();
                context.intuneDevice.Add(intDevice);
                context.SaveChanges();
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }

        private static GraphServiceClient CreateGraphClient(string tenantId, string clientId, string clientSecret)
        {
            var options = new TokenCredentialOptions
            {   
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            return new GraphServiceClient(clientSecretCredential, scopes);
        }

        private static void readJsonToObject()
        {
            string jsonFilePath = "JSONData/IntuneDevice.json"; // Replace with the actual file path.

            string json = System.IO.File.ReadAllText(jsonFilePath);

            IntuneDevice device = JsonConvert.DeserializeObject<IntuneDevice>(json);

            using (var context = new IntuneDeviceDbContext())
            {
                context.Database.EnsureCreated();
                context.intuneDevice.Add(device);
                context.SaveChanges();
            }
        }
    }
}
