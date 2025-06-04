using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiWhatsapp.DTO
{
    public class TelefonoWithoutNombre
    {
        [Required]
        [Column("NUMERO")]
        public required int Numero { get; set; }

        [Required]
        [Column("PREFIJO")]
        public required short Prefijo { get; set; }
    }
}
