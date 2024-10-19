using System;
using System.Text.Json.Serialization;

namespace PocketBaseCore
{
    public abstract class PocketBaseRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }
        
        [JsonPropertyName("updated")]
        public DateTime Updated { get; set; }
    }
}