using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ApiWhatsapp.Controller
{
    [ApiController]
    [Route("fichero")]
    public class FicherosController : ControllerBase
    {
        private FicheroRepository ficheroRepository;

        /// <summary>
        /// Constructor del controlador FicherosController
        /// </summary>
        /// <param name="context">Contexto de la base de datos</param>
        /// <param name="mapper">Instancia de AutoMapper</param>
        public FicherosController(DbWhatsapp context, IMapper mapper)
        {
            ficheroRepository = new FicheroRepository(context, mapper);
        }

        /// <summary>
        /// Agrega un fichero a la base de datos
        /// </summary>
        /// <param name="ficheroDTO">La ruta del fichero. Se tiene que pasar a través de JSON con la clase FicheroDTO</param>
        /// <returns>Devuelve OK si se guarda correctamente, BadRequest si falla</returns>
        [HttpPost("agregar-fichero")]
        public async Task<ActionResult> AgregarFichero(FicheroDTO ficheroDTO)
        {
            // Normaliza y valida la ruta
            ficheroDTO.Ruta = ValidarRuta(ficheroDTO.Ruta);

            if (ficheroDTO.Ruta == "")
            {
                return BadRequest("La ruta no es valida");
            }

            // Construye la entidad a partir del DTO
            Fichero fichero = ficheroRepository.ConstuirFichero(ficheroDTO);

            // Verifica si ya existe
            if (ficheroRepository.ExisteFichero(fichero))
            {
                return BadRequest("Este fichero ya existe. Utilizar remplazar");
            }

            // Intenta guardar en la base de datos
            if (await ficheroRepository.AddFichero(fichero))
            {
                return Ok();
            }

            return BadRequest("El fichero no se ha guardado correctamente");
        }

        /// <summary>
        /// Obtiene la lista de todos los ficheros almacenados
        /// </summary>
        /// <returns>Lista de objetos Fichero, o BadRequest si no hay ficheros</returns>
        [HttpGet("obtener-ficheros")]
        public async Task<ActionResult> GetFicheros()
        {
            try
            {
                // Obtiene los ficheros desde el repositorio
                List<Fichero> ficheros = ficheroRepository.GetFicheros();

                if (ficheros.IsNullOrEmpty())
                {
                    return BadRequest("No hay ficheros disponibles");
                }

                // Normaliza las rutas para usabilidad
                foreach (var fichero in ficheros)
                {
                    fichero.Ruta = fichero.Ruta.Replace('\\', '/');
                }

                return Ok(ficheros);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtiene un fichero específico a través de su ruta
        /// </summary>
        /// <param name="ruta">Ruta absoluta del fichero</param>
        /// <returns>Devuelve el fichero si existe, NotFound si no, BadRequest si ocurre un error</returns>
        [HttpGet("obtener-fichero-by-ruta/{ruta}")]
        public async Task<ActionResult> GetFicherosByRuta(string ruta)
        {
            try
            {
                Fichero fichero = await ficheroRepository.GetFicheroByRuta(ruta);

                if (fichero is null)
                {
                    return NotFound($"No se ha encontrado ningún fichero en la ruta: {ruta}");
                }

                return Ok(fichero);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtiene un fichero específico a través de su ID
        /// </summary>
        /// <param name="id">Identificador del fichero</param>
        /// <returns>Devuelve el fichero si existe, NotFound si no, BadRequest si ocurre un error</returns>
        [HttpGet("obtener-fichero-by-id/{id}")]
        public ActionResult GetFicheroById(int id)
        {
            try
            {
                Fichero fichero = ficheroRepository.GetFicheroById(id);

                if (fichero is null)
                {
                    return NotFound("Este fichero no existe");
                }

                return Ok(fichero);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Valida y normaliza una ruta de fichero
        /// </summary>
        /// <param name="ruta">Ruta original del fichero</param>
        /// <returns>Ruta completa y válida, o cadena vacía si es inválida</returns>
        private string ValidarRuta(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new ArgumentException("La ruta no puede estar vacía.");

            try
            {
                // Reemplaza | por el separador del sistema
                string rutaNormalizada = ruta.Replace('|', Path.DirectorySeparatorChar);

                // Convierte a ruta absoluta
                string rutaCompleta = Path.GetFullPath(rutaNormalizada);

                // Verifica caracteres inválidos
                char[] caracteresInvalidos = Path.GetInvalidPathChars();
                if (rutaCompleta.Any(c => caracteresInvalidos.Contains(c)))
                    throw new ArgumentException("La ruta contiene caracteres inválidos.");

                return rutaCompleta;
            }
            catch (Exception ex)
            {
                // Log del error
                Console.WriteLine($"Error al validar ruta: {ex.Message}");
                return "";
            }
        }
    }
}
