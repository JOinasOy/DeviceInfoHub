using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DeviceInfoHub.DataModels

{
    public class Customer
    {
        public string Id { get; set; } 

        public string? Name { get; set; }
        
        public string? ClientId { get; set; }

        public string? TenantId { get; set; }

        public string? ClientSecret { get; set; }

        public string? KandjiApiKey { get; set; }
    }

    public class CustomerDbContext : DbContext
    {
        public DbSet<Customer> customer { get; set; }
    
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("MyDbConnection");

            // Configure the database connection string
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}

