using ApiWhatsapp.BBDD;
using ApiWhatsapp.Controller;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entities;
using ApiWhatsapp.Entitties;
using ApiWhatsapp.Repositories;
using AutoMapper;

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
            var texto = ConstruirCuerpo(idAccion, error);
            var botonesSiguientes = ObtenerBotonesSiguientes(idAccion, error);

            await _mensajeController.EnviarMensajeBoton(texto, numero, botonesSiguientes);
        }

        /// <summary>
        /// Construye el cuerpo del mensaje de respuesta en base al resultado de la acción.
        /// </summary>
        /// <param name="id">Identificador de la acción realizada.</param>
        /// <param name="result">Resultado o mensaje de error.</param>
        /// <returns>Mensaje personalizado para enviar al usuario.</returns>
        private string ConstruirCuerpo(int id, string result)
        {
            bool fallo = !isError(result);
            string horaActual = DateTime.Now.ToString("HH:mm");

            return id switch
            {
                1 => fallo
                    ? $"✅ Jornada iniciada con éxito.\n\n🕒 *Hora actual:* {horaActual}\n\n¡Buen trabajo! 🫡"
                    : $"❌ *No se pudo iniciar la jornada*\n\n🕒 *Hora actual:* {horaActual}\n🔁 Por favor, inténtalo de nuevo.\n\n🛠️ Detalle del error: {result}",

                2 => fallo
                    ? $"⏸️ Jornada pausada correctamente.\n\n🕒 *Hora actual:* {horaActual}\n⏱️ *Tiempo trabajado:* {result}\n\n¡Tómate un respiro! ☕"
                    : $"❌ *No se pudo pausar la jornada.*\n\n🕒 *Hora actual:* {horaActual}\n🔁 Intenta nuevamente.\n\n🛠️ Detalle del error: {result}",

                3 => fallo
                    ? $"▶️ Jornada reanudada con éxito.\n\n🕒 *Hora actual:* {horaActual}\n\n¡Seguimos! 🚀"
                    : $"❌ *No se pudo reanudar la jornada.*\n\n🕒 *Hora actual:* {horaActual}\n🔁 Por favor, vuelve a intentarlo.\n\n🛠️ Detalle del error: {result}",

                4 => fallo
                    ? $"✅ Jornada finalizada correctamente.\n\n🕒 *Hora actual:* {horaActual}\n⏱️ *Tiempo total trabajado:* {result}\n\n¡Buen trabajo hoy! 🎉"
                    : $"❌ *No se pudo finalizar la jornada.*\n\n🕒 *Hora actual:* {horaActual}\n🔁 Intenta de nuevo más tarde.\n\n🛠️ Detalle del error: {result}",

                _ => $"⚠️ *Ha ocurrido un error inesperado.*\n\n🕒 *Hora actual:* {horaActual}\n🔧 Por favor, intenta nuevamente o contacta al soporte si el problema persiste."
            };
        }

        /// <summary>
        /// Obtiene los botones interactivos siguientes que se deben mostrar al usuario.
        /// </summary>
        /// <param name="idAccion">Acción actual realizada.</param>
        /// <param name="error">Resultado de la acción.</param>
        /// <returns>Arreglo de enteros que representan los botones siguientes disponibles.</returns>
        private int[] ObtenerBotonesSiguientes(int idAccion, string error)
        {
            bool fallo = isError(error);

            if (fallo)
            {
                return idAccion switch
                {
                    1 => [1, 2, 3],
                    2 => [1, 2, 3],
                    3 => [1, 3, 4],
                    4 => [1, 3, 4],
                    _ => []
                };
            }

            return idAccion switch
            {
                1 => [2, 4],
                2 => [3, 4],
                3 => [2, 4],
                4 => [1],
                _ => []
            };
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
