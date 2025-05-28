using System.Text.Json.Serialization;

namespace ApiWhatsapp.DTO
{
    public class TokenValidationDTO
    {
        [JsonPropertyName("TOKEN")]
        public string token { get; set; }

        [JsonPropertyName("EXPIRATION")]
        public DateTime expiration { get; set; }
    }
}
