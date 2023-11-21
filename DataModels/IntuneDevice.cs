using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DeviceInfoHub.DataModels

{
    public class IntuneDevice
    {
        public string Id { get; set; }

        public DateTime EnrolledDateTime { get; set; }

        public string OperatingSystem { get; set; }

        public string DisplayName { get; set; }

        public string Model { get; set; }

        public string Manufacturer { get; set; }

        public string SerialNumber { get; set; }

        public List<Application> Applications { get; set; }

        public List<Policy> Policies { get; set; }
    }


    public class Application
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Publisher { get; set; }
        public string Version { get; set; }
    }

    public class Policy
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        // Add policy-specific properties as needed
    }

    public class IntuneDeviceDbContext : DbContext
    {
        public DbSet<IntuneDevice> intuneDevice { get; set; }
    
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("MyDbConnection");

            // Configure the database connection string
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}

