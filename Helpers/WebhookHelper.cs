using System.Net.Http.Headers;
using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;
using AutoMapper;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entities;
using ApiWhatsapp.Repositories;

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
        private readonly IConfiguration _configuration;
        private readonly string ruta;
        private readonly BotonesHelper botonesHelper;
        private readonly IMapper mapper;
        private readonly LocalizacionRepository localizacionRepository;

        public WebhookHelper(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper, IConfiguration configuration)
        {
            this.context = context;
            this.mapper = mapper;
            telefonoRepository = new TelefonoRepository(context, contextTerceros, mapper);
            mensajeRepository = new MensajeRepository(context);
            ficheroRepository = new FicheroRepository(context, mapper);
            _httpClient = new HttpClient();
            _configuration = configuration;
            ruta = _configuration["RutaFicherosLocal"]!;
            botonesHelper = new BotonesHelper(context, contextTerceros, mapper, configuration);
            localizacionRepository = new LocalizacionRepository(context);
        }

        /// <summary>
        /// Guarda un mensaje recibido por el webhook, junto con la información del teléfono si es necesario.
        /// </summary>
        /// <param name="mensaje">Mensaje recibido desde el webhook.</param>
        /// <param name="numero">Nombre o identificador asociado al número.</param>
        public async Task GuardarMensaje(MessageWebhook mensaje, string numero)
        {
            await ObtenerTelefono(mensaje, numero);
            await GetMensajePorTipo(mensaje);
        }

        /// <summary>
        /// Identifica el tipo de mensaje recibido y ejecuta la lógica correspondiente.
        /// </summary>
        /// <param name="mensaje">Mensaje recibido del webhook.</param>
        private async Task GetMensajePorTipo(MessageWebhook mensaje)
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

                case "interactive":
                    await GuardarMensajeBoton(mensaje);
                    await botonesHelper.ResponderMensaje(mensaje);
                    break;

                case "button":
                    //await
                    break;

                case "location":
                    await GuardarLocalizacion(mensaje);
                    break;

                default:
                    Console.WriteLine("Tipo de mensaje no soportado.");
                    break;
            }
        }

        /// <summary>
        /// Guarda un mensaje de archivo (imagen o documento) en el sistema.
        /// </summary>
        /// <param name="mensaje">Mensaje con archivo multimedia.</param>
        private async Task GuardarMensajeArchivo(MessageWebhook mensaje)
        {
            string rutaArchivo = "";

            if (mensaje.image is not null)
                rutaArchivo = await DescargarArchivoDesdeMetaAsync(mensaje.image.id, mensaje.image.id);
            else if (mensaje.document is not null)
                rutaArchivo = await DescargarArchivoDesdeMetaAsync(mensaje.document.id, mensaje.document.filename);

            try
            {
                var ficheroDTO = new FicheroDTO { Ruta = rutaArchivo };
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
        /// Guarda un mensaje de tipo botón interactivo en la base de datos.
        /// </summary>
        /// <param name="mensaje">Mensaje interactivo recibido.</param>
        private async Task GuardarMensajeBoton(MessageWebhook mensaje)
        {
            try
            {
                var mensajeArchivo = mensajeRepository.ConstruirMensajeBotonGuardado(
                    long.Parse(mensaje.from), 34644288224, int.Parse(mensaje.interactive.button_reply.id));

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
        /// <param name="mensaje">Mensaje de texto recibido.</param>
        private async Task GuardarMensajeTexto(MessageWebhook mensaje)
        {
            var mensajeTexto = mensajeRepository.ConstruirMensajeTexto(
                long.Parse(mensaje.from), 34644288224, mensaje.text.body);

            await mensajeRepository.AddMensaje(mensajeTexto);
        }

        /// <summary>
        /// Obtiene un teléfono de la base de datos o lo crea si no existe.
        /// </summary>
        /// <param name="webhook">Mensaje recibido desde el webhook.</param>
        /// <param name="numero1">Nombre o alias para asociar al teléfono si es nuevo.</param>
        /// <returns>Objeto Telefono correspondiente.</returns>
        private async Task<Telefono> ObtenerTelefono(MessageWebhook webhook, string numero1)
        {
            Telefono telefono = await telefonoRepository.GetTelefonosById(long.Parse(webhook.from));

            if (telefono is null)
            {
                var numero = DetectarPrefijo(long.Parse(webhook.from));

                if (!numero.HasValue)
                    throw new Exception("El número no es válido.");

                telefono = telefonoRepository.ConstruirTelefono(
                    numero.Value.numeroSinPrefijo, numero.Value.prefijo, numero1);

                await telefonoRepository.AddTelefono(telefono);
            }

            return telefono;
        }

        /// <summary>
        /// Detecta el prefijo y el número local a partir del número completo.
        /// </summary>
        /// <param name="numeroCompleto">Número telefónico completo.</param>
        /// <returns>Tupla con prefijo y número sin prefijo, o null si no se detecta.</returns>
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
        /// Guarda la localización enviada en el mensaje si no existe una localización del mismo día.
        /// </summary>
        /// <param name="mensaje">Mensaje con la localización.</param>
        private async Task GuardarLocalizacion(MessageWebhook mensaje)
        {
            Console.WriteLine($"Longitud: {mensaje.location.longitude}, Latitud: {mensaje.location.latitude}");
            try
            {
                Localizacion? localizacion = await localizacionRepository.GetLocalizacionByDia(
                    DateOnly.FromDateTime(DateTime.UtcNow), long.Parse(mensaje.from));

                if (localizacion is not null)
                    return;

                int result = await localizacionRepository.AddLocalizacion(new Localizacion
                {
                    Longitud = mensaje.location.longitude,
                    Latitud = mensaje.location.latitude,
                    Dia = DateOnly.FromDateTime(DateTime.UtcNow),
                    IdTelefono = long.Parse(mensaje.from)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Descarga un archivo multimedia desde los servidores de Meta (Facebook).
        /// </summary>
        /// <param name="mediaId">ID del media en la API de Meta.</param>
        /// <param name="nombreArchivo">Nombre base para guardar el archivo localmente.</param>
        /// <returns>Ruta local del archivo guardado.</returns>
        public async Task<string> DescargarArchivoDesdeMetaAsync(string mediaId, string nombreArchivo)
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
                    "image/webp" => ".webp",
                    "application/pdf" => ".pdf",
                    "application/msword" => ".doc",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                    "application/vnd.ms-excel" => ".xls",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                    "application/vnd.ms-powerpoint" => ".ppt",
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
                    "text/plain" => ".txt",
                    "audio/aac" => ".aac",
                    "audio/mpeg" => ".mp3",
                    "audio/mp4" => ".mp4",
                    "audio/amr" => ".amr",
                    "audio/ogg" => ".ogg",
                    "audio/opus" => ".opus",
                    "video/mp4" => ".mp4",
                    "video/3gpp" => ".3gp",
                    _ => ".bin"
                };

                Console.WriteLine(nombreArchivo);
                string fullPath = nombreArchivo + extension;
                Directory.CreateDirectory(Path.GetDirectoryName(ruta + fullPath)!);

                await File.WriteAllBytesAsync(ruta + fullPath, fileBytes);

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
