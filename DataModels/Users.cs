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
    /// Represents the data model for Users. This class is used to map users data in the database.
    /// </summary>
    public class Users
    {
        // Unique identifier for each user
        public int Id { get; set; }
        // Identifier for the user, potentially unique within a company
        public string? UserId { get; set; }
        // Identifier for the company to which the user belongs
        public int CompanyId { get; set; }
        // The display name of the user
        public string? DisplayName { get; set; }
        // Principal name used to identify the user in administrative contexts
        public string? UserPrincipalName { get; set; }
        // Given name of the user
        public string? GivenName { get; set; }
        // Email address of the user
        public string? Email { get; set; }
        // Department where the user works
        public string? Department { get; set; }
        // Timestamp of the last update made to this user's record
        public DateTime? LastUpdated { get; set; }
        // Flag indicating whether the user is archived (no longer active)
        public bool Archived { get; set; }
    }

    /// <summary>
    /// Database context class for Users. It is used by Entity Framework to map, query, and save data related to Users in the database.
    /// </summary>
    public class UsersDbContext : DbContext
    {
        // DbSet representing the collection of Users in the database
        public DbSet<Users> users { get; set; }
        
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