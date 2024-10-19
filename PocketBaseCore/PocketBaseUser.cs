using System.Text.Json.Serialization;

namespace PocketBaseCore
{
    public class PocketBaseUser : PocketBaseRecord
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("verified")]
        public bool Verified { get; set; }
        [JsonPropertyName("emailVisibility")]
        public bool EmailVisibility { get; set; }
    }
}