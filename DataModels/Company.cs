using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DeviceInfoHub.DataModels

{
    /// <summary>
    /// Represents the data model for a Company in the system.
    /// </summary>
    public class Company
    {
        // Unique identifier for each company
        public int Id { get; set; }
        // Name of the company
        public string Name { get; set; }
        // Client ID used for authentication to the OAUTH (GraphAPI)
        public string? ClientId { get; set; }
        // Tenant ID used for authentication to the OAUTH (GraphAPI)
        public string? TenantId { get; set; }
        // Client Secret used for authentication to the OAUTH (GraphAPI)
        public string? ClientSecret { get; set; }
        // API key for Kandji authentication
        public string? KandjiApiKey { get; set; }
        // Timestamp of the last update made to this company's record
        public DateTime? LastUpdated { get; set; }
        // Flag indicating whether the company is archived (no longer active)
        public bool Archived { get; set; }
    }

    /// <summary>
    /// Database context class for Companies. It is used by Entity Framework to map, query, and save data related to Companies in the database.
    /// </summary>
    public class CompanyDbContext : DbContext
    {
        // DbSet representing the collection of Companies in the database
        public DbSet<Company> company { get; set; }
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

