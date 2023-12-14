using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DeviceInfoHub.DataModels

{
    public class Company
    {
        public string Id { get; set; } 

        public string? Name { get; set; }
        
        public string? ClientId { get; set; }

        public string? TenantId { get; set; }

        public string? ClientSecret { get; set; }

        public string? KandjiApiKey { get; set; }
    }

    public class CompanyDbContext : DbContext
    {
        public DbSet<Company> company { get; set; }
    
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("MyDbConnection");

            // Configure the database connection string
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}

