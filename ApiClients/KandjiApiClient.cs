using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DeviceInfoHub.DataModels;
using Newtonsoft.Json;

namespace DeviceInfoHub.ApiClients
{
    public class KandjiApiClient
    {
        private static HttpClient _httpClient;
        private static string _apiKey;

        public static void Initialize(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public static async Task<List<string>> GetOrganizations()
        {
            try
            {
                if (_httpClient == null)
                {
                    throw new InvalidOperationException("KandjiApiClient has not been initialized. Call Initialize method first.");
                }

                var response = await _httpClient.GetAsync("https://api.kandji.io/v1/organizations");
                response.EnsureSuccessStatusCode();

                var organizations = JsonConvert.DeserializeObject<List<string>>(await response.Content.ReadAsStringAsync());
                return organizations;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error: {ex.Message}");
                return new List<string>();
            }
        }

        public static async Task<List<DataModels.Device>> GetDevices(string companyId)
        {
            try
            {
                if (_httpClient == null)
                {
                    throw new InvalidOperationException("KandjiApiClient has not been initialized. Call Initialize method first.");
                }

                var response = await _httpClient.GetAsync("https://simplified.api.eu.kandji.io/api/v1/devices");
                response.EnsureSuccessStatusCode();

                var devices = JsonConvert.DeserializeObject<List<KandjiDevice>>(await response.Content.ReadAsStringAsync());
                var resultDevices = new List<DataModels.Device>();

                foreach (var device in devices)
                {
                    var resultDevice = new DataModels.Device
                    {
                        Id = device.device_id,
                        CompanyId = companyId,
                        SerialNumber = device.serial_number,
                        DisplayName = device.device_name,
                        EnrolledDateTime = device.first_enrollment,
                        OperatingSystem = $"{device.platform} {device.os_version}"
                    };

                    resultDevices.Add(resultDevice);
                }
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
