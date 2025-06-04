using System.Net.Http.Headers;
using System.Text.Json;
using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.Controller
{
    [ApiController]
    [Route("control-presencia")]
    public class ControlPresenciaController
    {
        private readonly HttpClient _httpClient;
        private readonly string URL;
        private string Cod;
        private TokenValidationDTO _tokenActual;
        private DbWhatsapp _context;


        public ControlPresenciaController(IConfiguration _configuration, MensajesController mensajes, DbWhatsapp _context, DbTerceros terceros, IMapper mapper)
        {
            _httpClient = new HttpClient();
            URL = _configuration["RutaControlPresencia"]!;
            Cod = "";
            this._context = _context;
        }

        [HttpPost("iniciar-jornada")]
        public async Task<string> IniciarJornada(string cod)
        {
            try
            {
                Console.WriteLine(cod);
                var token = await ObtenerToken(cod);

                var url = URL + "reloj/empezar-jornada/" + Cod;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var response = await _httpClient.PostAsync(url, null);

                string contenido = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return "00:00:00";
                }
                else
                {
                    Console.WriteLine(response.StatusCode);
                    try
                    {
                        using var doc = JsonDocument.Parse(contenido);
                        return GetMensajeError(doc);
                    }
                    catch
                    {
                        return contenido;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        [HttpPost("reaunudar-jornada")]
        public async Task<string> ReaunudarJornada(string cod)
        {
            try
            {
                var token = await ObtenerToken(cod);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var url = URL + "reloj/reanudar-jornada/" + Cod;

                var response = await _httpClient.PutAsync(url, null);

                string contenido = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // El contenido ya es texto plano como "Jornada iniciada"
                    return "00:00:00";
                }
                else
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(contenido);
                        return GetMensajeError(doc);
                    }
                    catch
                    {
                        return contenido;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        [HttpPost("pausar-jornada")]
        public async Task<string> PausarJornada(string cod)
        {
            try
            {
                var token = await ObtenerToken(cod);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var url = URL + "reloj/pausar-jornada/" + Cod;

                var response = await _httpClient.PutAsync(url, null);

                string contenido = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // El contenido ya es texto plano como "Jornada iniciada"
                    string mensaje = contenido.Substring(1, contenido.Length - 2);
                    Console.WriteLine(mensaje);
                    return mensaje;
                }
                else
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(contenido);
                        return GetMensajeError(doc);
                    }
                    catch
                    {
                        return contenido;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        [HttpPost("finalizar-jornada")]
        public async Task<string> FinalizarJornada(string cod)
        {
            try
            {
                var token = await ObtenerToken(cod);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var url = URL + "reloj/finalizar-jornada/" + Cod;

                var response = await _httpClient.PutAsync(url, null);

                string contenido = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // El contenido ya es texto plano como "Jornada iniciada"
                    string mensaje = contenido.Substring(1, contenido.Length - 2);
                    Console.WriteLine(mensaje);
                    return mensaje;
                }
                else
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(contenido);
                        return GetMensajeError(doc);
                    }
                    catch
                    {
                        return contenido;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        private string GetMensajeError(JsonDocument doc)
        {
            var root = doc.RootElement;

            // Manejo de errores tipo validation problem details
            if (root.TryGetProperty("errors", out var errores))
            {
                foreach (var prop in errores.EnumerateObject())
                {
                    foreach (var msg in prop.Value.EnumerateArray())
                    {
                        return msg.GetString()!; // Devuelve el primer mensaje de error encontrado
                    }
                }
            }

            // Por si hay un "message"
            if (root.TryGetProperty("message", out var mensaje))
            {
                return mensaje.GetString();
            }

            return "error"; // Si no encuentra nada útil, devuelve todo
        }

        private async Task<TokenValidationDTO> ObtenerToken(string cod)
        {

            Telefono? telefono = await _context.Telefonos.FirstOrDefaultAsync(x => x.IdGenerales == cod);
            if (telefono == null)
                return null;

            var url = $"{URL}usuario/obtener-token?userCod={Uri.EscapeDataString(cod)}&tokenValidation={Uri.EscapeDataString(telefono.Token)}";

            // Crear la solicitud HTTP con header personalizado
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("userCod", cod);
            request.Headers.Add("tokenValidation", telefono.Token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var tokenDto = JsonSerializer.Deserialize<TokenValidationDTO>(responseJson);

                this.Cod = cod;

                telefono.Token = tokenDto.token;

                _context.Update(telefono);

                return tokenDto;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error en la solicitud: {response.StatusCode}\n{error}");
                return null;
            }
        }
    }
}