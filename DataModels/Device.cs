using System;
using System.Text;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Kiota.Abstractions;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace DeviceInfoHub.DataModels
{
    /// <summary>
    /// Represents the data model for a Device in the system.
    /// </summary>
    public class Device
    {
        // Unique identifier for each device
        public int Id { get; set; }
        // Unique identifier assigned to the device
        public string DeviceId { get; set; }
        // Name given to the device
        public string? DeviceName { get; set; }
        // Identifier for the user associated with the device
        public int UserId { get; set; }
        // Identifier for the company to which the device belongs
        public int CompanyId { get; set; }
        // Date and time when the device was first enrolled
        public DateTime? FirstEnrollment { get; set; }
        // Date and time when the device was last enrolled
        public DateTime? LastEnrollment { get; set; }
        // Model of the device
        public string? Model { get; set; }
         // Manufacturer of the device
        public string? Manufacturer { get; set; }
        // Serial number of the device
        public string? SerialNumber { get; set; }
        // Platform of the device 
        public string? Platform { get; set; }
        // Operating system version of the device
        public string? OsVersion { get; set; }
        // Date and time of the device's last sync DateTime
        public DateTime? LastSyncDateTime { get; set; }
        // Date and time when the database record was last updated
        public DateTime? DBLastUpdated { get; set; }
        // Description of the last updates made to the database record
        public string? DBLastUpdatedDesc { get; set; }
        // Source from which the device information was obtained
        public string? Source { get; set; }
        // Total storage space of the device in bytes
        public long? TotalStorageSpaceInBytes { get; set; }
        // Available storage space of the device in bytes
        public long? FreeStorageSpaceInBytes { get; set; }
        // Physical memory of the device in bytes
        public long? PhysicalMemoryInBytes { get; set; }
        // Flag indicating whether the device is archived (no longer active)
        public bool Archived { get; set; }
        // List of applications installed on the device
        public List<Application>? Applications { get; set; }
        // List of policies applied to the device
        public List<Policy>? Policies { get; set; }

        /// <summary>
        /// Determines if the current device data is updated compared to another device instance.
        /// </summary>
        /// <param name="other">The device instance to compare with.</param>
        /// <returns>True if the current device data is updated; otherwise, false.</returns>
        public bool isUpdated(Device other)
        {
            List<string> updatedThings = new List<string>();
            bool update = false;
            
            // Check which values are updated
            if (LastEnrollment > other.LastEnrollment)
            {
                updatedThings.Add($"LastEnrollment:{other.LastEnrollment}=>{LastEnrollment}");
                update = true;
            }
            if (DeviceName != other.DeviceName)
            {
                updatedThings.Add($"DisplayName:{other.DeviceName}=>{DeviceName}");
                update = true;
            }
            if (OsVersion != other.OsVersion)
            {
                updatedThings.Add($"OsVersion:{other.OsVersion}=>{OsVersion}");
                update = true;
            }
            if (SerialNumber != other.SerialNumber)
            {
                updatedThings.Add($"SerialNumber:{other.SerialNumber}=>{SerialNumber}");
                update = true;
            }
            if (Manufacturer != other.Manufacturer)
            {
                updatedThings.Add($"Manufacturer:{other.Manufacturer}=>{Manufacturer}");
                update = true;
            }

            // Checks if above data of device is updated 
            if (update)
            {
                // Concatenates the list of updates into a single string, separated by commas.
                DBLastUpdatedDesc = String.Join(", ", updatedThings);
                
                // Checks if the length of the concatenated updates string exceeds 250 characters.
                if (DBLastUpdatedDesc.Length >= 250)
                {
                    // If it does, truncate the string to the first 250 characters.
                    // This ensures the description fits within a potential database field size limit.
                    DBLastUpdatedDesc = DBLastUpdatedDesc.Substring(0,250);
                }
            }
            else
            {
                DBLastUpdatedDesc = other.DBLastUpdatedDesc;
            }

            return update;
        }
    }

    // Represents the data model for an Application in the system.
    public class Application
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Publisher { get; set; }
        public string Version { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Archived { get; set; }
    }

    // Represents the data model for a Policy in the system.
    public class Policy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Archived { get; set; }
    }

    /// <summary>
    /// Database context class for Devices. It is used by Entity Framework to map, query, and save data related to Devices in the database.
    /// </summary>
    public class DeviceDbContext : DbContext
    {
        // DbSet representing the collection of Devices in the database
        public DbSet<Device> device { get; set; }

        // Configuring the database context
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Fetching the connection string from environment variables
            var connectionString = Environment.GetEnvironmentVariable("MyDbConnection");

            // Configuring the database to use SQL Server with the provided connection string
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}

