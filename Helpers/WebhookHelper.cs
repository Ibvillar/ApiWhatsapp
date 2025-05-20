using System.Net.Http.Headers;
using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;
using AutoMapper;
using ApiWhatsapp.DTO;

namespace ApiWhatsapp.Helpers
{
    /// <summary>
    /// Clase auxiliar para manejar la lógica del webhook de WhatsApp.
    /// Procesa y almacena mensajes entrantes.
    /// </summary>
    public class WebhookHelper
    {
        private TelefonoRepository telefonoRepository;
        private MensajeRepository mensajeRepository;
        private FicheroRepository ficheroRepository;
        private readonly HttpClient _httpClient;
        private readonly string TOKEN = "EAAHbxd02hJUBOZBOv4ZBzlQOtnojQLixKdobeqIz654prmYhyHXZBJCLXMBfyuBHt8ckCaBWILHENAmfRMDUhEoY3kHZBuaxsBmJMBAiarzNZADbLj6bVsrf288U3qdYtCXgiE5AZCfN0oFuXESDsOBmDYcB2aKE3zqnnsDYumU5T3XZAmVb8a1ZBqfUnNEmxgDp0liEh6zeo01Kei90";
        private readonly DbWhatsapp context;

        /// <summary>
        /// Constructor de la clase WebhookHelper.
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        /// <param name="mapper">Instancia de AutoMapper</param>
        public WebhookHelper(DbWhatsapp context, IMapper mapper)
        {
            this.context = context;
            telefonoRepository = new TelefonoRepository(context, mapper);
            mensajeRepository = new MensajeRepository(context);
            ficheroRepository = new FicheroRepository(context, mapper);
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Guarda un mensaje recibido por el webhook, junto con la información del teléfono si es necesario.
        /// </summary>
        /// <param name="mensaje">Mensaje recibido desde el webhook</param>
        /// <param name="nombre">Nombre asociado al número si es nuevo</param>
        public async Task GuardarMensaje(MessageWeebhook mensaje, string nombre)
        {
            await ObtenerTelefono(mensaje, nombre);
            await GetMensajePorTipo(mensaje);
        }

        /// <summary>
        /// Identifica el tipo de mensaje y ejecuta la lógica correspondiente.
        /// </summary>
        private async Task GetMensajePorTipo(MessageWeebhook mensaje)
        {
            switch (mensaje.type)
            {
                case "text":
                    await GuardarMensajeTexto(mensaje);
                    break;

                case "image":
                case "document":
                    await GuardarMensajeArchivo(mensaje);
                    break;

                default:
                    Console.WriteLine("Tipo de mensaje no soportado.");
                    break;
            }
        }

        /// <summary>
        /// Guarda un mensaje de archivo (imagen o documento) en el sistema.
        /// </summary>
        private async Task GuardarMensajeArchivo(MessageWeebhook mensaje)
        {
            string ruta = mensaje.image is null
                ? await DescargarArchivoDesdeMetaAsync(mensaje.document.id)
                : await DescargarArchivoDesdeMetaAsync(mensaje.image.id);

            try
            {
                var ficheroDTO = new FicheroDTO { Ruta = ruta };
                var fichero = ficheroRepository.ConstuirFichero(ficheroDTO);
                await ficheroRepository.AddFichero(fichero);

                var mensajeArchivo = mensajeRepository.ConstruirMensajeArchivo(
                    long.Parse(mensaje.from), 34644288224, fichero.Id);

                await mensajeRepository.AddMensaje(mensajeArchivo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Guarda un mensaje de texto en la base de datos.
        /// </summary>
        private async Task GuardarMensajeTexto(MessageWeebhook mensaje)
        {
            var mensajeTexto = mensajeRepository.ConstruirMensajeTexto(
                long.Parse(mensaje.from), 34644288224, mensaje.text.body);

            await mensajeRepository.AddMensaje(mensajeTexto);
        }

        /// <summary>
        /// Obtiene un teléfono de la base de datos o lo crea si no existe.
        /// </summary>
        /// <param name="webhook">Mensaje recibido</param>
        /// <param name="nombre">Nombre a asociar si es nuevo</param>
        /// <returns>Objeto Telefono</returns>
        private async Task<Telefono> ObtenerTelefono(MessageWeebhook webhook, string nombre)
        {
            var telefono = telefonoRepository.GetTelefonosById(long.Parse(webhook.from));

            if (telefono is null)
            {
                var numero = DetectarPrefijo(long.Parse(webhook.from));

                if (!numero.HasValue)
                {
                    throw new Exception("El número no es válido.");
                }

                telefono = telefonoRepository.ConstruirTelefono(
                    numero.Value.numeroSinPrefijo, numero.Value.prefijo, nombre);

                await telefonoRepository.AddTelefono(telefono);
            }

            return telefono;
        }

        /// <summary>
        /// Detecta el prefijo y el número local a partir del número completo.
        /// </summary>
        /// <param name="numeroCompleto">Número telefónico completo</param>
        /// <returns>Tupla con prefijo y número sin prefijo</returns>
        public (short prefijo, int numeroSinPrefijo)? DetectarPrefijo(long numeroCompleto)
        {
            string numeroStr = numeroCompleto.ToString();
            var prefijos = context.Prefijos.ToList();

            foreach (var pref in prefijos.OrderByDescending(p => p.Prefijo!.Length))
            {
                if (numeroStr.StartsWith(pref.Prefijo))
                {
                    string sinPrefijoStr = numeroStr[pref.Prefijo.Length..];
                    if (int.TryParse(sinPrefijoStr, out int numeroSinPrefijo))
                        return (short.Parse(pref.Prefijo), numeroSinPrefijo);
                }
            }

            return null;
        }

        /// <summary>
        /// Descarga un archivo multimedia desde los servidores de Meta (Facebook).
        /// </summary>
        /// <param name="mediaId">ID del media en la API de Meta</param>
        /// <returns>Ruta local del archivo guardado</returns>
        public async Task<string> DescargarArchivoDesdeMetaAsync(string mediaId)
        {
            try
            {
                string metadataUrl = $"https://graph.facebook.com/v22.0/{mediaId}";
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, metadataUrl);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN);

                var metadataResponse = await _httpClient.SendAsync(requestMessage);
                metadataResponse.EnsureSuccessStatusCode();

                var metadataJson = await metadataResponse.Content.ReadAsStringAsync();
                var metadata = System.Text.Json.JsonSerializer.Deserialize<MetaMediaResponse>(metadataJson);

                var fileRequest = new HttpRequestMessage(HttpMethod.Get, metadata!.url);
                fileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN);

                var fileResponse = await _httpClient.SendAsync(fileRequest);
                fileResponse.EnsureSuccessStatusCode();

                var fileBytes = await fileResponse.Content.ReadAsByteArrayAsync();
                string mimeType = fileResponse.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                string extension = mimeType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "application/pdf" => ".pdf",
                    _ => ".bin"
                };

                string fileName = $"{Guid.NewGuid()}{extension}";
                string fullPath = Path.Combine(@"C:\Proyectos\ApiWhatsapp\FicherosWebHook", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                await File.WriteAllBytesAsync(fullPath, fileBytes);

                Console.WriteLine($"Archivo guardado exitosamente en: {fullPath}");

                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error descargando archivo: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Clase interna para deserializar la respuesta de la API de Meta con los datos del archivo.
        /// </summary>
        private class MetaMediaResponse
        {
            public string url { get; set; }
            public string mime_type { get; set; }
        }
    }
}
