using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using DeviceInfoHub.DataModels;
using Newtonsoft.Json;
using Microsoft.Graph.DeviceAppManagement.MobileAppConfigurations.Item.DeviceStatusSummary;

namespace DeviceInfoHub
{
    public class GetData
    {
        [Function("GetData")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpTriggerFunction");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            IntuneDevice intDevice = new IntuneDevice();
            
            string devicesSerialized = "null";

            using (var context = new IntuneDeviceDbContext())
            {
                List<IntuneDevice> devices = context.intuneDevice.ToList();
                if (devices != null)
                {
                    devicesSerialized = JsonConvert.SerializeObject(devices);
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/json; charset=utf-8");

            response.WriteString(devicesSerialized);
            return response;
        }
    }
}
