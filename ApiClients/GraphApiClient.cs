using Microsoft.Graph;
using Azure.Identity;
using DeviceInfoHub.DataModels;
using DeviceInfoHub.Helpers;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Graph.Models;
using System.Threading.Tasks;

namespace DeviceInfoHub.ApiClients
{
    /// <summary>
    /// Provides methods to interact with Microsoft Graph API for accessing and managing devices and users.
    /// </summary>
    public class GraphApiClient
    {    
        private static GraphServiceClient _current;

        /// <summary>
        /// Initializes the GraphServiceClient with the provided Azure AD credentials.
        /// </summary>
        /// <param name="tenantId">The Azure AD tenant ID.</param>
        /// <param name="clientId">The client ID of the Azure AD application.</param>
        /// <param name="clientSecret">The client secret of the Azure AD application.</param>
        public static void Initialize(string tenantId, string clientId, string clientSecret)
        {
            // Retrieve the encryption key for the database
            string? DBCryptKey = Environment.GetEnvironmentVariable("DBCryptKey");
            
            // Check if the DBCryptKey is available, otherwise log an error
            if (string.IsNullOrEmpty(DBCryptKey)) {
                Console.WriteLine("Error: Check DBCryptKey!");
                return;
            }

            // Decrypt the provided credentials using the encryption key
            clientId = EncryptionHelper.DecryptString(DBCryptKey, clientId);
            clientSecret = EncryptionHelper.DecryptString(DBCryptKey, clientSecret);
            tenantId = EncryptionHelper.DecryptString(DBCryptKey, tenantId);
            // Set DBCryptKey to null value for security reasons
            DBCryptKey = null;

            // Configuration for Azure AD authentication
            var options = new TokenCredentialOptions
            {   
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // Create a new ClientSecretCredential instance for Azure AD authentication
            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            
            // Initialize the GraphServiceClient with the credentials
            _current = new GraphServiceClient(clientSecretCredential, scopes);
        }
        
        /// <summary>
        /// Retrieves a list of devices from Microsoft Graph for a specific company.
        /// </summary>
        /// <param name="companyId">The ID of the company to retrieve devices for.</param>
        /// <returns>A list of devices.</returns>
        public static async Task<List<DataModels.Device>> GetDevices(int companyId)
        {
            try
            {
                // Ensure the GraphApiClient has been initialized
                if (_current == null)
                {
                    throw new InvalidOperationException("GraphApiClient has not been initialized. Call Initialize method first.");
                }

                // Fetch devices from Microsoft Graph
                var devices = await _current.DeviceManagement.ManagedDevices.GetAsync();
                var users = await _current.Users.GetAsync();
                var intDevices = new List<DataModels.Device>();

                // Process each device and map it to the internal data model
                foreach (var device in devices.Value)
                {
                    var intDevice = new DataModels.Device
                    {
                        DeviceId = device.Id,
                        CompanyId = companyId,
                        SerialNumber = device.SerialNumber,
                        DeviceName = device.DeviceName,
                        FirstEnrollment = device.EnrolledDateTime?.DateTime ?? DateTime.MinValue,
                        Platform = device.OperatingSystem,
                        OsVersion = device.OsVersion,
                        Manufacturer = device.Manufacturer,
                        Model = device.Model,
                        LastSyncDateTime = device.LastSyncDateTime?.DateTime ?? DateTime.MinValue,
                        TotalStorageSpaceInBytes = device.TotalStorageSpaceInBytes,
                        FreeStorageSpaceInBytes = device.FreeStorageSpaceInBytes,
                        PhysicalMemoryInBytes = device.PhysicalMemoryInBytes,
                        Source = "Intune",
                        DBLastUpdated = DateTime.Now
                    };
                    
                    using (var context = new UsersDbContext())
                    {
                        string currentUserID = "UNKNOWN";
                        
                        if (!string.IsNullOrEmpty(device.UserId))
                        {
                            currentUserID = device.UserId;
                        }

                        // Check if device user already exists in the database
                        var user = context.users.Where(u => u.UserId == currentUserID && u.CompanyId == companyId);
                       
                        // User exists in the database. Set the Device UserId 
                        if (user.Count() > 0)
                        {
                            // Find correct user from Graph Users and save department info
                            foreach (var existsUser in users.Value)
                            {
                                if (existsUser.Id == user.First().UserId)
                                {
                                    if (string.IsNullOrEmpty(existsUser.Department))
                                    {
                                        user.First().Department = "undefined";
                                    }
                                    else
                                    {
                                        user.First().Department = existsUser.Department;
                                    }
                                    context.users.Update(user.First());
                                }
                            }

                            intDevice.UserId = user.First().Id;

                            context.SaveChanges();
                        }
                        else // User doesn't exists in the database. Create a new user to the database
                        {
                            var newUser = new Users();

                            // If user details exists in device data 
                            if (!string.IsNullOrEmpty(device.UserId))
                            {
                                newUser = new Users
                                {
                                    UserId = device.UserId,
                                    CompanyId = companyId,
                                    DisplayName = device.UserDisplayName,
                                    UserPrincipalName = device.UserPrincipalName,
                                    Email = device.EmailAddress,
                                    GivenName = "",
                                    Department = "",
                                    LastUpdated = DateTime.Now
                                };
                            }
                            else // User details doesn't found. Add unknown user
                            {
                                newUser = new Users
                                {
                                    UserId = currentUserID,
                                    CompanyId = companyId,
                                    DisplayName = "UNKNOWN",
                                    LastUpdated = DateTime.Now,
                                    Archived = false
                                };
                            }
                            context.users.Add(newUser);
                            // Save changes to the database
                            context.SaveChanges();
                            
                            // Gets just created user id value from the database and updates it to device userid value
                            user = context.users.Where(u => u.UserId == device.UserId && u.CompanyId == companyId);
                            intDevice.UserId = user.First().Id;
                        }
                    }
                    
                    intDevices.Add(intDevice);
                }
                // return list of devices
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
