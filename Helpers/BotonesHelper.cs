using ApiWhatsapp.Controller;
using ApiWhatsapp.Data;
using AutoMapper;

namespace ApiWhatsapp.Helpers
{
    public class BotonesHelper
    {
        private readonly MensajesController _mensajeController;

        public BotonesHelper(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper) 
        {
            _mensajeController = new MensajesController(context, contextTerceros, mapper);
        }

        public async Task ResponderMensaje()
        {
            await _mensajeController.EnviarMensajeBoton();
        }
    }
}
