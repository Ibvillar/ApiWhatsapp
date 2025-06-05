using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ApiWhatsapp.Controller
{
    /// <summary>
    /// Controlador encargado de gestionar el control de presencia de usuarios (inicio, pausa, reanudación y finalización de jornada).
    /// </summary>
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

        /// <summary>
        /// Inicia la jornada de un usuario a través de su código.
        /// </summary>
        /// <param name="cod">Código del usuario.</param>
        /// <returns>Mensaje con el resultado de la operación.</returns>
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
                    return "00:00:00";
                else
                    return TryParseError(contenido);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        /// <summary>
        /// Reanuda la jornada. Tiene que estar pausada primero.
        /// </summary>
        /// <param name="cod">Código del usuario.</param>
        /// <returns>Mensaje con el resultado de la operación.</returns>
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

                return response.IsSuccessStatusCode ? "00:00:00" : TryParseError(contenido);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        /// <summary>
        /// Pausa la jornada actual del usuario. Tiene que estar iniciada primero
        /// </summary>
        /// <param name="cod">Código del usuario.</param>
        /// <returns>Mensaje con el resultado de la operación.</returns>
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

                return response.IsSuccessStatusCode
                    ? contenido.Substring(1, contenido.Length - 2)
                    : TryParseError(contenido);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        /// <summary>
        /// Finaliza la jornada laboral del usuario. Primero tiene que estar iniciada
        /// </summary>
        /// <param name="cod">Código del usuario.</param>
        /// <returns>Mensaje con el resultado de la operación.</returns>
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

                return response.IsSuccessStatusCode
                    ? contenido.Substring(1, contenido.Length - 2)
                    : TryParseError(contenido);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        /// <summary>
        /// Extrae el mensaje de error desde un documento JSON de error HTTP.
        /// </summary>
        /// <param name="doc">Documento JSON con detalles del error.</param>
        /// <returns>Primer mensaje de error encontrado, o mensaje genérico.</returns>
        private string GetMensajeError(JsonDocument doc)
        {
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errores))
            {
                foreach (var prop in errores.EnumerateObject())
                {
                    foreach (var msg in prop.Value.EnumerateArray())
                        return msg.GetString()!;
                }
            }

            if (root.TryGetProperty("message", out var mensaje))
                return mensaje.GetString();

            return "error";
        }

        /// <summary>
        /// Renueva el token del usuario y lo devuelve.
        /// </summary>
        /// <param name="cod">Código del usuario.</param>
        /// <returns>Objeto <see cref="TokenValidationDTO"/> con el token actualizado.</returns>
        private async Task<TokenValidationDTO> ObtenerToken(string cod)
        {
            Telefono? telefono = await _context.Telefonos.FirstOrDefaultAsync(x => x.IdGenerales == cod);
            if (telefono == null) return null!;

            var url = $"{URL}usuario/obtener-token?userCod={Uri.EscapeDataString(cod)}&tokenValidation={Uri.EscapeDataString(telefono.Token)}";

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

        /// <summary>
        /// Intenta extraer un mensaje de error de una cadena JSON.
        /// </summary>
        /// <param name="contenido">Contenido JSON como string.</param>
        /// <returns>Mensaje de error legible.</returns>
        private string TryParseError(string contenido)
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
}
