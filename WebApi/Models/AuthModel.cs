using System.Text.Json.Serialization;

namespace ComicBackend.WebApi.Models
{
    public class RegisterRequest
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}