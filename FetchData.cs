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
    /// <summary>
    /// FetchData is Azure Function which fetch device data from different sources (e.g. GraphAPI, KandjiAPI) 
    /// </summary>
    public class FetchData
    {
        // Azure Function triggered by HTTP requests (GET) with a specific authorization level
        [Function("FetchData")]
        public static async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req, FunctionContext executionContext)
        {
            // Initialize logger to log information during function execution
            var logger = executionContext.GetLogger("HttpTriggerFunction");
            logger.LogInformation("C# HTTP trigger function processed a request.");
            
            // Get companies from the database
            List<Company> companies = new List<Company>();
            using (var context = new CompanyDbContext())
            {
                companies = await context.company.ToListAsync();
            }

            // Iterate over each company and query device data from sources if not archived
            foreach (var company in companies)
            {
                // If company is not arhived
                if (!company.Archived)
                {
                    // If company has valid Graph API credentials, initialize GraphApiClient and fetch devices
                    if (!string.IsNullOrEmpty(company.ClientId) && !string.IsNullOrEmpty(company.ClientSecret) && !string.IsNullOrEmpty(company.TenantId))
                    {
                        GraphApiClient.Initialize(company.TenantId, company.ClientId, company.ClientSecret);

                        var devices = await GraphApiClient.GetDevices(company.Id);

                        saveDevicesToDB(company, devices);
                    }

                    // If company has a Kandji API key, initialize KandjiApiClient and fetch devices
                    if (!string.IsNullOrEmpty(company.KandjiApiKey))
                    {
                        KandjiApiClient.Initialize(company.KandjiApiKey);

                        var devices = await KandjiApiClient.GetDevices(company.Id);

                        saveDevicesToDB(company, devices);
                    }
                }
            }

            // Create an HTTP response with status code 200 OK
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Executed succesfully!");

            return response;
        }

        // Method to save or update device data in the database
        public static void saveDevicesToDB(Company company, List<DataModels.Device> devices)
        {
            using (var context = new DeviceDbContext())
            {
                foreach (var device in devices)
                {
                    // Check if the device of current company already exists in the database
                    var item = context.device.Where(u => u.DeviceId == device.DeviceId && u.CompanyId == company.Id);

                    // If device does not exist, add it to the database
                    if (item.Count() == 0)
                    {
                        context.device.Add(device);
                    }
                    else // If device exists, update it if necessary
                    {
                        var firstItem = item.First();
                        // Check if current company and device id matches
                        if (device.CompanyId == firstItem.CompanyId && device.DeviceId == firstItem.DeviceId)
                        {
                            // Update current device id 
                            device.Id = firstItem.Id;

                            // Determines if the current device data is updated compared to another device instance.
                            if (device.isUpdated(firstItem))
                            {  
                                context.Entry(item.First()).CurrentValues.SetValues(device);
                            }
                        }
                    }

                    // Save changes to the database
                    context.SaveChanges();
                }               
            }
        }
    }

}
