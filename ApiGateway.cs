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
using DeviceInfoHub.Helpers;
using Microsoft.Graph.Models;

namespace DeviceInfoHub
{
    public class ApiGateway
    {
        [Function("ApiGateway")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "data/{id}")] HttpRequestData req, string id, FunctionContext executionContext)
        {
            string jsonData = "";
            var logger = executionContext.GetLogger("HttpTriggerFunction");

            logger.LogInformation($"Processing GET request for data with ID: {id}");
            string? companyId = req.Query["CompanyId"];

            try
            {
                switch (req.Method)
                {
                    case "GET":
                        if (id == "GetDevices") 
                        {
                            jsonData = GetDevicesFunction(companyId).Result;

                        }
                        if (id == "GetCompany") 
                        {
                            jsonData = GetCompanyFunction(companyId).Result;
                        }
                        break;

                    case "POST":
                        var headers = req.Headers;

                        if (id == "SaveCompany") 
                        {   
                            jsonData = SaveCompanyFunction(headers).Result;
                        }
                        break;
                }
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                response.WriteString(jsonData);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }


        }

        public static async Task<string> GetDevicesFunction(string companyId)
        {
            string jsonData = "";
            List<DataModels.Device> devices;

            using (var context = new DeviceDbContext())
            {
                if (string.IsNullOrEmpty(companyId))
                {
                    devices = await context.device.ToListAsync();
                }
                else
                {
                    devices = await context.device.Where(e => e.CompanyId == int.Parse(companyId)).ToListAsync();
                }
                jsonData = JsonConvert.SerializeObject(devices);
            }
            return jsonData;
        }

        public static async Task<string> GetCompanyFunction(string companyId)
        {
            string jsonData = "";

            using (var context = new CompanyDbContext())
            {
                List<CompaniesDto> companies = await context.company.Select(e => new CompaniesDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ClientId = !string.IsNullOrEmpty(e.ClientId),
                    TenantId = !string.IsNullOrEmpty(e.TenantId),
                    ClientSecret = !string.IsNullOrEmpty(e.ClientSecret),
                    KandjiApiKey = !string.IsNullOrEmpty(e.KandjiApiKey)
                }).ToListAsync();
                jsonData = JsonConvert.SerializeObject(companies);

                if (!string.IsNullOrEmpty(companyId))
                {
                    var company = companies.Where(e => e.Id == int.Parse(companyId));
                    jsonData = JsonConvert.SerializeObject(company);
                }
            }
            return jsonData;
        }

        public static async Task<string> SaveCompanyFunction(HttpHeadersCollection headers)
        {
            string jsonData = "";
            var DBCryptKey = Environment.GetEnvironmentVariable("DBCryptKey");
            
            if (string.IsNullOrEmpty(DBCryptKey)) {
                Console.WriteLine("Error: Check DBCryptKey!");
                return "Error";
            }

            Company company = new Company();

            if(headers.TryGetValues("CompanyId", out var id))
            {
                company.Id = int.Parse(id.First());
            }

            if(headers.TryGetValues("CompanyName", out var name))
            {
                company.Name = name.First();
            }
            if(headers.TryGetValues("ClientId", out var clientId))
            {
                company.ClientId = EncryptionHelper.EncryptString(DBCryptKey, clientId.First());
            }
            if(headers.TryGetValues("ClientSecret", out var clientSecret))
            {
                company.ClientSecret = EncryptionHelper.EncryptString(DBCryptKey, clientSecret.First());
            }
            if(headers.TryGetValues("TenantId", out var tenantId))
            {
                company.TenantId = EncryptionHelper.EncryptString(DBCryptKey, tenantId.First());
            }
            if(headers.TryGetValues("KandjiApiKey", out var kandjiApiKey))
            {
                company.KandjiApiKey = EncryptionHelper.EncryptString(DBCryptKey, kandjiApiKey.First());
            }
            if(headers.TryGetValues("Archived", out var archived))
            {
                company.Archived = bool.Parse(archived.First());
            }
            company.LastUpdated = DateTime.Now;
            DBCryptKey = null;

            using (var context = new CompanyDbContext())
            {
                context.Database.EnsureCreated();
                bool deviceExists = context.company.Any(u => u.Id == company.Id);
                
                if (!deviceExists)
                {    
                    company.Id = 0;
                    context.company.Add(company);
                    jsonData = "Company added succesfully!";
                }
                else
                {
                    context.company.Update(company);
                    jsonData = "Company updated succesfully!";
                }
                context.SaveChanges();
            }
            return jsonData;
        }
    }
}

