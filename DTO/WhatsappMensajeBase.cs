using System.Text.Json.Serialization;

namespace ApiWhatsapp.DTO
{
    public class WhatsappMensajeBase
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonPropertyName("to")]
        public string TelefonoDestino { get; set; }

        [JsonPropertyName("type")]
        public string Tipo { get; set; }
    }
}
