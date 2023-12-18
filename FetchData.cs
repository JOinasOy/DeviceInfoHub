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
            
            List<Company> companies = new List<Company>();

            using (var context = new CompanyDbContext())
            {
                companies = await context.company.ToListAsync();
            }

            foreach (var company in companies)
            {

                if (!string.IsNullOrEmpty(company.ClientId) && !string.IsNullOrEmpty(company.ClientSecret) && !string.IsNullOrEmpty(company.TenantId))
                {
                    GraphApiClient.Initialize(company.TenantId, company.ClientId, company.ClientSecret);
                    var users = await GraphApiClient.GetUsers();

                    foreach (var user in users)
                    {
                        var userDevices = await GraphApiClient.GetUserDevices(company.Id, user);

                        saveDevicesToDB(company, userDevices);
                    }
                }

                if (!string.IsNullOrEmpty(company.KandjiApiKey))
                {
                    KandjiApiClient.Initialize(company.KandjiApiKey);

                    var userDevices = await KandjiApiClient.GetDevices(company.Id);

                    saveDevicesToDB(company, userDevices);
                }
            }
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Executed succesfully!");

            return response;
        }

        public static void saveDevicesToDB(Company company, List<DataModels.Device> userDevices)
        {
            using (var context = new DeviceDbContext())
            {
                foreach (var device in userDevices)
                {

                    context.Database.EnsureCreated();
                    var item = context.device.Where(u => u.DeviceId == device.DeviceId && u.CompanyId == company.Id);

                    if (item.Count() == 0)
                    {
                        context.device.Add(device);
                    }
                    else
                    {
                        var firstItem = item.First();
                        if (device.CompanyId == firstItem.CompanyId && device.DeviceId == firstItem.DeviceId)
                        {
                            device.Id = firstItem.Id;
                            if (device.isUpdated(firstItem))
                            {  
                                context.Entry(item.First()).CurrentValues.SetValues(device);
                            }
                        }
                    }
                    
                    context.SaveChanges();
                }               
            }
        }
    }

}
