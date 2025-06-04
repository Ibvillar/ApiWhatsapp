using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace ApiWhatsapp.BBDD
{
    /// <summary>
    /// Repositorio para operaciones relacionadas con los mensajes.
    /// </summary>
    public class MensajeRepository
    {
        private readonly DbWhatsapp context;

        public MensajeRepository(DbWhatsapp context)
        {
            this.context = context;
        }

        /// <summary>
        /// Agrega un mensaje a la base de datos.
        /// </summary>
        /// <param name="mensaje">Mensaje a insertar</param>
        /// <returns>True si fue insertado correctamente, false si no</returns>
        public async Task<bool> AddMensaje(Mensaje mensaje)
        {
            try
            {
                context.Mensajes.Add(mensaje);
                await context.SaveChangesAsync();

                if (await GetMensajesById(mensaje.Id) is null)
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
        /// Obtiene todos los mensajes registrados.
        /// </summary>
        /// <returns>Lista de objetos Mensaje, o null si ocurre un error</returns>
        public async Task<List<Mensaje>> GetMensajes()
        {
            try
            {
                return await context.Mensajes.OrderBy(x => x.Fecha).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        /// <summary>
        /// Obtiene un mensaje por su ID.
        /// </summary>
        /// <param name="Id">ID del mensaje</param>
        /// <returns>Mensaje encontrado o null si no existe</returns>
        public async Task<Mensaje> GetMensajesById(int Id)
        {
            List<Mensaje> mensajes = await GetMensajes();
            if (mensajes is null)
            {
                return null!;
            }

            return mensajes.OrderBy(x => x.Fecha).FirstOrDefault(x => x.Id == Id)!;
        }

        /// <summary>
        /// Obtiene los mensajes enviados por un número de origen específico.
        /// </summary>
        /// <param name="IdOrigen">ID del número de origen</param>
        /// <returns>Lista de mensajes enviados por el origen</returns>
        public async Task<List<Mensaje>> GetMensajesByOrigen(long IdOrigen)
        {
            List<Mensaje> mensajes = await GetMensajes();
            if (mensajes is null)
            {
                return null!;
            }

            return mensajes.Where(x => x.IdOrigen == IdOrigen).OrderBy(x => x.Fecha).ToList();
        }

        /// <summary>
        /// Obtiene los mensajes recibidos por un número de destino específico.
        /// </summary>
        /// <param name="IdDestino">ID del número de destino</param>
        /// <returns>Lista de mensajes recibidos por el destino</returns>
        public async Task<List<Mensaje>> GetMensajesByDestino(long IdDestino)
        {
            List<Mensaje> mensajes = await GetMensajes();
            if (mensajes is null)
            {
                return null!;
            }

            return mensajes.Where(x => x.IdDestino == IdDestino).OrderBy(x => x.Fecha).ToList();
        }

        /// <summary>
        /// Marca un mensaje como leído.
        /// </summary>
        /// <param name="id">ID del mensaje</param>
        /// <returns>1 si se modificó correctamente, 0 si no se encontró</returns>
        public async Task<int> SetLeido(int id)
        {
            try
            {
                var mensaje = context.Mensajes.FirstOrDefault(x => x.Id == id);

                if (mensaje is null)
                {
                    return 0;
                }

                mensaje.Leido = true;
                int modificado = await context.SaveChangesAsync();

                return modificado;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Elimina un mensaje por su ID.
        /// </summary>
        /// <param name="mensajeId">ID del mensaje a eliminar</param>
        /// <returns>1 si se eliminó correctamente, -1 si no se encontró</returns>
        public async Task<int> RemoveMensaje(int mensajeId)
        {
            try
            {
                Mensaje? mensaje = await context.Mensajes.FirstOrDefaultAsync(x => x.Id == mensajeId);

                if (mensaje is null)
                {
                    return -1;
                }

                context.Mensajes.Remove(mensaje);
                await context.SaveChangesAsync();

                return 1;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Construye un mensaje de tipo texto.
        /// </summary>
        /// <param name="numeroOrigen">Número del remitente</param>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="texto">Contenido del mensaje</param>
        /// <returns>Objeto Mensaje con texto</returns>
        public Mensaje ConstruirMensajeTexto(long numeroOrigen, long numeroDestino, string texto)
        {
            var mensaje = ConstruirMensaje(numeroOrigen, numeroDestino);
            mensaje.Texto = texto;

            return mensaje;
        }

        /// <summary>
        /// Construye un mensaje con archivo (documento o imagen).
        /// </summary>
        /// <param name="numeroOrigen">Número del remitente</param>
        /// <param name="numeroDestino">Número del destinatario</param>
        /// <param name="IdFichero">ID del fichero asociado</param>
        /// <returns>Objeto Mensaje con archivo</returns>
        public Mensaje ConstruirMensajeArchivo(long numeroOrigen, long numeroDestino, int IdFichero)
        {
            var mensaje = ConstruirMensaje(numeroOrigen, numeroDestino);
            mensaje.IdFichero = IdFichero;

            return mensaje;
        }

        /// <summary>
        /// Construye un mensaje con un botón guardado asociado.
        /// </summary>
        /// <param name="numeroOrigen">Número de teléfono del remitente.</param>
        /// <param name="numeroDestino">Número de teléfono del destinatario.</param>
        /// <param name="idBoton">ID del botón asociado al mensaje.</param>
        /// <returns>Un objeto Mensaje que contiene el botón asignado.</returns>
        public Mensaje ConstruirMensajeBotonGuardado(long numeroOrigen, long numeroDestino, int idBoton)
        {
            var mensaje = ConstruirMensaje(numeroOrigen, numeroDestino);
            mensaje.Boton = idBoton;

            return mensaje;
        }

        /// <summary>
        /// Construye un mensaje interactivo con un botón de tipo "reply" para ser enviado a través de la API de WhatsApp.
        /// </summary>
        /// <param name="numero">Número de teléfono del destinatario en formato internacional.</param>
        /// <param name="mensaje">Texto del cuerpo del mensaje que se mostrará al usuario.</param>
        /// <param name="idBoton">Identificador único del botón de respuesta (usado para identificar qué botón fue pulsado).</param>
        /// <param name="tituloBoton">Texto visible que se mostrará en el botón.</param>
        /// <returns>Objeto <see cref="MensajeBotonReply"/> listo para ser serializado y enviado a la API de WhatsApp.</returns>
        public MensajeBotonReply ContruirMensajeBoton(string numero, string mensaje, ButtonReply[] botones)
        {
            return new MensajeBotonReply
            {
                MessagingProduct = "whatsapp",
                TelefonoDestino = numero,
                Tipo = "interactive",
                interactive = new InteractiveReply
                {
                    type = "button",
                    body = new BodyReply
                    {
                        text = mensaje
                    },
                    action = new ActionReply
                    {
                        buttons = botones
                    }
                }
            };
        }


        /// <summary>
        /// Método base para construir un objeto Mensaje con datos comunes.
        /// </summary>
        /// <param name="numeroOrigen">Número de origen</param>
        /// <param name="numeroDestino">Número de destino</param>
        /// <returns>Objeto Mensaje inicializado</returns>
        private Mensaje ConstruirMensaje(long numeroOrigen, long numeroDestino)
        {
            return new Mensaje
            {
                IdOrigen = numeroOrigen,
                IdDestino = numeroDestino,
                Fecha = DateTime.Now,
                Leido = false,
                Texto = "",
                IdFichero = -1,
                Boton = -1
            };
        }
    }
}
