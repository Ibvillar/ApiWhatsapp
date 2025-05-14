
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

namespace ApiWhatsapp.Controller
{
    [ApiController]
    [Route("mensajes")]
    public class MensajesController : ControllerBase
    {
        private static readonly string TOKEN = "EAAHbxd02hJUBOZBOv4ZBzlQOtnojQLixKdobeqIz654prmYhyHXZBJCLXMBfyuBHt8ckCaBWILHENAmfRMDUhEoY3kHZBuaxsBmJMBAiarzNZADbLj6bVsrf288U3qdYtCXgiE5AZCfN0oFuXESDsOBmDYcB2aKE3zqnnsDYumU5T3XZAmVb8a1ZBqfUnNEmxgDp0liEh6zeo01Kei90";
        private readonly DbWhatsapp context;
        private readonly HttpClient _httpClient;
        private MensajeHelper _mensajesHelper;
        private FicheroRepository ficheroRepository;
        private MensajeRepository mensajeRepository;
        private TelefonoRepository telefonoRepository;

        public MensajesController(DbWhatsapp context, IMapper mapper)
        {
            this.context = context;
            _httpClient = new HttpClient();
            _mensajesHelper = new MensajeHelper(TOKEN, getUrl(""));
            ficheroRepository = new FicheroRepository(context, mapper);
            mensajeRepository = new MensajeRepository(context);
            telefonoRepository = new TelefonoRepository(context, mapper);
        }

        [HttpPost("texto")]
        public async Task<ActionResult> EnviarMensajeTexto(long numeroDestino, string texto)
        {
            try {
                GuardarMensaje(34644288224, numeroDestino, texto, 0);
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

        [HttpPost("imagen")]
        public async Task<ActionResult> EnviarMensajeImagen(long numeroDestino, string ruta)
        {
            try
            {
                GuardarFichero(ruta);
                GuardarMensaje(34644288224, numeroDestino, null!, 0);
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

        [HttpPost("documento")]
        public async Task<ActionResult> EnviarMensajeDocumento(long numeroDestino, string nombre, string ruta)
        {
            try
            {
                GuardarFichero(ruta);
                GuardarMensaje(34644288224, numeroDestino, null!, 0);
            }
            catch (Exception e)
            {
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

        private bool GuardarMensaje(long numeroOrigen, long numeroDestino, string texto, int idFichero)
        {
            if (telefonoRepository.GetTelefonosById(numeroDestino) is null)
            {
                throw new Exception($"El telefono {numeroDestino} no esta disponible o no existe");
            }

            Mensaje mensaje;

            if (texto is null)
            {
                mensaje = mensajeRepository.ConstruirMensajeArchivo(34644288224, numeroDestino, idFichero);
            }
            else
            {
                mensaje = mensajeRepository.ConstruirMensajeTexto(34644288224, numeroDestino, texto);
            }

            return mensajeRepository.AddMensaje(mensaje);
        }

        private async void GuardarFichero(string ruta)
        {
            try
            {
                Fichero fichero = ficheroRepository.ConstuirFichero(new FicheroDTO { Ruta = ruta });

                if (!ficheroRepository.ExisteFichero(fichero))
                {
                    await ficheroRepository.AddFichero(fichero);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
