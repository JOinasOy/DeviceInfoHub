using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DeviceInfoHub.DataModels;
using DeviceInfoHub.ApiClients;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;
using Azure.Identity;
using Microsoft.Graph.Devices;
using Microsoft.Graph.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceInfoHub.Function
{
    public class FetchData
    {
        [Function("FetchData")]
        public static async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpTriggerFunction");

            logger.LogInformation("C# HTTP trigger function processed a request.");

            List<Customer> customers = new List<Customer>();

            using (var context = new CustomerDbContext())
            {
                customers = await context.customer.ToListAsync();
            }

            foreach (var customer in customers)
            {
                if (customer.ClientId != null && customer.ClientSecret != null && customer.TenantId != null)
                {
                    GraphApiClient.Initialize(customer.TenantId, customer.ClientId, customer.ClientSecret);
                    var users = await GraphApiClient.GetUsers();

                    foreach (var user in users)
                    {
                        var userDevices = await GraphApiClient.GetUserDevices(customer.Id, user);

                        foreach (var device in userDevices)
                        {
                            using (var context = new DeviceDbContext())
                            {
                                bool deviceExists = context.device.Any(u => u.Id == device.Id);
                                if (!deviceExists)
                                {
                                    context.Database.EnsureCreated();
                                    context.device.Add(device);
                                    context.SaveChanges();
                                }
                            }
                        }
                    }
                }

                if (customer.KandjiApiKey != null)
                {
                    KandjiApiClient.Initialize(customer.KandjiApiKey);

                    var userDevices = await KandjiApiClient.GetDevices(customer.Id);

                    foreach (var device in userDevices)
                    {
                        using (var context = new DeviceDbContext())
                        {
                            bool deviceExists = context.device.Any(u => u.Id == device.Id);
                            
                            if (!deviceExists)
                            {
                                context.Database.EnsureCreated();
                                context.device.Add(device);
                                context.SaveChanges();
                            }
                        }
                    }
                }
            }
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Executed succesfully!");

            return response;
        }
    }
}
