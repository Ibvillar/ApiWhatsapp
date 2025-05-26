using System.Net.Http.Headers;
using System.Text.Json;
using ApiWhatsapp.DTO;
using Microsoft.AspNetCore.StaticFiles;

namespace ApiWhatsapp.EnvioMensajes
{
    /// <summary>
    /// Clase auxiliar para construir y enviar mensajes de WhatsApp.
    /// </summary>
    public class MensajeHelper
    {
        private HttpClient _httpClient;
        private readonly string _token;
        private readonly string url;
        private readonly string ruta;

        /// <summary>
        /// Constructor de la clase MensajeHelper.
        /// </summary>
        /// <param name="_token">Token de autenticación de la API de WhatsApp</param>
        /// <param name="url">URL base del servicio de WhatsApp</param>
        public MensajeHelper(string _token, string url, IConfiguration _configuracion)
        {
            _httpClient = new HttpClient();
            this._token = _token;
            this.url = url;
            ruta = _configuracion["RutaFicheros"]!;
        }

        /// <summary>
        /// Construye un mensaje de texto para WhatsApp.
        /// </summary>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="texto">Contenido del mensaje</param>
        /// <returns>Objeto MensajeTexto</returns>
        public MensajeTexto ConstruirMensajeTexto(long numeroDestino, string texto)
        {
            return new MensajeTexto
            {
                MessagingProduct = "whatsapp",
                TelefonoDestino = numeroDestino.ToString(),
                Tipo = "text",
                Texto = new TextoMensaje
                {
                    Body = texto
                }
            };
        }

        /// <summary>
        /// Construye un mensaje de imagen para WhatsApp.
        /// </summary>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="ruta">Ruta del archivo de imagen</param>
        /// <returns>Objeto MensajeImagen</returns>
        public async Task<MensajeImagen> ConstruirMensajeImagen(long numeroDestino, string ruta)
        {
            var mediaId = await GetIdFromFichero(ruta);
            Console.WriteLine(mediaId);

            return new MensajeImagen
            {
                MessagingProduct = "whatsapp",
                TelefonoDestino = numeroDestino.ToString(),
                Tipo = "image",
                Imagen = new ContenidoImagen
                {
                    Id = mediaId
                }
            };
        }

        /// <summary>
        /// Construye un mensaje de documento para WhatsApp.
        /// </summary>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="nombre">Nombre del archivo</param>
        /// <param name="ruta">Ruta del archivo</param>
        /// <returns>Objeto MensajeDocumento</returns>
        public async Task<MensajeDocumento> ConstruirMensajeDocumento(long numeroDestino, string nombre, string ruta)
        {
            var mediaId = await GetIdFromFichero(ruta);
            Console.WriteLine($"ID del archivo subido: {mediaId}");

            return new MensajeDocumento
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
        }

        public async Task<string> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), ruta);

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }



        /// <summary>
        /// Sube un archivo al servidor de WhatsApp y obtiene el ID del media.
        /// </summary>
        /// <param name="ruta">Ruta local del archivo</param>
        /// <returns>ID del archivo subido</returns>
        /// <exception cref="FileNotFoundException">Si el archivo no existe</exception>
        /// <exception cref="InvalidOperationException">Si el archivo está vacío</exception>
        /// <exception cref="Exception">Si ocurre un error en la subida</exception>
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
                return idProperty.GetString()!;
            }

            throw new Exception("No se pudo obtener el ID del archivo subido.");
        }

        /// <summary>
        /// Obtiene el tipo MIME basado en la extensión del archivo.
        /// </summary>
        /// <param name="ruta">Ruta del archivo</param>
        /// <returns>Cadena con el tipo MIME</returns>
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
