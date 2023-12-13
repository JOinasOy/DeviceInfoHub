using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DeviceInfoHub.Function
{
    public class FetchData2
    {
        private readonly ILogger _logger;

        public FetchData2(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FetchData2>();
        }

        [Function("FetchData2")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
