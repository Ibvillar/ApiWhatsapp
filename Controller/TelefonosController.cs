using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ApiWhatsapp.Controller
{
    /// <summary>
    /// Controlador para operaciones relacionadas con teléfonos.
    /// </summary>
    [ApiController]
    [Route("telefono")]
    public class TelefonosController : ControllerBase
    {
        private TelefonoRepository telefonoRepository;

        public TelefonosController(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper)
        {
            telefonoRepository = new TelefonoRepository(context, contextTerceros, mapper);
        }

        /// <summary>
        /// Agrega un nuevo teléfono a la base de datos.
        /// </summary>
        /// <param name="telefonoDTO">Objeto DTO con los datos del teléfono.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("agregar-telefono")]
        public async Task<ActionResult> CrearTeleofno(TelefonoDTO telefonoDTO)
        {
            try
            {
                string mensaje = ValidarTelefono(telefonoDTO);
                if (mensaje is not null)
                {
                    return BadRequest(mensaje);
                }

                Telefono telefono = telefonoRepository.ConstruirTelefono(
                                        telefonoDTO.Numero, telefonoDTO.Prefijo, telefonoDTO.Nombre);

                if (telefonoRepository.GetTelefonosById(telefono.Id) is not null)
                {
                    return BadRequest("Este teléfono ya existe");
                }

                await telefonoRepository.AddTelefono(telefono);

                return Ok("Teléfono creado correctamente");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de teléfonos registrados.
        /// </summary>
        /// <returns>Lista de teléfonos o un mensaje de error.</returns>
        [HttpGet("obtener-telefonos")]
        public async Task<ActionResult> GetTelefonos()
        {
            try
            {
                List<Telefono> telefonos = await telefonoRepository.GetTelefonos();

                if (telefonos.IsNullOrEmpty())
                {
                    return BadRequest("No hay teléfonos disponibles");
                }

                return Ok(telefonos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtiene un teléfono específico por su ID.
        /// </summary>
        /// <param name="telefonoId">ID del teléfono</param>
        /// <returns>Objeto teléfono o mensaje de error.</returns>
        [HttpGet("obtener-telefono/{telefonoId}")]
        public async Task<ActionResult> GetTelefonoById(long telefonoId)
        {
            try
            {
                Telefono telefono = await telefonoRepository.GetTelefonosById(telefonoId);

                if (telefono is null)
                {
                    return NotFound("Este teléfono no existe");
                }

                return Ok(telefono);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Permite actualizar el nombre asociado a un número de teléfono.
        /// </summary>
        /// <param name="telefono">Objeto que contiene el número de teléfono y el nuevo nombre a asignar.</param>
        /// <returns>
        /// Retorna un resultado HTTP 200 (OK) con el objeto enviado si la operación fue exitosa,
        /// o un HTTP 400 (Bad Request) si falló o se produjo una excepción.
        /// </returns>
        [HttpPut("set-name")]
        public async Task<ActionResult> SetName(TelefonoNombre telefono) 
        {
            try
            {
                bool result = await telefonoRepository.SetNombre(telefono.telfono, telefono.nombre);

                if (result)
                {
                    return Ok(telefono);
                }

                return BadRequest("El nombre no se ha podido cambiar");
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        /// <summary>
        /// Elimina un teléfono por su ID.
        /// </summary>
        /// <param name="telefonoId">ID del teléfono a eliminar</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("borrar-telefono/{telefonoId}")]
        public async Task<ActionResult> RemoveTelefono(long telefonoId)
        {
            try
            {
                bool exito = await telefonoRepository.RemoveTelefono(telefonoId);

                if (exito)
                {
                    return Ok("Se ha eliminado correctamente");
                }

                return NotFound("Este teléfono no existe");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Valida los campos del DTO de teléfono.
        /// </summary>
        /// <param name="telefono">Objeto DTO del teléfono</param>
        /// <returns>Mensaje de error si hay errores, null si es válido</returns>
        private string ValidarTelefono(TelefonoDTO telefono)
        {
            // Validar prefijo (ej: códigos de país suelen estar entre 1 y 999)
            if (telefono.Prefijo <= 0 || telefono.Prefijo > 999)
            {
                return "El prefijo debe estar entre 1 y 999.";
            }

            // Validar número (debe ser mayor a 0)
            if (telefono.Numero <= 0)
            {
                return "El número de teléfono debe ser mayor que cero.";
            }

            // Validar longitud del número (por ejemplo, entre 6 y 9 dígitos)
            int longitudNumero = telefono.Numero.ToString().Length;
            if (longitudNumero < 6 || longitudNumero > 9)
            {
                return "El número de teléfono debe tener entre 6 y 9 dígitos.";
            }

            return null;
        }
    }
}
