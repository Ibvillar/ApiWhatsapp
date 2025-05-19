using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
        public async Task<List<Mensaje>> GetMensajes()
        {
            List<Mensaje> mensajes = [];
            try
            {
                mensajes = await context.Mensajes.ToListAsync();

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
        public async Task<Mensaje> GetMensajesById(int Id)
        {
            List<Mensaje> mensajes = await GetMensajes();
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
        public async Task<List<Mensaje>> GetMensajesByOrigen(long IdOrigen)
        {
            List<Mensaje> mensajes = await GetMensajes();
            if (mensajes is null)
            {
                return null;
            }

            mensajes = mensajes.Where(x => x.IdOrigen == IdOrigen).ToList();
            return mensajes;
            
        }

        /// <summary>
        /// Obtiene una lista de mensajes de quien los recive.
        /// </summary>
        /// <param name="id">El Id del telefono entrante.</param>
        /// <returns>Lista de mensajes correspondientes. Empty si no se encuentra ninguno.</returns>
        public async Task<List<Mensaje>> GetMensajesByDestino(long IdDestino)
        {
            List<Mensaje> mensajes = await GetMensajes();
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
        /// <returns>1 si lo cambia correctamente, 0 si no existe</returns>
        public async Task<int> SetLeido(int id)
        {
            try
            {
                var mensaje = context.Mensajes.Where(x => x.Id == id).FirstOrDefault();

                if (mensaje is null)
                {
                    return 0;
                }

                mensaje.Leido = true;
                int modificado = await context.SaveChangesAsync();

                return modificado;
            } catch (Exception e) 
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> RemoveMensaje(int mensajeId)
        {
            try
            {
                Mensaje mensaje = await context.Mensajes.FirstOrDefaultAsync(x => x.Id == mensajeId);

                if (mensaje is null)
                {
                    return -1;
                }

                context.Mensajes.Remove(mensaje);
                int result = await context.SaveChangesAsync();

                return 1;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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
