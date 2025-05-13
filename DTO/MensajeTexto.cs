using System.Text.Json.Serialization;

namespace ApiWhatsapp.DTO
{
    public class MensajeTexto: WhatsappMensajeBase
    {
        [JsonPropertyName("text")]
        public TextoMensaje Texto { get; set; }
    }
    
    public class TextoMensaje
    {
        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
