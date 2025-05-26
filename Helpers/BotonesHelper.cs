using ApiWhatsapp.Controller;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entities;
using AutoMapper;

namespace ApiWhatsapp.Helpers
{
    public class BotonesHelper
    {
        private readonly MensajesController _mensajeController;
        private readonly ControlPresenciaController _controller;

        public BotonesHelper(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper) 
        {
            _mensajeController = new MensajesController(context, contextTerceros, mapper);
            _controller = new ControlPresenciaController();
        }

        public async Task ResponderMensaje(MessageWebhook mensaje)
        {
            int id = GetId(mensaje);

            switch (id)
            {
                case 1:
                    await ProcesarAccion(id, await _controller.IniciarJornada(), mensaje.from);
                    break;
                case 2:
                    await ProcesarAccion(id, await _controller.PausarJornada(), mensaje.from);
                    break;
                case 3:
                    await ProcesarAccion(id, await _controller.ReanudarJornada(), mensaje.from);
                    break;
                case 4:
                    await ProcesarAccion(id, await _controller.FinalizarJornada(), mensaje.from);
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

            await _mensajeController.EnviarMensajeBoton(texto, botonesSiguientes, numero);
        }

        private string ConstruirCuerpo(int id, string error)
        {
            bool fallo = string.IsNullOrWhiteSpace(error);
            return id switch
            {
                1 => fallo ? "Has iniciado la jornada" : $"No se ha podido iniciar la jornada: {error}",
                2 => fallo ? "Has pausado la jornada" : $"No se ha podido pausar la jornada: {error}",
                3 => fallo ? "Has reanudado la jornada" : $"No se ha podido reanudar la jornada: {error}",
                4 => fallo ? "Has finalizado la jornada" : $"No se ha podido finalizar la jornada: {error}",
                _ => "Ha ocurrido un error inesperado"
            };
        }

        private int[] ObtenerBotonesSiguientes(int idAccion, string error)
        {
            bool fallo = !string.IsNullOrWhiteSpace(error);

            // Si hubo fallo, volvemos a enviar el mismo botón
            if (fallo)
            {
                return new[] { idAccion };
            }

            // Si todo salió bien, devolvemos los botones siguientes válidos
            return idAccion switch
            {
                1 => new[] { 2, 4 }, // Después de iniciar: pausar o finalizar
                2 => new[] { 3, 4 }, // Después de pausar: reanudar o finalizar
                3 => new[] { 2, 4 }, // Después de reanudar: pausar o finalizar
                4 => new[] { 1 },    // Después de finalizar: reiniciar la jornada
                _ => Array.Empty<int>()
            };
        }

        private int GetId(MessageWebhook mensaje)
        {
            return int.Parse(mensaje.interactive.button_reply.id);
        }

    }
}
