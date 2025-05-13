using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ApiWhatsapp.BBDD;
using ApiWhatsapp.DTO;
using Microsoft.AspNetCore.StaticFiles;

namespace ApiWhatsapp.EnvioMensajes
{
    public class MensajeHelper
    {
        private HttpClient _httpClient;
        private readonly string _token;
        private readonly string url;

        public MensajeHelper(string _token, string url) 
        {
            _httpClient = new HttpClient();
            this._token = _token;
            this.url = url;
        }

        public MensajeTexto ConstruirMensajeTexto(long numeroDestino, string texto)
        {
            var mensajeTexto = new MensajeTexto
            {
                MessagingProduct = "whatsapp",
                TelefonoDestino = numeroDestino.ToString(),
                Tipo = "text",
                Texto = new TextoMensaje
                {
                    Body = texto
                }
            };

            return mensajeTexto;
        }

        public async Task<MensajeImagen> ConstruirMensajeImagen(long numeroDestino, string ruta)
        {
            var mediaId = await GetIdFromFichero(ruta);
            Console.WriteLine(mediaId);

            var mensajeImagen = new MensajeImagen
            {
                MessagingProduct = "whatsapp",
                TelefonoDestino = numeroDestino.ToString(),
                Tipo = "image",
                Imagen = new ContenidoImagen
                {
                    Id = mediaId
                }
            };

            return mensajeImagen;
        }

        public async Task<MensajeDocumento> ConstruirMensajeDocumento(long numeroDestino, string nombre, string ruta)
        {
            var mediaId = await GetIdFromFichero(ruta);
            Console.WriteLine($"ID del archivo subido: {mediaId}");

            var mensajeDocumento = new MensajeDocumento
            {
                MessagingProduct = "whatsapp",
                TelefonoDestino = numeroDestino.ToString(),
                Tipo = "document",
                Documento = new ContenidoDocumento
                {
                    Id = mediaId,
                    Nombre = nombre
                }
            };

            return mensajeDocumento;
        }

        private async Task<string> GetIdFromFichero(string ruta)
        {
            var fileInfo = new FileInfo(ruta);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("El archivo no existe", ruta);

            if (fileInfo.Length == 0)
                throw new InvalidOperationException("El archivo está vacío.");

            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(ruta);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(ruta));

            form.Add(fileContent, "file", Path.GetFileName(ruta));
            form.Add(new StringContent("whatsapp"), "messaging_product");

            var request = new HttpRequestMessage(HttpMethod.Post, url + "media");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Content = form;

            var response = await _httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al subir el archivo: {response.StatusCode} - {content}");

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);
            if (jsonResponse.TryGetProperty("id", out JsonElement idProperty))
            {
                return idProperty.GetString();
            }

            throw new Exception("No se pudo obtener el ID del archivo subido.");
        }

        private string GetMimeType(string ruta)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(ruta, out string mimeType))
            {
                mimeType = "application/octet-stream";
            }

            return mimeType;
        }
    }
}
