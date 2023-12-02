using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DeviceInfoHub.DataModels;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DeviceInfoHub
{
    public class GetData
    {
        [Function("GetData")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
             var logger = executionContext.GetLogger("HttpTriggerFunction");

             logger.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
                string devicesSerialized;

                using (var context = new DeviceDbContext())
                {
                    List<Device> devices = await context.device.ToListAsync();
                    devicesSerialized = JsonConvert.SerializeObject(devices);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                response.WriteString(devicesSerialized);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
