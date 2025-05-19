using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using ApiWhatsapp.Utilidades;
using AutoMapper;

namespace ApiWhatsapp.BBDD
{
    public class TelefonoRepository
    {
        private readonly DbWhatsapp context;
        private readonly IMapper mapper;

        public TelefonoRepository(DbWhatsapp context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        /// <summary>
        /// Agrega un telefono a la base de datos
        /// </summary>
        /// <param name="telefono">Telefono a agregar</param>
        /// <returns>true si se ha insertado correctamente, false de lo contrario</returns>
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
        /// Obtiene una lista de telefonos
        /// </summary>
        /// <returns>Devuelve una lista de telefonos. Empty si no hay ninguno</returns>
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
                return null;
            }
        }

        /// <summary>
        /// Obtiene un telefono a traves de su id
        /// </summary>
        /// <param name="Id">El Id del telefono</param>
        /// <returns>Devuelve el telefono, o null si no se ha encontrado</returns>
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
        /// Construye un objeto de Telefono
        /// </summary>
        /// <returns>Devuelve el objeto de Telefono</returns>
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
