using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DeviceInfoHub.DataModels

{
    /// <summary>
    /// Represents the data model for a Company without secret values in the system. 
    /// </summary>
    public class CompaniesDto
    {
        public int Id { get; set; } 

        public string? Name { get; set; }
        
        public bool? ClientId { get; set; }

        public bool? TenantId { get; set; }

        public bool? ClientSecret { get; set; }

        public bool? KandjiApiKey { get; set; }
    }
}