using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiWhatsapp.DTO
{
    public class FicheroDTO
    {
        [Required]
        [Column("RUTA")]
        public required string Ruta { get; set; }
    }
}
