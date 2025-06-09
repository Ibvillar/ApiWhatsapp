using ApiWhatsapp.BBDD;
using ApiWhatsapp.Controller;
using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entities;
using ApiWhatsapp.Entitties;
using ApiWhatsapp.Repositories;
using AutoMapper;
using Newtonsoft.Json;

namespace ApiWhatsapp.Helpers
{
    /// <summary>
    /// Clase auxiliar que maneja la lógica relacionada con los botones interactivos de WhatsApp para control de jornada laboral.
    /// </summary>
    public class BotonesHelper
    {
        private readonly MensajesController _mensajeController;
        private readonly ControlPresenciaController _controller;
        private readonly TelefonoRepository _telefonosRepository;
        private readonly LocalizacionRepository _localizacionRepository;

        public BotonesHelper(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper, IConfiguration _configuracion)
        {
            _localizacionRepository = new LocalizacionRepository(context);
            _mensajeController = new MensajesController(context, contextTerceros, mapper, _configuracion);
            _controller = new ControlPresenciaController(_configuracion, _mensajeController, context, contextTerceros, mapper);
            _telefonosRepository = new TelefonoRepository(context, contextTerceros, mapper);
        }

        /// <summary>
        /// Procesa la respuesta del usuario a través de un botón interactivo.
        /// </summary>
        /// <param name="mensaje">Mensaje recibido desde el webhook de WhatsApp.</param>
        public async Task ResponderMensaje(MessageWebhook mensaje)
        {
            int id = GetId(mensaje);
            var codUsuario = await GetCodFromNumber(mensaje.from);

            switch (id)
            {
                case 1:
                    bool tieneUbicacion = await _localizacionRepository.UsuarioTieneLocalizacion(long.Parse(mensaje.from));

                    if (!tieneUbicacion)
                    {
                        await EnviarMensajeLcalizacion(mensaje.from);
                        return;
                    }

                    await ProcesarAccion(id, await _controller.IniciarJornada(codUsuario), mensaje.from);
                    break;
                case 2:
                    await ProcesarAccion(id, await _controller.PausarJornada(codUsuario), mensaje.from);
                    break;
                case 3:
                    await ProcesarAccion(id, await _controller.ReaunudarJornada(codUsuario), mensaje.from);
                    break;
                case 4:
                    await ProcesarAccion(id, await _controller.FinalizarJornada(codUsuario), mensaje.from);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Envía un mensaje solicitando la ubicación al usuario.
        /// </summary>
        /// <param name="telefono">Número de teléfono del usuario.</param>
        private async Task EnviarMensajeLcalizacion(string telefono)
        {
            await _mensajeController.EnviarMensajeBoton("📍 Por favor, comparte tu ubicación antes de registrar la jornada.", telefono, 1);
        }

        /// <summary>
        /// Procesa la acción correspondiente (iniciar, pausar, reanudar, finalizar) y envía la respuesta al usuario.
        /// </summary>
        /// <param name="idAccion">Identificador de la acción realizada.</param>
        /// <param name="error">Resultado o mensaje de error.</param>
        /// <param name="numero">Número de teléfono del usuario.</param>
        private async Task ProcesarAccion(int idAccion, string error, string numero)
        {
            if (isError(error))
                await enviarMensaje(error, numero, "error_control_presencia");
            else
                switch (idAccion) {
                    case 1:
                        await enviarMensaje("iniciado", numero, "succes_control_presencia");
                        break;
                    case 2:
                        await enviarMensaje("pausado", numero, "succes_control_presencia");
                        break;
                    case 3:
                        await enviarMensaje("reaunudado", numero, "succes_control_presencia");
                        break;
                    case 4:
                        await enviarMensaje("finalizado", numero, "succes_control_presencia");
                        break;
                } 
        }

        private async Task enviarMensaje(string texto, string numero, string nombrePlantilla)
        {
            await _mensajeController.EnviarMensaje(
                JsonConvert.SerializeObject(new JsonMensajeBienvenida
                {
                    to = numero.ToString(),
                    template = new Template
                    {
                        name = nombrePlantilla,
                        language = new Language { code = "es" },
                        components =
                        [
                            new Component
                            {
                                type = "body",
                                parameters =
                                [
                                    new Parameter { type = "text", text = texto },
                                    new Parameter { type = "text", text = DateTime.Now.ToString("HH:mm")}
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
                            new Component
                            {
                                type = "button",
                                sub_type = "quick_reply",
                                index = "1",
                                parameters = new List<Parameter>
                                {
                                    new Parameter { type = "payload", payload = "pausar_jornada" }
                                }
                            },
                            new Component
                            {
                                type = "button",
                                sub_type = "quick_reply",
                                index = "2",
                                parameters = new List<Parameter>
                                {
                                    new Parameter { type = "payload", payload = "reaunudar_jornada" }
                                }
                            },
                            new Component
                            {
                                type = "button",
                                sub_type = "quick_reply",
                                index = "3",
                                parameters = new List<Parameter>
                                {
                                    new Parameter { type = "payload", payload = "finalizar_jornada" }
                                }
                            },
                        ]
                    }
                },
                Formatting.Indented
             ));
        }

        /// <summary>
        /// Extrae el ID del botón presionado a partir del mensaje recibido.
        /// </summary>
        /// <param name="mensaje">Mensaje del webhook.</param>
        /// <returns>ID del botón presionado como entero.</returns>
        private int GetId(MessageWebhook mensaje)
        {
            return int.Parse(mensaje.interactive.button_reply.id);
        }

        /// <summary>
        /// Determina si el resultado representa un error.
        /// </summary>
        /// <param name="result">Texto del resultado.</param>
        /// <returns><c>true</c> si el resultado es un error; de lo contrario, <c>false</c>.</returns>
        private bool isError(string result)
        {
            try
            {
                Console.WriteLine(result);
                int.Parse(result.Substring(0, 2));
                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Obtiene el código de usuario (IdGenerales) a partir del número de teléfono.
        /// </summary>
        /// <param name="numero">Número de teléfono como string.</param>
        /// <returns>Código de usuario.</returns>
        private async Task<string> GetCodFromNumber(string numero)
        {
            long longNumber = long.Parse(numero);
            Telefono telefono = await _telefonosRepository.GetTelefonosById(longNumber);
            return telefono.IdGenerales;
        }
    }
}
