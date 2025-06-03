using ApiWhatsapp.BBDD;
using ApiWhatsapp.Controller;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entities;
using ApiWhatsapp.Entitties;
using ApiWhatsapp.Repositories;
using AutoMapper;

namespace ApiWhatsapp.Helpers
{
    public class BotonesHelper
    {
        private readonly MensajesController _mensajeController;
        private readonly ControlPresenciaController _controller;
        private readonly TelefonoRepository _telefonosRepository;
        private readonly LocalizacionRepository _localizacionRepository;

        public BotonesHelper(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper, IConfiguration _configuracion) 
        {
            _mensajeController = new MensajesController(context, contextTerceros, mapper, _configuracion);
            _controller = new ControlPresenciaController(_configuracion, context);
            _telefonosRepository = new TelefonoRepository(context, contextTerceros, mapper);
            _localizacionRepository = new LocalizacionRepository(context);
        }

        public async Task ResponderMensaje(MessageWebhook mensaje)
        {
            int id = GetId(mensaje);

            var codUsuario = await GetCodFromNumber(mensaje.from);

            bool tieneUbicacion = await _localizacionRepository.UsuarioTieneLocalizacionValida(long.Parse(mensaje.from));

            if (!tieneUbicacion)
            {
                await _mensajeController.EnviarMensajeTexto(long.Parse(mensaje.from), "📍 Por favor, comparte tu ubicación antes de registrar la jornada.");
                return;
            }

            switch (id)
            {
                case 1:
                    await ProcesarAccion(id, await _controller.IniciarJornada(await GetCodFromNumber(mensaje.from)), mensaje.from);
                    break;
                case 2:
                    await ProcesarAccion(id, await _controller.PausarJornada(await GetCodFromNumber(mensaje.from)), mensaje.from);
                    break;
                case 3:
                    await ProcesarAccion(id, await _controller.ReaunudarJornada(await GetCodFromNumber(mensaje.from)), mensaje.from);
                    break;
                case 4:
                    await ProcesarAccion(id, await _controller.FinalizarJornada(await GetCodFromNumber(mensaje.from)), mensaje.from);
                    break;
                default:
                    break;
            }
        }

        private async Task ProcesarAccion(int idAccion, string error, string numero)
        {
            var texto = ConstruirCuerpo(idAccion, error);

            // Determina los siguientes botones a mostrar según resultado
            var botonesSiguientes = ObtenerBotonesSiguientes(idAccion, error);

            await _mensajeController.EnviarMensajeBoton(texto, numero, botonesSiguientes);
        }

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


        private int[] ObtenerBotonesSiguientes(int idAccion, string error)
        {
            bool fallo = isError(error);

            // Si hubo fallo, volvemos a enviar el mismo botón
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

            // Si todo salió bien, devolvemos los botones siguientes válidos
            return idAccion switch
            {
                1 => [2, 4], // Después de iniciar: pausar o finalizar
                2 => [3, 4], // Después de pausar: reanudar o finalizar
                3 => [2, 4], // Después de reanudar: pausar o finalizar
                4 => [1],    // Después de finalizar: reiniciar la jornada
                _ => []
            };
        }

        private int GetId(MessageWebhook mensaje)
        {
            return int.Parse(mensaje.interactive.button_reply.id);
        }

        private bool isError(string result)
        {
            try
            {
                Console.WriteLine(result);
                int.Parse(result.Substring(0, 2));

                return false;
            } catch
            {
                return true;
            }
        }

        private async Task<string> GetCodFromNumber(string numero)
        {
            long longNumber = long.Parse(numero);

            Telefono telefono = await _telefonosRepository.GetTelefonosById(longNumber);

            return telefono.IdGenerales;
        }

    }
}
