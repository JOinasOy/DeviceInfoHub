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
    /// <summary>
    /// Api Gateway is Azure function which provides data from the database and updates data to the database. 
    /// </summary>
    public class ApiGateway
    {
        // Azure Function triggered by HTTP requests (GET or POST) with a specific authorization level
        [Function("ApiGateway")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "data/{id}")] HttpRequestData req, string id, FunctionContext executionContext)
        {
            string jsonData = "";

            // Initialize logger to log information during function execution
            var logger = executionContext.GetLogger("HttpTriggerFunction");
            logger.LogInformation($"Processing GET request for data with ID: {id}");

            // Try to get company id from the request
            string? companyId = req.Query["CompanyId"];

            try
            {
                // Select case by request method
                switch (req.Method)
                {
                    case "GET":

                        // Get device data from database and return it in JSON
                        if (id == "GetDevices") 
                        {
                            jsonData = GetDevicesFunction(companyId).Result;

                        }
                        // Get company data from database and return it in JSON
                        if (id == "GetCompany") 
                        {
                            jsonData = GetCompanyFunction(companyId).Result;
                        }
                        break;

                    case "POST":
                        // Save or update company details to the database
                        if (id == "SaveCompany") 
                        {   
                            jsonData = SaveCompanyFunction(req.Headers).Result;
                        }
                        break;
                }

                // Create an HTTP response with status code 200 OK
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                response.WriteString(jsonData);

                return response;
            }
            catch (Exception ex) // In case an error occured
            {
                ResponseInfo resInfo = new ResponseInfo(req.Method + ": " + id + " failed");
                resInfo.Details = new { ErrorText = ex.Message };
                logger.LogError($"An error occurred: {ex.Message}");
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                response.WriteString(resInfo.ToJson());
                return response;
            }
        }

        /// <summary>
        /// Gets device data from the database
        /// </summary>
        /// <param name="companyId">defined company id</param>
        /// <returns>All or defined company devices</returns>
        private static async Task<string> GetDevicesFunction(string companyId)
        {
            string jsonData = "";
            List<DataModels.Device> devices;

            using (var context = new DeviceDbContext())
            {
                // Company id is not defined
                if (string.IsNullOrEmpty(companyId))
                {
                    devices = await context.device.ToListAsync();
                }
                else // Company id is defined
                {
                    // Get all devices with defined company id in the database
                    devices = await context.device.Where(e => e.CompanyId == int.Parse(companyId)).ToListAsync();
                }
                // Serialize object to JSON
                jsonData = JsonConvert.SerializeObject(devices);
            }
            return jsonData;
        }

        /// <summary>
        /// Gets company data from the database
        /// </summary>
        /// <param name="companyId">defined company id</param>
        /// <returns>All or defined company details</returns>
        private static async Task<string> GetCompanyFunction(string companyId)
        {
            string jsonData = "";

            using (var context = new CompanyDbContext())
            {
                // Get companies from the database in object with has no secret values
                List<CompaniesDto> companies = await context.company.Select(e => new CompaniesDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ClientId = !string.IsNullOrEmpty(e.ClientId),
                    TenantId = !string.IsNullOrEmpty(e.TenantId),
                    ClientSecret = !string.IsNullOrEmpty(e.ClientSecret),
                    KandjiApiKey = !string.IsNullOrEmpty(e.KandjiApiKey)
                }).ToListAsync();

                // Serialize object to JSON
                jsonData = JsonConvert.SerializeObject(companies);

                // Company id is defined
                if (!string.IsNullOrEmpty(companyId))
                {
                    // Get company details 
                    var company = companies.Where(e => e.Id == int.Parse(companyId));
                    // Serialize object to JSON
                    jsonData = JsonConvert.SerializeObject(company);
                }
            }
            return jsonData;
        }

        /// <summary>
        /// Saves or updates company details in the database
        /// </summary>
        /// <param name="headers">header parameters</param>
        /// <returns>JSON data</returns>
        private static async Task<string> SaveCompanyFunction(HttpHeadersCollection headers)
        {
            ResponseInfo resInfo = new ResponseInfo("");
            Company company = new Company();

            // Retrieve the encryption key for the database
            var DBCryptKey = Environment.GetEnvironmentVariable("DBCryptKey");
            
            // Check if the DBCryptKey is available, otherwise log an error
            if (string.IsNullOrEmpty(DBCryptKey)) {
                resInfo.Message = "Checking for the DBCryptKey failed";
                return resInfo.ToJson();
            }

            // Try to get company id from the header parameters
            if(headers.TryGetValues("CompanyId", out var id))
            {
                // Set current company id
                company.Id = int.Parse(id.First());
            }

            using (var context = new CompanyDbContext())
            {
                // If company exists in the database
                var deviceExists = await context.company.FirstOrDefaultAsync(u => u.Id == company.Id);
                if (deviceExists != null)
                {
                    // Set current company values with existing company values
                    company = deviceExists;
                }

                // Update company values
                UpdateCompanyFromHeaders(company, headers, DBCryptKey);

                // Set last updated value to current date and time
                company.LastUpdated = DateTime.Now;
                // Set DBCryptKey to null value for security reasons
                DBCryptKey = null;
                
                // If device added
                if (deviceExists == null)
                {    
                    // set company id 
                    company.Id = 0;
                    // Add company to the database context
                    context.company.Add(company);
                    // Set return value
                    resInfo.Message = "Company added succesfully!";
                }
                else // If device updated
                {
                    // Update company to the database context
                    context.company.Update(company);
                    // Set return value
                    resInfo.Message = "Company updated succesfully!";
                }
                // Save changes to the database
                context.SaveChanges();
            
            }

            return resInfo.ToJson();
        }

        /// <summary>
        /// Updates a Company object with information from HTTP headers.
        /// This method parses the headers for specific keys related to company details,
        /// such as company name, client ID, client secret, tenant ID, Kandji API key, and 
        /// archived status. Where applicable, it encrypts certain values using the provided 
        /// DBCryptKey before updating the Company object. 
        /// The method assumes that the necessary headers are present and correctly formatted.
        /// </summary>
        /// <param name="company">The Company object to be updated.</param>
        /// <param name="headers">The HttpHeadersCollection containing the header parameters.</param>
        /// <param name="DBCryptKey">The encryption key used for encrypting sensitive information.</param>
        private static void UpdateCompanyFromHeaders(Company company, HttpHeadersCollection headers, string DBCryptKey)
        {
            // Try to get company name from the header parameters
            if(headers.TryGetValues("CompanyName", out var name))
            {
                // Set current company name
                company.Name = name.First();
            }
            // Try to get client id from the header parameters
            if(headers.TryGetValues("ClientId", out var clientId))
            {
                // Encrypt client id and set value
                company.ClientId = EncryptionHelper.EncryptString(DBCryptKey, clientId.First());
            }
            // Try to get client secret from the header parameters
            if(headers.TryGetValues("ClientSecret", out var clientSecret))
            {
                // Encrypt client secret and set value
                company.ClientSecret = EncryptionHelper.EncryptString(DBCryptKey, clientSecret.First());
            }
            // Try to get tenant id from the header parameters
            if(headers.TryGetValues("TenantId", out var tenantId))
            {
                // Encrypt tenant id and set value
                company.TenantId = EncryptionHelper.EncryptString(DBCryptKey, tenantId.First());
            }
            // Try to get kandji api key from the header parameters
            if(headers.TryGetValues("KandjiApiKey", out var kandjiApiKey))
            {
                // Encrypt kandji api key and set value
                company.KandjiApiKey = EncryptionHelper.EncryptString(DBCryptKey, kandjiApiKey.First());
            }
            // Try to get archived value from the header parameters
            if(headers.TryGetValues("Archived", out var archived))
            {
                // Set archived value
                company.Archived = bool.Parse(archived.First());
            }
        }
    }
}

