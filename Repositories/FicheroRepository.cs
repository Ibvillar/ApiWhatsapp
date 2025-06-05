using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.BBDD
{
    public class FicheroRepository
    {
        private readonly DbWhatsapp context;
        private readonly IMapper mapper;

        public FicheroRepository(DbWhatsapp context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        /// <summary>
        /// Agrega la ruta de un fichero
        /// </summary>
        /// <param name="fichero">Fichero a agregar</param>
        /// <returns>true si se alamcena correctamente, false en caso contrario</returns>
        public async Task<bool> AddFichero(Fichero fichero)
        {
            try
            {
                await context.Ficheros.AddAsync(fichero);
                await context.SaveChangesAsync();

                if (GetFicheroById(fichero.Id) is null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
                return null!;
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
                return null!;
            }

            return ficheros.FirstOrDefault(x => x.Id == Id)!;
        }

        /// <summary>
        /// Construye un objeto Fichero a traves de la ruta
        /// </summary>
        /// <param name="ruta">Ruta del fichero a construir</param>
        /// <returns>Devuelve un objeto del fichero en la ruta indicada</returns>
        public Fichero ConstuirFichero(FicheroDTO ficheroDTO)
        {
            FicheroConExtensionDTO ficheroConExtension = mapper.Map<FicheroConExtensionDTO>(ficheroDTO);
            ficheroConExtension.Extension = GetExtension(ficheroConExtension.Ruta);

            return mapper.Map<Fichero>(ficheroConExtension);
        }

        /// <summary>
        /// Comprobar si existe el fichero indicado
        /// </summary>
        /// <returns>true en caso de que exista, false en caso contrario</returns>
        public async Task<bool> ExisteFichero(Fichero fichero)
        {
            if (fichero is null)
            {
                return false;
            }

            Fichero? fichero1  = await context.Ficheros.FirstOrDefaultAsync(x => x.Ruta == fichero.Ruta);

            if (fichero1 is null)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Obtener el fichero a traves de la ruta con la extension incluida dentro de esta
        /// </summary>
        /// <returns>Devuelve el fichero, null en caso de que no exista</returns>
        public async Task<Fichero> GetFicheroByRuta(string ruta)
        {
            string extension = GetExtension(ruta);

            Fichero? fichero1 = await context.Ficheros.Where(x => x.Ruta == ruta
                                   && x.Extension.Equals(extension)).FirstOrDefaultAsync();

            return fichero1!;
        }

        /// <summary>
        /// Obtener la extension del fichero
        /// </summary>
        /// <param name="ruta">ruta absoluta del archivo sin extension</param>
        /// <returns>devuelve la extension del archivo</returns>
        public string GetExtension(string ruta)
        {
            return Path.GetExtension(ruta);
        }

        /// <summary>
        /// Obtiene el MIME ype del archivo
        /// </summary>
        /// <param name="ruta">ruta absoluta del archivo</param>
        /// <returns>Un string con el valor del MIME type</returns>
        public string GetMIME(string ruta)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(ruta, out string? extension))
            {
                extension = "application/octet-stream";
            }

            return extension;
        }
    }
}
