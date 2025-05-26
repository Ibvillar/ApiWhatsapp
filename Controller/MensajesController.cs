using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using ApiWhatsapp.EnvioMensajes;
using System.Text;
using System.Text.Json;
using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;
using AutoMapper;
using ApiWhatsapp.DTO;
using Microsoft.IdentityModel.Tokens;
using ApiWhatsapp.Repositories;
using ApiWhatsapp.Entities;

namespace ApiWhatsapp.Controller
{
    /// <summary>
    /// Controlador para manejar el envío y la gestión de mensajes vía WhatsApp.
    /// </summary>
    [ApiController]
    [Route("mensajes")]
    public class MensajesController : ControllerBase
    {
        private static readonly string TOKEN = "EAAHbxd02hJUBOZBOv4ZBzlQOtnojQLixKdobeqIz654prmYhyHXZBJCLXMBfyuBHt8ckCaBWILHENAmfRMDUhEoY3kHZBuaxsBmJMBAiarzNZADbLj6bVsrf288U3qdYtCXgiE5AZCfN0oFuXESDsOBmDYcB2aKE3zqnnsDYumU5T3XZAmVb8a1ZBqfUnNEmxgDp0liEh6zeo01Kei90";
        private readonly HttpClient _httpClient;
        private MensajeHelper _mensajesHelper;
        private FicheroRepository ficheroRepository;
        private MensajeRepository mensajeRepository;
        private TelefonoRepository telefonoRepository;
        private BotonRepository botonRepository;
        private readonly long NumeroEmpresa;

        /// <summary>
        /// Constructor del controlador de mensajes.
        /// </summary>
        public MensajesController(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper, IConfiguration _configuracion)
        {
            _httpClient = new HttpClient();
            _mensajesHelper = new MensajeHelper(TOKEN, getUrl(""), _configuracion);
            ficheroRepository = new FicheroRepository(context, mapper);
            mensajeRepository = new MensajeRepository(context);
            telefonoRepository = new TelefonoRepository(context, contextTerceros, mapper);
            botonRepository = new BotonRepository(context);
            NumeroEmpresa = long.Parse(_configuracion["NumeroEmpresa"]!);
        }

        /// <summary>
        /// Envía un mensaje de texto a un número específico.
        /// </summary>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="texto">Texto del mensaje</param>
        [HttpPost("enviar-texto")]
        public async Task<ActionResult> EnviarMensajeTexto(long numeroDestino, string texto)
        {
            try
            {
                await GuardarMensaje(NumeroEmpresa, numeroDestino, texto, -1);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            var mensaje = _mensajesHelper.ConstruirMensajeTexto(numeroDestino, texto);
            var json = CastToJson(mensaje);

            var respuesta = await EnviarMensaje(json);

            return respuesta ? Ok() : BadRequest("Algo salió mal al enviar el mensaje");
        }

        /// <summary>
        /// Envía una imagen a un número específico.
        /// </summary>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="ruta">Ruta del fichero de imagen</param>
        [HttpPost("enviar-imagen/{numeroDestino}")]
        [HttpPost("enviar-imagen")]
        public async Task<ActionResult> EnviarMensajeImagen(long numeroDestino, IFormFile file)
        {
            try
            {
                // Subir archivo y obtener la ruta donde se guardó
                string rutaArchivo = await _mensajesHelper.UploadFile(file);
                if (string.IsNullOrEmpty(rutaArchivo))
                    return BadRequest("No se pudo guardar el archivo");

                // Guardar referencia del archivo y el mensaje
                int idFichero = await GuardarFichero(rutaArchivo.Substring(rutaArchivo.LastIndexOfAny("/".ToCharArray()), rutaArchivo.Last()));
                await GuardarMensaje(NumeroEmpresa, numeroDestino, "", idFichero);

                // Construir y enviar el mensaje con imagen
                var mensaje = await _mensajesHelper.ConstruirMensajeImagen(numeroDestino, rutaArchivo);
                var json = CastToJson(mensaje);

                var respuesta = await EnviarMensaje(json);
                return respuesta ? Ok() : BadRequest("Algo salió mal al enviar el mensaje");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return BadRequest(e.Message);
            }
        }


        /// <summary>
        /// Envía un documento a un número específico.
        /// </summary>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="nombre">Nombre del documento</param>
        /// <param name="ruta">Ruta del documento</param>
        [HttpPost("enviar-documento")]
        public async Task<ActionResult> EnviarMensajeDocumento(long numeroDestino, string nombre, string ruta)
        {
            try
            {
                int idFichero = await GuardarFichero(ruta);
                await GuardarMensaje(NumeroEmpresa, numeroDestino, "", idFichero);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            var mensaje = await _mensajesHelper.ConstruirMensajeDocumento(numeroDestino, nombre, ruta);
            var json = CastToJson(mensaje);

            var respuesta = await EnviarMensaje(json);
            return respuesta ? Ok() : BadRequest("Algo salió mal al enviar el mensaje");
        }

        /// <summary>
        /// Envía un mensaje con botones a un número de teléfono.
        /// </summary>
        /// <param name="cuerpo">Texto del mensaje.</param>
        /// <param name="numero">Número de destino.</param>
        /// <param name="idBoton">IDs de los botones a incluir (máx. 3).</param>
        /// <returns>OK si se envió correctamente, BadRequest si hubo error.</returns>
        [HttpPost("enviar-boton")]
        public async Task<ActionResult> EnviarMensajeBoton(string cuerpo, string numero, params int[] idBoton)
        {
            if (idBoton == null || idBoton.Length == 0)
                return BadRequest("Se debe proporcionar al menos un ID de botón.");

            List<DTO.ButtonReply> botones = new List<DTO.ButtonReply>();

            foreach (var id in idBoton.Take(3))
            {
                Boton boton = await botonRepository.GetBotonById(id);
                if (boton == null)
                    continue;

                botones.Add(new DTO.ButtonReply
                {
                    type = "reply",
                    reply = new Reply
                    {
                        id = id.ToString(),
                        title = boton.Texto
                    }
                });
            }

            if (botones.Count == 0)
                return BadRequest("No se pudieron construir botones válidos.");

            var mensaje = mensajeRepository.ContruirMensajeBoton(
                numero,
                cuerpo,
                botones.ToArray()
            );

            var json = CastToJson(mensaje);

            var respuesta = await EnviarMensaje(json);
            return respuesta ? Ok() : BadRequest("Algo salió mal al enviar el mensaje");
        }

        /// <summary>
        /// Cambia el estado de un mensaje a leído.
        /// </summary>
        /// <param name="mensajeId">ID del mensaje</param>
        [HttpPut("cambiar-a-leido/{mensajeId}")]
        public async Task<ActionResult> CambiarALeido(int mensajeId)
        {
            try
            {
                int result = await mensajeRepository.SetLeido(mensajeId);

                return result == 0 ? NotFound("Este mensaje no existe") : Ok("Se ha cambiado correctamente");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtiene todos los mensajes registrados.
        /// </summary>
        [HttpGet("obtener-mensajes")]
        public async Task<ActionResult> GetAllMensajes()
        {
            try
            {
                var mensajes = await mensajeRepository.GetMensajes();
                return mensajes.IsNullOrEmpty() ? NotFound("No hay mensajes disponibles") : Ok(mensajes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtiene los mensajes enviados por un número específico.
        /// </summary>
        /// <param name="telefonoId">ID del número de origen</param>
        [HttpGet("obtener-mensajes-origen/{telefonoId}")]
        public async Task<ActionResult> GetMensajesByOrigen(long telefonoId)
        {
            try
            {
                var mensajes = await mensajeRepository.GetMensajesByOrigen(telefonoId);
                return mensajes.IsNullOrEmpty() ? NotFound("No se han encontrado mensajes como origen") : Ok(mensajes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtiene los mensajes recibidos por un número específico.
        /// </summary>
        /// <param name="telefonoId">ID del número de destino</param>
        [HttpGet("obtener-mensajes-destino/{telefonoId}")]
        public async Task<ActionResult> GetMensajesByDestino(long telefonoId)
        {
            try
            {
                var mensajes = await mensajeRepository.GetMensajesByDestino(telefonoId);
                return mensajes.IsNullOrEmpty() ? NotFound("No se han encontrado mensajes como destino") : Ok(mensajes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtiene un mensaje por su ID.
        /// </summary>
        /// <param name="mensajeId">ID del mensaje</param>
        [HttpGet("obtener-mensaje/{mensajeId}")]
        public async Task<ActionResult> GetMensajeById(int mensajeId)
        {
            try
            {
                var mensaje = await mensajeRepository.GetMensajesById(mensajeId);
                return mensaje is null ? NotFound("Este mensaje no existe") : Ok(mensaje);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Elimina un mensaje por su ID.
        /// </summary>
        /// <param name="mensajeId">ID del mensaje</param>
        [HttpDelete("eliminar-mensaje/{mensajeId}")]
        public async Task<ActionResult> RemoveMensaje(int mensajeId)
        {
            try
            {
                int result = await mensajeRepository.RemoveMensaje(mensajeId);

                if (result == -1) return NotFound("Este mensaje no existe");
                if (result == 0) return BadRequest("No se ha conseguido eliminar");

                return Ok("Eliminado correctamente");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Retorna la URL base del endpoint de la API de WhatsApp.
        /// </summary>
        private static string getUrl(string phonNumberId)
        {
            return "https://graph.facebook.com/v22.0/109348135405910/";
        }

        /// <summary>
        /// Serializa un objeto a formato JSON.
        /// </summary>
        private string CastToJson(object mensaje)
        {
            return JsonSerializer.Serialize(mensaje);
        }

        /// <summary>
        /// Envía un mensaje a la API de WhatsApp.
        /// </summary>
        /// <param name="json">Mensaje en formato JSON</param>
        /// <returns>True si se envió con éxito, False si falló</returns>
        private async Task<bool> EnviarMensaje(string json)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TOKEN);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(getUrl("") + "messages", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            Console.WriteLine(responseString);
            return false;
        }

        /// <summary>
        /// Guarda un mensaje en la base de datos.
        /// </summary>
        private async Task<bool> GuardarMensaje(long numeroOrigen, long numeroDestino, string texto, int idFichero)
        {
            if (telefonoRepository.GetTelefonosById(numeroDestino) is null)
            {
                throw new Exception($"El teléfono {numeroDestino} no está disponible o no existe");
            }

            Mensaje mensaje = string.IsNullOrEmpty(texto)
                ? mensajeRepository.ConstruirMensajeArchivo(numeroOrigen, numeroDestino, idFichero)
                : mensajeRepository.ConstruirMensajeTexto(numeroOrigen, numeroDestino, texto);

            return await mensajeRepository.AddMensaje(mensaje);
        }

        /// <summary>
        /// Guarda un fichero en la base de datos (si no existe ya).
        /// </summary>
        private async Task<int> GuardarFichero(string ruta)
        {
            try
            {
                var fichero = ficheroRepository.ConstuirFichero(new FicheroDTO { Ruta = ruta });

                if (await ficheroRepository.ExisteFichero(fichero))
                {
                    fichero = await ficheroRepository.GetFicheroByRuta(ruta);
                }
                else
                {
                    await ficheroRepository.AddFichero(fichero);
                }

                return fichero.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception(e.Message);
            }
        }
    }
}
