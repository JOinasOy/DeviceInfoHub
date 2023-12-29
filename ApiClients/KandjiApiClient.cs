using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DeviceInfoHub.DataModels;
using DeviceInfoHub.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DeviceInfoHub.ApiClients
{
    /// <summary>
    /// Client for interacting with Kandji API to fetch data related to organizations and devices.
    /// </summary>
    public class KandjiApiClient
    {
        private static HttpClient _httpClient;
        private static string _apiKey;
        
        /// <summary>
        /// Initializes the Kandji API client with the provided API key.
        /// </summary>
        /// <param name="apiKey">The API key for Kandji authentication.</param>
        public static void Initialize(string apiKey)
        {
            // Retrieve the encryption key for the database
            string? DBCryptKey = Environment.GetEnvironmentVariable("DBCryptKey");
            
            // Check if the DBCryptKey is available, otherwise log an error
            if (string.IsNullOrEmpty(DBCryptKey)) {
                Console.WriteLine("Error: Check DBCryptKey!");
                return;
            }
            
            // Decrypt the provided API key using the encryption key
            apiKey = EncryptionHelper.DecryptString(DBCryptKey, apiKey);
            DBCryptKey = null;
            
            // Set the API key and initialize the HttpClient
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }   

        /// <summary>
        /// Retrieves a list of devices from Kandji API for a specific company.
        /// </summary>
        /// <param name="companyId">The ID of the company to retrieve devices for.</param>
        /// <returns>A list of devices.</returns>
        public static async Task<List<DataModels.Device>> GetDevices(int companyId)
        {
            try
            {
                // Ensure the KandjiApiClient has been initialized
                if (_httpClient == null)
                {
                    throw new InvalidOperationException("KandjiApiClient has not been initialized. Call Initialize method first.");
                }

                // Send an HTTP GET request to retrieve devices
                var response = await _httpClient.GetAsync("https://simplified.api.eu.kandji.io/api/v1/devices");
                response.EnsureSuccessStatusCode();

                // Deserialize the JSON response into a list of KandjiDevice objects
                var devices = JsonConvert.DeserializeObject<List<KandjiDevice>>(await response.Content.ReadAsStringAsync());
                var resultDevices = new List<DataModels.Device>();
                
                // Process each KandjiDevice and map it to the internal DataModels.Device
                foreach (var device in devices)
                {
                    var resultDevice = new DataModels.Device
                    {
                        DeviceId = device.device_id,
                        CompanyId = companyId,
                        SerialNumber = device.serial_number,
                        DeviceName = device.device_name,
                        FirstEnrollment = device.first_enrollment,
                        Platform = device.platform,
                        OsVersion = device.os_version,
                        Manufacturer = "",
                        Model = device.model,
                        TotalStorageSpaceInBytes = 0,
                        FreeStorageSpaceInBytes = 0,
                        PhysicalMemoryInBytes = 0,
                        Source = "Kandji",
                        DBLastUpdated = DateTime.Now
                    };
                    
                    using (var context = new UsersDbContext())
                    {
                        // Check if device user already exists in the database
                        var user = context.users.Where(u => u.UserPrincipalName == device.user && u.CompanyId == companyId);
                        
                        // User exists in the database. Set the Device UserId 
                        if (user.Count() > 0)
                        {
                            resultDevice.UserId = user.First().Id;
                        }
                        else // User doesn't exists in the database. Create a new user to the database 
                        {
                            var newUser = new Users
                            {
                                UserId = "",
                                CompanyId = companyId,
                                DisplayName = "EMPTY",
                                UserPrincipalName = device.user,
                                Email = "",
                                GivenName = "",
                                Department = "",
                                LastUpdated = DateTime.Now
                            };
                            
                            context.users.Add(newUser);
                            // Save changes to the database
                            context.SaveChanges();
                            
                            // Gets just created user id value from the database and updates it to device userid value
                            user = context.users.Where(u => u.UserPrincipalName == device.user && u.CompanyId == companyId);
                            resultDevice.UserId = user.First().Id;
                        }
                    }

                    resultDevices.Add(resultDevice);
                }
                // return list of devices
                return resultDevices;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error: {ex.Message}");
                return new List<DataModels.Device>();
            }
        }
    }

    /// <summary>
    /// Represents a device data model for Kandji API response.
    /// </summary>
    public class KandjiDevice
    {
        public string device_id { get; set; }
        public string device_name { get; set; }
        public string model { get; set; }
        public string serial_number { get; set; }
        public string platform { get; set; }
        public string os_version { get; set; }
        public string supplemental_build_version { get; set; }
        public string supplemental_os_version_extra { get; set; }
        public DateTime last_check_in { get; set; }
        public string user { get; set; }
        public string asset_tag { get; set; }
        public string blueprint_id { get; set; }
        public bool mdm_enabled { get; set; }
        public bool agent_installed { get; set; }
        public bool is_missing { get; set; }
        public bool is_removed { get; set; }
        public string agent_version { get; set; }
        public DateTime first_enrollment { get; set; }
        public DateTime last_enrollment { get; set; }
        public string blueprint_name { get; set; }
        public string lost_mode_status { get; set; }
    }
}
