using System;
using System.Text;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Kiota.Abstractions;

namespace DeviceInfoHub.DataModels

{
    public class Device
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public int CompanyId { get; set; }
        public DateTime? EnrolledDateTime { get; set; }
        public string? OperatingSystem { get; set; }
        public string? DisplayName { get; set; }
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? LastUpdatedDesc { get; set; }
        public bool Archived { get; set; }
        public List<Application>? Applications { get; set; }
        public List<Policy>? Policies { get; set; }

        public bool isUpdated(Device other)
        {
            List<string> updatedThings = new List<string>();
            bool update = false;
            if (DisplayName != other.DisplayName)
            {
                updatedThings.Add($"DisplayName:{other.DisplayName}=>{DisplayName}");
                update = true;
            }
            if (OperatingSystem != other.OperatingSystem)
            {
                updatedThings.Add($"OperatingSystem:{other.OperatingSystem}=>{OperatingSystem}");
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

            LastUpdatedDesc = String.Join(", ", updatedThings);
            if (LastUpdatedDesc.Length >= 250)
            {
                LastUpdatedDesc = LastUpdatedDesc.Substring(0,250);
            }

            return update;
        }
    }


    public class Application
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Publisher { get; set; }
        public string Version { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Archived { get; set; }
    }

    public class Policy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Archived { get; set; }
    }

    public class DeviceDbContext : DbContext
    {
        public DbSet<Device> device { get; set; }
    
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("MyDbConnection");

            // Configure the database connection string
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}

