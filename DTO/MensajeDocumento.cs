using System.Text.Json.Serialization;

namespace ApiWhatsapp.DTO
{
    public class MensajeDocumento: WhatsappMensajeBase
    {
        [JsonPropertyName("document")]
        public ContenidoDocumento Documento { get; set; }
    }

    public class ContenidoDocumento
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("filename")]
        public string Nombre { get; set; }
    }
}
