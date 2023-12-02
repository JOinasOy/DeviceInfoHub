using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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

        public static async Task<List<KandjiDevice>> GetDevices()
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
                return devices;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error: {ex.Message}");
                return new List<KandjiDevice>();
            }
        }
    }

    public class KandjiDevice
    {
        // Define properties based on the Kandji API device structure
        public string device_id { get; set; }
        public string device_name { get; set; }
        // Add other relevant properties
    }
}
