using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using ApiWhatsapp.Utilidades;
using AutoMapper;

namespace ApiWhatsapp.BBDD
{
    /// <summary>
    /// Repositorio para la gestión de entidades de tipo Teléfono.
    /// </summary>
    public class TelefonoRepository
    {
        private readonly DbWhatsapp context;
        private readonly IMapper mapper;

        /// <summary>
        /// Constructor del repositorio de teléfonos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos</param>
        /// <param name="mapper">Instancia de AutoMapper</param>
        public TelefonoRepository(DbWhatsapp context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        /// <summary>
        /// Agrega un teléfono a la base de datos.
        /// </summary>
        /// <param name="telefono">Teléfono a agregar</param>
        /// <returns>True si se ha insertado correctamente, False en caso contrario</returns>
        public async Task<bool> AddTelefono(Telefono telefono)
        {
            try
            {
                context.Telefonos.Add(telefono);
                await context.SaveChangesAsync();

                if (GetTelefonosById(telefono.Id) is null)
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
        /// Obtiene una lista de todos los teléfonos.
        /// </summary>
        /// <returns>Lista de teléfonos. Lista vacía si no hay ninguno</returns>
        public List<Telefono> GetTelefonos()
        {
            List<Telefono> telefonos = [];
            try
            {
                telefonos = context.Telefonos.ToList();
                return telefonos;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        /// <summary>
        /// Obtiene un teléfono a partir de su ID.
        /// </summary>
        /// <param name="Id">ID del teléfono</param>
        /// <returns>Objeto Teléfono si se encuentra, null en caso contrario</returns>
        public Telefono GetTelefonosById(long Id)
        {
            List<Telefono> telefonos = GetTelefonos();
            if (telefonos is null)
            {
                return null!;
            }

            return telefonos.FirstOrDefault(x => x.Id == Id)!;
        }

        /// <summary>
        /// Construye un objeto Teléfono a partir de sus atributos.
        /// </summary>
        /// <param name="numero">Número del teléfono</param>
        /// <param name="prefijo">Prefijo del país</param>
        /// <param name="nombre">Nombre asociado al teléfono</param>
        /// <returns>Objeto Teléfono</returns>
        public Telefono ConstruirTelefono(int numero, short prefijo, string nombre)
        {
            TelefonoDTO telefono = new TelefonoDTO
            {
                Numero = numero,
                Prefijo = prefijo,
                Nombre = nombre
            };

            return mapper.Map<Telefono>(telefono);
        }

        /// <summary>
        /// Elimina un teléfono de la base de datos por su ID.
        /// </summary>
        /// <param name="telefonoId">ID del teléfono a eliminar</param>
        /// <returns>True si fue eliminado, False si no se encontró</returns>
        public async Task<bool> RemoveTelefono(long telefonoId)
        {
            var telefono = context.Telefonos.FirstOrDefault(x => x.Id == telefonoId);

            if (telefono != null)
            {
                context.Telefonos.Remove(telefono);
                await context.SaveChangesAsync();
                return true;
            }
            else
            {
                Console.WriteLine("Teléfono no encontrado.");
                return false;
            }
        }
    }
}
