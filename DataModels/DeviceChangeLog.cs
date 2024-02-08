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
    /// Represents the data model for a DeviceChangeLog in the system.
    /// </summary>
    public class DeviceChangeLog
    {
        // Unique identifier for each device
        public int Id { get; set; }
        // Unique identifier assigned to the device
        public int DeviceId { get; set; }
        // Device object
        public Device? device { get; set; }
        // Date and time when the database record was last updated
        public DateTime? UpdateTime { get; set; }
        // Description of the last updates made to the database record
        public string? UpdateTxt { get; set; }
    }

    /// <summary>
    /// Database context class for DeviceChangeLogs. It is used by Entity Framework to map, query, and save data related to DeviceChangeLogs in the database.
    /// </summary>
    public class DeviceChangeLogDbContext : DbContext
    {
        // DbSet representing the collection of DeviceChangeLogs in the database
        public DbSet<DeviceChangeLog> deviceChangeLog { get; set; }

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

