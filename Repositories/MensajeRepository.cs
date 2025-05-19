using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;

namespace ApiWhatsapp.BBDD
{
    public class MensajeRepository
    {
        private readonly DbWhatsapp context;

        public MensajeRepository(DbWhatsapp context)
        {
            this.context = context;
        }

        /// <summary>
        /// Agrega un mensaje
        /// </summary>
        /// <param name="mensaje">Mensaje para agregar</param>
        /// <returns>true si lo ha insertado correctamente, false de lo contrario</returns>
        public async Task<bool> AddMensaje(Mensaje mensaje)
        {
            try
            {
                context.Mensajes.Add(mensaje);
                await context.SaveChangesAsync();

                if (GetMensajesById(mensaje.Id) is null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Obtiene una lista de todos los mensajes.
        /// </summary>
        /// <returns>Lista de mensajes. Empty si no se encuentra ninguno</returns>
        public List<Mensaje> GetMensajes()
        {
            List<Mensaje> mensajes = [];
            try
            {
                mensajes = context.Mensajes.ToList();

                return mensajes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        /// <summary>
        /// Obtiene un mensaje por su identificador.
        /// </summary>
        /// <param name="id">El Id único del mensaje.</param>
        /// <returns>El mensaje correspondiente, o null si no se encuentra.</returns>
        public Mensaje GetMensajesById(int Id)
        {
            List<Mensaje> mensajes = GetMensajes();
            if (mensajes is null)
            {
                return null!;
            }

            return mensajes.FirstOrDefault(x => x.Id == Id)!;
        }

        /// <summary>
        /// Obtiene una lista de mensajes de quien los envia.
        /// </summary>
        /// <param name="id">El Id del telefono saliente.</param>
        /// <returns>Lista de mensajes correspondientes. Empty si no se encuentra ninguno.</returns>
        public List<Mensaje> GetMensajesByOrigen(int IdOrigen)
        {
            List<Mensaje> mensajes = GetMensajes();
            if (mensajes is null)
            {
                return null;
            }

            return mensajes.Where(x => x.IdOrigen == IdOrigen).ToList();
            
        }

        /// <summary>
        /// Obtiene una lista de mensajes de quien los recive.
        /// </summary>
        /// <param name="id">El Id del telefono entrante.</param>
        /// <returns>Lista de mensajes correspondientes. Empty si no se encuentra ninguno.</returns>
        public List<Mensaje> GetMensajesByDestino(int IdDestino)
        {
            List<Mensaje> mensajes = GetMensajes();
            if (mensajes is null)
            {
                return null;
            }

            return mensajes.Where(x => x.IdDestino == IdDestino).ToList();
        }

        /// <summary>
        /// Cambia un mensaje a leido
        /// </summary>
        /// <param name="id">Id del mensaje a cambiar</param>
        /// <returns>-1 si hay error, 0 si no existe, 1 si se cambia correctamente</returns>
        public int SetLeido(int id)
        {
            try
            {
                var mensaje = context.Mensajes.Where(x => x.Id == id).FirstOrDefault();

                if (mensaje is null)
                {
                    return 0;
                }

                mensaje.Leido = true;
                int modificado = context.SaveChanges();

                return modificado;
            } catch (Exception e) 
            {
                Console.WriteLine(e.ToString());
                return -1;
            }
        }

        /// <summary>
        /// Construye el objeto Mensaje con formato de texto
        /// </summary>
        /// <returns>Devuelve un objeto a través de los parametros</returns>
        public Mensaje ConstruirMensajeTexto(long numeroOrigen, long numeroDestino, string texto)
        {
            var mensaje = ConstruirMensaje(numeroOrigen, numeroDestino);
            mensaje.Texto = texto;

            return mensaje;
        }

        /// <summary>
        /// Construye el objeto Mensaje con formato de documento o imagen
        /// </summary>
        /// <returns>Devuelve un objeto a través de los parametros</returns>
        public Mensaje ConstruirMensajeArchivo(long numeroOrigen, long numeroDestino, int IdFichero)
        {
            var mensaje = ConstruirMensaje(numeroOrigen, numeroDestino);
            mensaje.IdFichero = IdFichero;

            return mensaje;
        }

        private Mensaje ConstruirMensaje(long numeroOrigen, long numeroDestino)
        {
            Mensaje mensaje = new Mensaje
            {
                IdOrigen = numeroOrigen,
                IdDestino = numeroDestino,
                Fecha = DateTime.Now,
                Leido = false,
                Texto = "",
                IdFichero = -1
            };

            return mensaje;
        }
    }
}
