
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using ApiWhatsapp.EnvioMensajes;
using System.Text;
using System.Text.Json;

namespace ApiWhatsapp.Controller
{
    [ApiController]
    [Route("mensajes")]
    public class MensajesController: ControllerBase
    {
        private static readonly string TOKEN = "EAAHbxd02hJUBOZBOv4ZBzlQOtnojQLixKdobeqIz654prmYhyHXZBJCLXMBfyuBHt8ckCaBWILHENAmfRMDUhEoY3kHZBuaxsBmJMBAiarzNZADbLj6bVsrf288U3qdYtCXgiE5AZCfN0oFuXESDsOBmDYcB2aKE3zqnnsDYumU5T3XZAmVb8a1ZBqfUnNEmxgDp0liEh6zeo01Kei90";
        private readonly HttpClient _httpClient;
        private MensajeHelper _mensajesHelper;

        public MensajesController()
        {
            _httpClient = new HttpClient();
            _mensajesHelper = new MensajeHelper(TOKEN, getUrl(""));
        }

        [HttpPost("texto")]
        public async Task<ActionResult> EnviarMensajeTexto(long numeroDestino, string texto)
        {
            var mensaje = _mensajesHelper.ConstruirMensajeTexto(numeroDestino, texto);
            var json = CastToJson(mensaje);

            var respuesta = await EnviarMensaje(json);

            if (respuesta)
            {
                return Ok();
            } else
            {
                return BadRequest("Algo salio mal al enviar el mensaje");
            }
        }

        [HttpPost("imagen")]
        public async Task<ActionResult> EnviarMensajeImagen(long numeroDestino, string ruta)
        {
            var mensaje = await _mensajesHelper.ConstruirMensajeImagen(numeroDestino, ruta);
            var json = CastToJson(mensaje);

            var respuesta = await EnviarMensaje(json);

            if (respuesta)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Algo salio mal al enviar el mensaje");
            }
        }

        [HttpPost("documento")]
        public async Task<ActionResult> EnviarMensajeDocumento(long numeroDestino, string nombre, string ruta)
        {
            var mensaje = await _mensajesHelper.ConstruirMensajeDocumento(numeroDestino, nombre, ruta);
            var json = CastToJson(mensaje);

            var respuesta = EnviarMensaje(json).Result;

            if (respuesta)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Algo salio mal al enviar el mensaje");
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
    }
}
