using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiWhatsapp.DTO
{
    public class TelefonoDTO
    {
        [Required]
        [Column("NUMERO")]
        public required int Numero { get; set; }

        [Required]
        [Column("PREFIJO")]
        public required short Prefijo { get; set; }

        [Required]
        [Column("NOMBRE")]
        public required string Nombre { get; set; }
    }
}
