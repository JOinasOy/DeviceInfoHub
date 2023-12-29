using System;
using System.Text.Json;
using Newtonsoft.Json;

namespace DeviceInfoHub.DataModels
{
    /// <summary>
    /// The ResponseInfo class is designed to represent general response information.
    /// It includes a message and additional details that can be used for various response purposes.
    /// This class is especially useful for structuring and serializing response data in applications, 
    /// such as API responses or system notifications.
    /// The ToJson method enables easy conversion of the response details into a JSON-formatted string.
    /// </summary>
    public class ResponseInfo
    {
        // Property to hold the main response message
        public string Message { get; set; }

        // Property to hold additional details about the response
        public object Details { get; set; }

        // Constructor to initialize the properties
        public ResponseInfo(string message, object details = null)
        {
            Message = message;
            Details = details ?? new {}; // Use an empty object if details are not provided
        }

        // Method to convert the response information into a JSON string
        public string ToJson()
        {
            // Serialize this object to a JSON string
            return JsonConvert.SerializeObject(this);
        }
    }
}