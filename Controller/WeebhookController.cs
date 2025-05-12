using System;
using System.IO;
using System.Threading.Tasks;
using ApiWhatsapp.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ApiWhatsapp.Controller
{
    [ApiController]
    [Route("webhook")]
    public class WhatsappWebhookController : ControllerBase
    {

        private const string VERIFY_TOKEN = "whatsapp_webhook_verification_token";

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

                var message = payload.entry[0].changes[0].value.messages?[0];
                if (message != null)
                {
                    Console.WriteLine($"Mensaje recibido de {message.from}: {message.text.body}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando webhook: {ex.Message}");
            }

            return Ok();
        }
    }
}
