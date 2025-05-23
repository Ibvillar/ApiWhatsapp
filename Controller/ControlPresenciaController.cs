using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core.Serialization;

namespace ApiWhatsapp.Controller
{
    public class ControlPresenciaController
    {
        private readonly HttpClient _httpClient;
        private readonly string URL;
        private string Cod;

        public ControlPresenciaController()
        {
            _httpClient = new HttpClient();
            URL = "http://localhost:5113/";
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

        public async Task<string> IniciarJornada()
        {
            try
            {
                var token = await IniciarSesion("000999");
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var url = URL + "reloj/empezar-jornada/" + Cod;

                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();
                    return "";
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return error;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        public async Task<string> ReanudarJornada()
        {
            try
            {
                var token = await IniciarSesion("000999");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var url = URL + "reloj/reanudar-jornada/" + Cod;

                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Respuesta del servidor: {resultado}");
                    return "";
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error: {response.StatusCode}\n{error}");
                    return error;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        public async Task<string> PausarJornada()
        {
            try
            {
                var token = await IniciarSesion("000999");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var url = URL + "reloj/pausar-jornada/" + Cod;

                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Respuesta del servidor: {resultado}");
                    return "";
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error: {response.StatusCode}\n{error}");
                    return error;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        public async Task<string> FinalizarJornada()
        {
            try
            {
                var token = await IniciarSesion("000999");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

                var url = URL + "reloj/finalizar-jornada/" + Cod;

                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Respuesta del servidor: {resultado}");
                    return "";
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error: {response.StatusCode}\n{error}");
                    return error;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "Ha ocurrido un error";
            }
        }

        public async Task MensajeError()
        {

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
    }
}
