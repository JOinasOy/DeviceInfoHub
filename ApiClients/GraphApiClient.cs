using Microsoft.Graph;
using Azure.Identity;
using DeviceInfoHub.DataModels;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Graph.Models;
using System.Threading.Tasks;

namespace DeviceInfoHub.ApiClients
{
    public class GraphApiClient
    {    
        private static GraphServiceClient _current;

        public static void Initialize(string tenantId, string clientId, string clientSecret)
        {
            var options = new TokenCredentialOptions
            {   
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            _current = new GraphServiceClient(clientSecretCredential, scopes);
        }

        public static async Task<List<string>> GetUsers()
        {
            try
            {
                if (_current == null)
                {
                    throw new InvalidOperationException("GraphApiClient has not been initialized. Call Initialize method first.");
                }

                var users = await _current.Users.GetAsync();
                var userDisplayNames = new List<string>();

                foreach (var user in users.Value)
                {
                    userDisplayNames.Add(user.UserPrincipalName);
                }

                return userDisplayNames;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error: {ex.Message}");
                return new List<string>();
            }
        }
        public static async Task<List<DataModels.Device>> GetUserDevices(string customerId, string username)
        {
            try
            {
                if (_current == null)
                {
                    throw new InvalidOperationException("GraphApiClient has not been initialized. Call Initialize method first.");
                }

                var user = await _current.Users[username].GetAsync();
                var dispName = user?.DisplayName;

                var devices = await _current.Devices.GetAsync();

                var intDevices = new List<DataModels.Device>();

                foreach (var device in devices.Value)
                {
                    var intDevice = new DataModels.Device
                    {
                        Id = device.Id,
                        CustomerId = customerId,
                        DisplayName = device.DisplayName,
                        EnrolledDateTime = device.RegistrationDateTime?.DateTime ?? DateTime.MinValue,
                        OperatingSystem = $"{device.OperatingSystem} {device.OperatingSystemVersion}"
                    };

                    intDevices.Add(intDevice);
                }

                return intDevices;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error: {ex.Message}");
                return new List<DataModels.Device>();
            }
        }
    }
}
