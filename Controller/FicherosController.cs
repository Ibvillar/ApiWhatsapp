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
    public class FicherosController: ControllerBase
    {
        private FicheroRepository ficheroRepository;

        public FicherosController(DbWhatsapp context, IMapper mapper)
        {
            ficheroRepository = new FicheroRepository(context, mapper);
        }

        [HttpPost("agregar-fichero")]
        public async Task<ActionResult> AgregarFichero(FicheroDTO ficheroDTO)
        {
            ficheroDTO.Ruta = ValidarRuta(ficheroDTO.Ruta);

            if (ficheroDTO.Ruta == "")
            {
                return BadRequest("La ruta no es valida");
            }

            Fichero fichero = ficheroRepository.ConstuirFichero(ficheroDTO);

            if (ficheroRepository.ExisteFichero(fichero))
            {
                return BadRequest("Este fichero ya existe. Utilizar remplazar");
            }

            if (await ficheroRepository.AddFichero(fichero))
            {
                return Ok();
            }

            return BadRequest("El fichero no se ha guardado correctamente");
        }

        [HttpGet("obtener-ficheros")]
        public async Task<ActionResult> GetFicheros()
        {
            try
            {
                List<Fichero> ficheros = ficheroRepository.GetFicheros();

                if (ficheros.IsNullOrEmpty())
                {
                    return BadRequest("No hay ficheros dispnibles");
                }

                foreach(var fichero in ficheros)
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

        [HttpGet("obtener-fichero-by-ruta/{ruta}")]
        public async Task<ActionResult> GetFicherosByRuta(string ruta)
        {
            try
            {
                Fichero fichero = await ficheroRepository.GetFicheroByRuta(ruta);

                if (fichero is null)
                {
                    return NotFound($"No se ha encontrado ningun fichero en la ruta: {ruta}");
                }

                return Ok(fichero);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

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
                // Opcional: log o control de error
                Console.WriteLine($"Error al validar ruta: {ex.Message}");
                return "";
            }
        }

    }
}
