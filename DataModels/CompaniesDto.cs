using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DeviceInfoHub.DataModels

{
    public class CompaniesDto
    {
        public string Id { get; set; } 

        public string? Name { get; set; }
        
        public bool? ClientId { get; set; }

        public bool? TenantId { get; set; }

        public bool? ClientSecret { get; set; }

        public bool? KandjiApiKey { get; set; }
    }
}