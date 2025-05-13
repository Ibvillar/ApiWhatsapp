using ApiWhatsapp.Data;
using ApiWhatsapp.Entitties;

namespace ApiWhatsapp.BBDD
{
    public class FicheroRepository
    {
        private readonly DbWhatsapp context;

        public FicheroRepository(DbWhatsapp context)
        {
            this.context = context;
        }

        /// <summary>
        /// Agrega la ruta de un fichero
        /// </summary>
        /// <param name="fichero">Fichero a agregar</param>
        /// <returns>true si se alamcena correctamente, false en caso contrario</returns>
        public bool AddFichero(Fichero fichero)
        {
            try
            {
                context.Ficheros.Add(fichero);
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
        /// Elimina un fichero por Id
        /// </summary>
        /// <param name="Id">Id del fichero</param>
        /// <returns>true si se ha eliminado correctamente, false en caso contrario</returns>
        public bool RemoveFichero(int Id)
        {
            try
            {
                Fichero fichero = context.Ficheros.FirstOrDefault(x => x.Id == Id);

                if (fichero is null)
                {
                    return false;
                }

                context.Ficheros.Remove(fichero);
                int filasAfectadas = context.SaveChanges();

                if (filasAfectadas == 0)
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
            ;
        }

        /// <summary>
        /// Obtiene la lista de ficheros
        /// </summary>
        /// <returns>Devuelve la lista de ficheros. 
        /// Empty en caso de que no se encuentre ninguno</returns>
        public List<Fichero> GetFicheros()
        {
            List<Fichero> ficheros = [];
            try
            {
                ficheros = context.Ficheros.ToList();

                return ficheros;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Obtiene un fichero por su Id
        /// </summary>
        /// <param name="Id">Id del fichero</param>
        /// <returns>Devuelve el fichero, null en caso de que no exista</returns>
        public Fichero GetFicheroById(int Id)
        {
            List<Fichero> ficheros = GetFicheros();
            if (ficheros is null)
            {
                return null;
            }

            return ficheros.FirstOrDefault(x => x.Id == Id);
        }
    }
}
