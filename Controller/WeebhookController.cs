using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;
using ApiWhatsapp.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ApiWhatsapp.Controller
{
    [ApiController]
    [Route("webhook")]
    public class WhatsappWebhookController : ControllerBase
    {

        private const string VERIFY_TOKEN = "rUVHBwXaFGlI0OOLpC5TdByEzzDI8LGlJayhXUz0";
        private WebhookHelper webhookHelper;

        public WhatsappWebhookController(DbWhatsapp context, IMapper mapper, IConfiguration configuration)
        {
            webhookHelper = new WebhookHelper(context, mapper, configuration);
        }

        /// <summary>
        /// Verifica que el endpoint sea valido
        /// </summary>
        [HttpGet]
        public IActionResult Verify([FromQuery(Name = "hub.mode")] string mode, [FromQuery(Name = "hub.challenge")] string challenge, [FromQuery(Name = "hub.verify_token")] string verify_token)
        {
            Console.WriteLine($"hub_mode: {mode}, hub_challenge: {challenge}, hub_verify_token: {verify_token}");

            if (mode == "subscribe" && verify_token == VERIFY_TOKEN)
            {
                return Ok(challenge);
            }

            return Forbid();
        }

        /// <summary>
        /// Obtiene los mesajes enviados y recibidos en formato Json a traves de meta
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Recive()
        {
            var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine("Webhook recibido: ");
            Console.WriteLine(body);

            try
            {
                var payload = JsonConvert.DeserializeObject<WebhookPayload>(body);
                var contact = payload?.entry.First().changes.First().value.contacts.FirstOrDefault();

                var messages = payload?.entry?[0]?.changes?[0]?.value?.messages;
                if (messages != null)
                {
                    // Logica para guardar el mensaje
                    foreach (var message in messages)
                    {
                        await webhookHelper.GuardarMensaje(message, contact!.profile.name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando webhook: {ex}");
            }

            return Ok();
        }
    }
}
