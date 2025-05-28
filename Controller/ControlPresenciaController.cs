using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace ApiWhatsapp.Controller
{
    public class ControlPresenciaController
    {
        private readonly HttpClient _httpClient;
        private readonly string URL;
        private string Cod;

        public ControlPresenciaController(IConfiguration _configuration)
        {
            _httpClient = new HttpClient();
            URL = _configuration["RutaControlPresencia"]!;
        }
        
        public async Task<TokenValidationDTO> IniciarSesion(string Cod)
        {
            try
            {
                var login = new LoginDTO
                {
                    Cod = Cod,
                    Password = "ANGEL"
                };

                var url = URL + "usuario/iniciar-sesion";

                var json = JsonSerializer.Serialize(login);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var token = JsonSerializer.Deserialize<TokenValidationDTO>(responseJson);
                    this.Cod = Cod;
                    return token!;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error en la solicitud: {response.StatusCode}\n{error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<string> IniciarJornada(string cod)
        {
            try
            {
                var token = await IniciarSesion(cod);
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var url = URL + "reloj/empezar-jornada/" + Cod;

                var response = await _httpClient.PostAsync(url, null);

                string contenido = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
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

        public async Task<string> ReanudarJornada(string cod)
        {
            try
            {
                var token = await IniciarSesion(cod);

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

        public async Task<string> PausarJornada(string cod)
        {
            try
            {
                var token = await IniciarSesion(cod);

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

        public async Task<string> FinalizarJornada(string cod)
        {
            try
            {
                var token = await IniciarSesion(cod);

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

        public class LoginDTO()
        {
            public string Cod { get; set; }
            public string Password { get; set; }
        }

        public class TokenValidationDTO
        {
            [JsonPropertyName("token")]
            public string token { get; set; }
            [JsonPropertyName("expiration")]
            public DateTime expiration { get; set; }
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
                        return msg.GetString(); // Devuelve el primer mensaje de error encontrado
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
    }
}
