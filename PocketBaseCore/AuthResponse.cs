using System.Text.Json.Serialization;

namespace PocketBaseCore
{
    public class AuthResponse 
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        
        [JsonPropertyName("record")]
        public PocketUser User { get; set; }
    }
}