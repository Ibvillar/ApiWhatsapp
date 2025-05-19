using System.Net.Http.Headers;
using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;
using AutoMapper;
using ApiWhatsapp.DTO;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ApiWhatsapp.Helpers
{
    public class WebhookHelper
    {
        private TelefonoRepository telefonoRepository;
        private MensajeRepository mensajeRepository;
        private FicheroRepository ficheroRepository;
        private readonly HttpClient _httpClient;
        private readonly string TOKEN = "EAAHbxd02hJUBOZBOv4ZBzlQOtnojQLixKdobeqIz654prmYhyHXZBJCLXMBfyuBHt8ckCaBWILHENAmfRMDUhEoY3kHZBuaxsBmJMBAiarzNZADbLj6bVsrf288U3qdYtCXgiE5AZCfN0oFuXESDsOBmDYcB2aKE3zqnnsDYumU5T3XZAmVb8a1ZBqfUnNEmxgDp0liEh6zeo01Kei90";
        private readonly DbWhatsapp context;

        public WebhookHelper(DbWhatsapp context, IMapper mapper) 
        {
            this.context = context;
            telefonoRepository = new TelefonoRepository(context, mapper);
            mensajeRepository = new MensajeRepository(context);
            ficheroRepository = new FicheroRepository(context, mapper);
            _httpClient = new HttpClient();
        }

        public async Task GuardarMensaje(MessageWeebhook mensaje, string nombre)
        {
            await ObtenerTelefono(mensaje, nombre);
            await GetMensajePorTipo(mensaje);
        }

        private async Task GetMensajePorTipo(MessageWeebhook mensaje)
        {
            switch (mensaje.type)
            {
                case "text":
                    await GuardarMensajeTexto(mensaje);
                    break;

                case "image":
                    await GuardarMensajeArchivo(mensaje);
                    break;

                case "document":
                    await GuardarMensajeArchivo(mensaje);
                    break;

                default:
                    Console.WriteLine("Tipo de mensaje no soportado.");
                    break;
            }
        }

        private async Task GuardarMensajeArchivo(MessageWeebhook mensaje)
        {
            string ruta = "";
            if (mensaje.image is null)
            {
                ruta = await DescargarArchivoDesdeMetaAsync(mensaje.document.id);
            } else
            {
                ruta = await DescargarArchivoDesdeMetaAsync(mensaje.image.id);
            }

            try
            {
                FicheroDTO ficheroDTO = new FicheroDTO { Ruta = ruta };
                Fichero fichero = ficheroRepository.ConstuirFichero(ficheroDTO);
                await ficheroRepository.AddFichero(fichero);
                Mensaje mensajeArchivo = mensajeRepository.ConstruirMensajeArchivo(long.Parse(mensaje.from), 34644288224, fichero.Id);
                await mensajeRepository.AddMensaje(mensajeArchivo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        private async Task GuardarMensajeTexto(MessageWeebhook mensaje)
        {
            Mensaje mensajeTexto = mensajeRepository.ConstruirMensajeTexto(long.Parse(mensaje.from), 34644288224, mensaje.text.body);
            await mensajeRepository.AddMensaje(mensajeTexto);
        }

        private async Task<Telefono> ObtenerTelefono(MessageWeebhook webhook, string nombre)
        {
            Telefono telefono = telefonoRepository.GetTelefonosById(long.Parse(webhook.from));

            if (telefono is null)
            {
                var numero = DetectarPrefijo(long.Parse(webhook.from));

                if (!numero.HasValue)
                {
                    throw new Exception("El numero es nulo");
                }

                telefono = telefonoRepository.ConstruirTelefono(numero.Value.numeroSinPrefijo, numero.Value.prefijo, nombre);
                await telefonoRepository.AddTelefono(telefono);
            }

            return telefono;
        }

        public (short prefijo, int numeroSinPrefijo)? DetectarPrefijo(long numeroCompleto)
        {
            string numeroStr = numeroCompleto.ToString();

            var prefijos = context.Prefijos.ToList();

            foreach (var pref in prefijos.OrderByDescending(p => p.ToString()!.Length))
            {
                Console.WriteLine(pref.Prefijo);
                Console.WriteLine(numeroStr);
                if (numeroStr.StartsWith(pref.Prefijo))
                {
                    string sinPrefijoStr = numeroStr.Substring(pref.Prefijo!.Length);
                    if (int.TryParse(sinPrefijoStr, out int numeroSinPrefijo))
                        return (short.Parse(pref.Prefijo), numeroSinPrefijo);
                }
            }

            return null;
        }

        public async Task<string> DescargarArchivoDesdeMetaAsync(string mediaId)
        {
            try
            {
                // Paso 1: Obtener la URL de descarga del archivo
                Console.WriteLine(mediaId);
                Console.WriteLine(TOKEN);
                string metadataUrl = $"https://graph.facebook.com/v22.0/{mediaId}";
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, metadataUrl);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN);

                var metadataResponse = await _httpClient.SendAsync(requestMessage);
                metadataResponse.EnsureSuccessStatusCode();

                var metadataJson = await metadataResponse.Content.ReadAsStringAsync();
                var metadata = System.Text.Json.JsonSerializer.Deserialize<MetaMediaResponse>(metadataJson);

                // Paso 2: Descargar el archivo desde la URL
                var fileRequest = new HttpRequestMessage(HttpMethod.Get, metadata!.url);
                fileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN);

                var fileResponse = await _httpClient.SendAsync(fileRequest);
                fileResponse.EnsureSuccessStatusCode();

                var fileBytes = await fileResponse.Content.ReadAsByteArrayAsync();
                string mimeType = fileResponse.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                // Paso 3: Guardar el archivo en disco
                string extension = mimeType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "application/pdf" => ".pdf",
                    _ => ".bin"
                };

                string fileName = $"{Guid.NewGuid()}{extension}";
                string fullPath = Path.Combine(@"C:\Proyectos\ApiWhatsapp\FicherosWebHook", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!); // Asegura que la carpeta exista

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

        private class MetaMediaResponse
        {
            public string url { get; set; }
            public string mime_type { get; set; }
        }

    }
}
