using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DeviceInfoHub.DataModels;
using DeviceInfoHub.ApiClients;
using DeviceInfoHub.Helpers;
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
            var DBCryptKey = Environment.GetEnvironmentVariable("DBCryptKey");
            
            List<Company> customers = new List<Company>();

            using (var context = new CompanyDbContext())
            {
                customers = await context.company.ToListAsync();
            }

            foreach (var customer in customers)
            {

                if (!string.IsNullOrEmpty(customer.ClientId) && !string.IsNullOrEmpty(customer.ClientSecret) && !string.IsNullOrEmpty(customer.TenantId))
                {
                    customer.ClientId = EncryptionHelper.DecryptString(DBCryptKey, customer.ClientId);
                    customer.ClientSecret = EncryptionHelper.DecryptString(DBCryptKey, customer.ClientSecret);
                    customer.TenantId = EncryptionHelper.DecryptString(DBCryptKey, customer.TenantId);
                    
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

                if (!string.IsNullOrEmpty(customer.KandjiApiKey))
                {
                    customer.KandjiApiKey = EncryptionHelper.DecryptString(DBCryptKey, customer.KandjiApiKey);
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
