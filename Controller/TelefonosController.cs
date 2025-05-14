using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiWhatsapp.Controller
{
    [ApiController]
    [Route("telefono")]
    public class TelefonosController: ControllerBase
    {
        private TelefonoRepository telefonoRepository;

        public TelefonosController(DbWhatsapp context, IMapper mapper)
        {
            telefonoRepository = new TelefonoRepository(context, mapper);
        }

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
                    return BadRequest("Este telefono ya existe");
                }

                await telefonoRepository.AddTelefono(telefono);

                return Ok("Telefono creado correctamente");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("obtener-telefonos")]
        public async Task<ActionResult> GetTelefonos()
        {
            try
            {
                List<Telefono> telefonos = telefonoRepository.GetTelefonos();

                if (telefonos.IsNullOrEmpty())
                {
                    return BadRequest("No hay telefonos dispnibles");
                }

                return Ok(telefonos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private string ValidarTelefono(TelefonoDTO telefono)
        {
            string mensajeError = string.Empty;

            // Validar prefijo (ej: códigos de país suelen estar entre 1 y 999)
            if (telefono.Prefijo <= 0 || telefono.Prefijo > 999)
            {
                return "El prefijo debe estar entre 1 y 999.";
            }

            // Validar número (puede variar, aquí usamos un rango típico de longitud)
            if (telefono.Numero <= 0)
            {
                return "El número de teléfono debe ser mayor que cero.";
            }

            // Validar longitud del número (por ejemplo, entre 6 y 10 dígitos)
            int longitudNumero = telefono.Numero.ToString().Length;
            if (longitudNumero < 6 || longitudNumero > 9)
            {
                return "El número de teléfono debe tener entre 6 y 9 dígitos.";
            }

            return null;
        }
    }
}
