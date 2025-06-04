using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiWhatsapp.DTO
{
    public class TelefonoDTO: TelefonoWithoutNombre
    {
        [Required]
        [Column("NOMBRE")]
        public required string Nombre { get; set; }
    }
}
