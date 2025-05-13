using ApiWhatsapp.Entitties;

namespace ApiWhatsapp.Helpers
{
    public class MensajesBBDDHelper
    {

        public MensajesBBDDHelper() {}

        public Mensaje ConstruirMensajeTexto(long numeroOrigen, long numeroDestino, string texto)
        {
            Mensaje mensaje = new Mensaje
            {
                IdOrigen = numeroOrigen,
                IdDestino = numeroDestino,
                Texto = texto,
                Fecha = DateTime.Now,
                Leido = false
            };

            return mensaje;
        }

        public Mensaje ConstruirMensajeImagen(long numeroOrigen, long numeroDestino, )
        {

        }
    }
}
