using System.Text.Json.Serialization;

namespace ApiWhatsapp.DTO
{
    public class MensajeImagen: WhatsappMensajeBase
    {
        [JsonPropertyName("image")]
        public ContenidoImagen Imagen { get; set; }
    }

    public class ContenidoImagen
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("caption")]
        public string NombreConExtension { get; set; }
    }
}
