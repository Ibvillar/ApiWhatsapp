
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

namespace ApiWhatsapp.Controller
{
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

        public MensajesController(DbWhatsapp context, IMapper mapper)
        {
            _httpClient = new HttpClient();
            _mensajesHelper = new MensajeHelper(TOKEN, getUrl(""));
            ficheroRepository = new FicheroRepository(context, mapper);
            mensajeRepository = new MensajeRepository(context);
            telefonoRepository = new TelefonoRepository(context, mapper);
        }

        [HttpPost("enviar-texto")]
        public async Task<ActionResult> EnviarMensajeTexto(long numeroDestino, string texto)
        {
            try {
                await GuardarMensaje(34644288224, numeroDestino, texto, -1);
            }
            catch (Exception e) {
                return BadRequest(e.Message);
            }

            var mensaje = _mensajesHelper.ConstruirMensajeTexto(numeroDestino, texto);
            var json = CastToJson(mensaje);

            var respuesta = await EnviarMensaje(json);

            if (!respuesta)
                return BadRequest("Algo salio mal al enviar el mensaje");

            return Ok();

        }

        [HttpPost("enviar-imagen")]
        public async Task<ActionResult> EnviarMensajeImagen(long numeroDestino, string ruta)
        {
            try
            {
                int idFichero = await GuardarFichero(ruta);
                await GuardarMensaje(34644288224, numeroDestino, "", idFichero);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            var mensaje = await _mensajesHelper.ConstruirMensajeImagen(numeroDestino, ruta);
            var json = CastToJson(mensaje);

            var respuesta = await EnviarMensaje(json);

            if (!respuesta)
            {
                return BadRequest("Algo salio mal al enviar el mensaje");
            }

            return Ok();
        }

        [HttpPost("enviar-documento")]
        public async Task<ActionResult> EnviarMensajeDocumento(long numeroDestino, string nombre, string ruta)
        {
            try
            {
                int idFichero = await GuardarFichero(ruta);
                await GuardarMensaje(34644288224, numeroDestino, "", idFichero);
            }
            catch (Exception e)
            {
                Console.WriteLine("1");
                return BadRequest(e.Message);
            }

            var mensaje = await _mensajesHelper.ConstruirMensajeDocumento(numeroDestino, nombre, ruta);
            var json = CastToJson(mensaje);

           var respuesta = EnviarMensaje(json).Result;

            if (!respuesta)
            {
                return BadRequest("Algo salio mal al enviar el mensaje");
            }

            return Ok();
        }

        [HttpPut("cambiar-a-leido/{mensajeId}")]
        public async Task<ActionResult> CambiarALeido(int mensajeId)
        {
            try
            {
                int result = await mensajeRepository.SetLeido(mensajeId);

                if (result == 0)
                {
                    return NotFound("Este mensaje no existe");
                }

                return Ok("Se ha cambiado correctamente");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("obtener-mensajes")]
        public async Task<ActionResult> GetAllMensajes()
        {
            try
            {
                List<Mensaje> mensajes = await mensajeRepository.GetMensajes();

                if (mensajes.IsNullOrEmpty())
                {
                    return NotFound("No hay mensajes disponibles");
                }

                return Ok(mensajes);
            } 
            catch (Exception e) 
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("obtener-mensajes-origen/{telefonoId}")]
        public async Task<ActionResult> GetMensajesByOrigen(long telefonoId)
        {
            try
            {
                List<Mensaje> mensajes = await mensajeRepository.GetMensajesByOrigen(telefonoId);

                if (mensajes.IsNullOrEmpty())
                {
                    return NotFound("No se han encontrado mensajes relacionados con este numero como origen");
                }

                return Ok(mensajes);
            } 
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("obtener-mensajes-destino/{telefonoId}")]
        public async Task<ActionResult> GetMensajesByDestino(long telefonoId)
        {
            try
            {
                List<Mensaje> mensajes = await mensajeRepository.GetMensajesByDestino(telefonoId);

                if (mensajes.IsNullOrEmpty())
                {
                    return NotFound("No se han encontrado mensajes relacionados con este numero como destino");
                }

                return Ok(mensajes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("obtener-mensaje/{mensajeId}")]
        public async Task<ActionResult> GetMensajeById(int mensajeId)
        {
            try
            {
                Mensaje mensaje = await mensajeRepository.GetMensajesById(mensajeId);

                if (mensaje is null)
                {
                    return NotFound("Este mensaje no existe");
                }

                return Ok(mensaje);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("eliminar-mensaje/{mensajeId}")]
        public async Task<ActionResult> RemoveMensaje(int mensajeId)
        {
            try
            {
                int result = await mensajeRepository.RemoveMensaje(mensajeId);

                if (result == -1)
                {
                    return NotFound("Este mensaje no existe");
                }
                if (result == 0)
                {
                    return BadRequest("No se ha conseguido eliminar");
                }

                return Ok("Eliminado correctamente");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private static string getUrl(string phonNumberId)
        {
            return "https://graph.facebook.com/v22.0/109348135405910/";
        }

        private string CastToJson(object mensaje)
        {
            return JsonSerializer.Serialize(mensaje);
        }

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
            else
            {
                Console.WriteLine(responseString);
                return false;
            }
        }

        private async Task<bool> GuardarMensaje(long numeroOrigen, long numeroDestino, string texto, int idFichero)
        {
            if (telefonoRepository.GetTelefonosById(numeroDestino) is null)
            {
                throw new Exception($"El telefono {numeroDestino} no esta disponible o no existe");
            }

            Mensaje mensaje;

            if (texto is "")
            {
                mensaje = mensajeRepository.ConstruirMensajeArchivo(34644288224, numeroDestino, idFichero);
            }
            else
            {
                mensaje = mensajeRepository.ConstruirMensajeTexto(34644288224, numeroDestino, texto);
            }

            return await mensajeRepository.AddMensaje(mensaje);
        }

        private async Task<int> GuardarFichero(string ruta)
        {
            try
            {
                Fichero fichero = ficheroRepository.ConstuirFichero(new FicheroDTO { Ruta = ruta });

                if (!ficheroRepository.ExisteFichero(fichero))
                {
                    await ficheroRepository.AddFichero(fichero);
                } else
                {
                    fichero = ficheroRepository.GetFicheroByRuta(ruta);
                }

                    Console.WriteLine(fichero.Id);

                return fichero.Id;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
