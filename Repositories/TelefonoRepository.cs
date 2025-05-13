using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;

namespace ApiWhatsapp.BBDD
{
    public class TelefonoRepository
    {
        private readonly DbWhatsapp context;

        public TelefonoRepository(DbWhatsapp context)
        {
            this.context = context;
        }

        /// <summary>
        /// Agrega un telefono
        /// </summary>
        /// <param name="telefono">Telefono a agregar</param>
        /// <returns>true si se ha insertado correctamente, false de lo contrario</returns>
        public bool AddTelefono(Telefono telefono)
        {
            try
            {
                context.Telefonos.Add(telefono);
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
                return null;
            }

            return telefonos.FirstOrDefault(x => x.Id == Id);
        }

        /// <summary>
        /// Construye un objeto de Telefono
        /// </summary>
        /// <returns>Devuelve el objeto de Telefono</returns>
        public Telefono ConstruirTelefono(long numero, string nombre)
        {
            Telefono telefono = new Telefono
            {
                Id = numero,
                Nombre = nombre
            };

            return telefono;
        }
    }
}
