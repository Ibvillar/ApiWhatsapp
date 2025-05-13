using System.Threading.Tasks;
using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;
using Microsoft.IdentityModel.Tokens;

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
        public bool AddMensaje(Mensaje mensaje)
        {
            try
            {
                context.Mensajes.Add(mensaje);
                context.SaveChangesAsync();

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
    }
}
