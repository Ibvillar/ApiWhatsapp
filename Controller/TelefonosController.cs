using ApiWhatsapp.BBDD;
using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiWhatsapp.Controller
{
    [ApiController]
    [Route("telefono")]
    public class TelefonosController: ControllerBase
    {
        private DbWhatsapp context;
        private TelefonoRepository telefonoRepository;

        public TelefonosController(DbWhatsapp context, IMapper mapper)
        {
            this.context = context;
            telefonoRepository = new TelefonoRepository(context, mapper);
        }

        [HttpPost("agregar-telefono")]
        public async Task<ActionResult> CrearTeleofno(TelefonoDTO telefonoDTO)
        {
            try
            {
                Telefono telefono = telefonoRepository.ConstruirTelefono(
                                        telefonoDTO.Numero, telefonoDTO.Prefijo, telefonoDTO.Nombre);
                await telefonoRepository.AddTelefono(telefono);

                return Ok("Telefono creado correctamente");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
