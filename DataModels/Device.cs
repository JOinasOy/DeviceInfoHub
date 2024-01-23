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
        public string? Source { get; set; }
        // Total storage space of the device in bytes
        public long? TotalStorageSpaceInBytes { get; set; }
        // Available storage space of the device in bytes
        public long? FreeStorageSpaceInBytes { get; set; }
        // Physical memory of the device in bytes
        public long? PhysicalMemoryInBytes { get; set; }
        // Flag indicating whether the device is archived (no longer active)
        public bool Archived { get; set; }
        // The user associated with the device
        public Users User { get; set; }

        /// <summary>
        /// Determines if the current device data is updated compared to another device instance.
        /// </summary>
        /// <param name="other">The device instance to compare with.</param>
        /// <returns>True if the current device data is updated; otherwise, false.</returns>
        public string isUpdated(Device other)
        {
            string DBLastUpdatedDesc = null;
            List<string> updatedThings = new List<string>();
            bool update = false;
            
            // Check which values are updated
            if (LastEnrollment > other.LastEnrollment)
            {
                updatedThings.Add($"LastEnrollment:{other.LastEnrollment}=>{LastEnrollment}");
                update = true;
            }
            if (UserId != other.UserId)
            {
                updatedThings.Add($"UserId:{other.UserId}=>{UserId}");
                update = true;
            }
            if (DeviceName != other.DeviceName)
            {
                updatedThings.Add($"DeviceName:{other.DeviceName}=>{DeviceName}");
                update = true;
            }
            if (OsVersion != other.OsVersion)
            {
                updatedThings.Add($"OsVersion:{other.OsVersion}=>{OsVersion}");
                update = true;
            }

            // Checks if above data of device is updated 
            if (update)
            {
                // Concatenates the list of updates into a single string, separated by commas.
                DBLastUpdatedDesc = String.Join(", ", updatedThings);
            }

            return DBLastUpdatedDesc;
        }
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

