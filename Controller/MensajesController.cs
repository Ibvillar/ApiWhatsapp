using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using ApiWhatsapp.EnvioMensajes;
using System.Text;
using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;
using AutoMapper;
using ApiWhatsapp.DTO;
using Microsoft.IdentityModel.Tokens;
using ApiWhatsapp.Entities;
using ApiWhatsapp.Helpers;
using Newtonsoft.Json;

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
        private readonly IMapper mapper;
        private readonly IConfiguration _configuracion;

        public MensajesController(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper, IConfiguration _configuracion)
        {
            _httpClient = new HttpClient();
            _mensajesHelper = new MensajeHelper(TOKEN, getUrl(""), _configuracion);
            ficheroRepository = new FicheroRepository(context, mapper);
            mensajeRepository = new MensajeRepository(context);
            telefonoRepository = new TelefonoRepository(context, contextTerceros, mapper);
            botonRepository = new BotonRepository(context);
            NumeroEmpresa = long.Parse(_configuracion["NumeroEmpresa"]!);
            this.mapper = mapper;
            this._configuracion = _configuracion;
        }

        /// <summary>
        /// Envía un mensaje de texto a un número específico.
        /// </summary>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="texto">Texto del mensaje</param>
        /// <returns>Resultado de la operación HTTP</returns>
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

            Console.WriteLine(json.ToString());

            return respuesta ? Ok() : BadRequest("Algo salió mal al enviar el mensaje");
        }

        /// <summary>
        /// Envía una imagen a un número específico.
        /// </summary>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="file">Archivo de imagen a enviar</param>
        /// <returns>Resultado de la operación HTTP</returns>
        [HttpPost("enviar-imagen")]
        public async Task<ActionResult> EnviarMensajeImagen(long numeroDestino, IFormFile file)
        {
            try
            {
                string rutaArchivo = await _mensajesHelper.UploadFile(file);
                if (string.IsNullOrEmpty(rutaArchivo))
                    return BadRequest("No se pudo guardar el archivo");

                int idFichero = await GuardarFichero(Path.GetFileName(rutaArchivo));
                await GuardarMensaje(NumeroEmpresa, numeroDestino, "", idFichero);

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
        /// <param name="file">Archivo de documento a enviar</param>
        /// <returns>Resultado de la operación HTTP</returns>
        [HttpPost("enviar-documento")]
        public async Task<ActionResult> EnviarMensajeDocumento(long numeroDestino, IFormFile file)
        {
            try
            {
                string rutaArchivo = await _mensajesHelper.UploadFile(file);
                if (string.IsNullOrEmpty(rutaArchivo))
                    return BadRequest("No se pudo guardar el archivo");

                string nombreDocumento = Path.GetFileName(rutaArchivo);

                int idFichero = await GuardarFichero(nombreDocumento);
                await GuardarMensaje(NumeroEmpresa, numeroDestino, "", idFichero);

                var mensaje = await _mensajesHelper.ConstruirMensajeDocumento(numeroDestino, nombreDocumento, rutaArchivo);
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
        /// Envía un mensaje con botones a un número de teléfono.
        /// </summary>
        /// <param name="cuerpo">Texto del mensaje.</param>
        /// <param name="numero">Número de destino.</param>
        /// <param name="idBoton">IDs de los botones a incluir (máx. 3).</param>
        /// <returns>Resultado de la operación HTTP</returns>
        [HttpPost("enviar-boton")]
        public async Task<ActionResult> EnviarMensajeBoton(string cuerpo, string numero, params int[] idBoton)
        {
            if (idBoton == null || idBoton.Length == 0)
                return BadRequest("Se debe proporcionar al menos un ID de botón.");

            List<DTO.ButtonReply> botones = new List<DTO.ButtonReply>();

            foreach (var id in idBoton)
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
        /// Envía un mensaje de bienvenida a un nuevo usuario.
        /// </summary>
        /// <param name="mensaje">DTO con información del mensaje de bienvenida</param>
        /// <returns>Resultado de la operación HTTP</returns>
        [HttpPost("mensaje-bienvenida")]
        public async Task<ActionResult> EnviarMensajeBienvenida(MensajeBienvenidaDTO mensaje)
        {
            try
            {
                long Id = long.Parse(mensaje.Telefono.Prefijo.ToString() + mensaje.Telefono.Numero.ToString());

                var telefonoDb = await telefonoRepository.GetTelefonosById(Id);

                Telefono telefono;

                if (telefonoDb is null)
                {
                    telefono = telefonoRepository.ConstruirTelefono(mensaje.Telefono.Numero, mensaje.Telefono.Prefijo, mensaje.Telefono.Nombre);
                    await telefonoRepository.AddTelefono(telefono);
                }
                else
                {
                    telefono = telefonoDb;
                }

                long telefonoId = telefono.Id;

                await telefonoRepository.UpdateToken(telefonoId, mensaje.Token);
                await telefonoRepository.ValidateNumber(mensaje.Telefono);
                await telefonoRepository.AddCodigo(telefono, mensaje.Telefono.IdGenerales);
                await telefonoRepository.SetUbicacion(mensaje.ubicacion, telefonoId);

                var mensaje1 = new JsonMensajeBienvenida
                {
                    to = telefonoId.ToString(),
                    template = new Template
                    {
                        name = "mensaje_bienvenida",
                        language = new Language { code = "es" },
                        components =
                        [
                            new Component
                            {
                                type = "body",
                                parameters =
                                [
                                    new Parameter { type = "text", text = telefono.Nombre }
                                ]
                            },
                            new Component
                            {
                                type = "button",
                                sub_type = "quick_reply",
                                index = "0",
                                parameters = new List<Parameter>
                                {
                                    new Parameter { type = "payload", payload = "iniciar_jornada" }
                                }
                            },
                        ]
                    }
                };

                var json = JsonConvert.SerializeObject(mensaje1, Formatting.Indented);
                bool enviado = await EnviarMensaje(json);

                Console.WriteLine(json.ToString());

                if (!enviado) 
                    return BadRequest("No se ha podido mandar el mensaje de bienvenida correctamente");
                    

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Cambia el estado de un mensaje a leído.
        /// </summary>
        /// <param name="mensajeId">ID del mensaje</param>
        /// <returns>Resultado de la operación HTTP</returns>
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
        /// <returns>Lista de mensajes o NotFound si no hay mensajes</returns>
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
        /// <returns>Lista de mensajes o NotFound si no existen</returns>
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
        /// <returns>Lista de mensajes o NotFound si no existen</returns>
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
        /// <returns>Mensaje solicitado o NotFound si no existe</returns>
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
        /// <param name="mensajeId">ID del mensaje a eliminar</param>
        /// <returns>Resultado de la operación HTTP</returns>
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
        /// <param name="phonNumberId">ID del número telefónico (no usado actualmente)</param>
        /// <returns>URL base como cadena</returns>
        private static string getUrl(string phonNumberId)
        {
            return "https://graph.facebook.com/v22.0/109348135405910/";
        }

        /// <summary>
        /// Serializa un objeto a formato JSON.
        /// </summary>
        /// <param name="mensaje">Objeto a serializar</param>
        /// <returns>Cadena JSON</returns>
        private string CastToJson(object mensaje)
        {
            return System.Text.Json.JsonSerializer.Serialize(mensaje);
        }

        /// <summary>
        /// Envía un mensaje a la API de WhatsApp.
        /// </summary>
        /// <param name="json">Mensaje en formato JSON</param>
        /// <returns>True si se envió con éxito, False si falló</returns>
        [NonAction]
        public async Task<bool> EnviarMensaje(string json)
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
        /// <param name="numeroOrigen">Número de origen</param>
        /// <param name="numeroDestino">Número destino</param>
        /// <param name="texto">Texto del mensaje</param>
        /// <param name="idFichero">ID del fichero asociado, si aplica</param>
        /// <returns>True si se guardó correctamente</returns>
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
        /// <param name="ruta">Ruta o nombre del fichero</param>
        /// <returns>ID del fichero guardado o existente</returns>
        private async Task<int> GuardarFichero(string ruta)
        {
            try
            {
                string nuevaRuta = _configuracion["RutaFicherosLocal"]! + ruta;

                FicherosHelper.CopiarArchivo(ruta, nuevaRuta);

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
