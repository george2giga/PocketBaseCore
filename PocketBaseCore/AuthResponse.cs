using System.Text.Json.Serialization;

namespace PocketBaseCore
{
    public class AuthResponse<T> where T : PocketBaseUser
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        
        [JsonPropertyName("record")]
        public T User { get; set; }
    }
}