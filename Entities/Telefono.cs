using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.Entitties
{
    public class Telefono
    {
        [Key]
        [Required]
        [Column("ID")]
        public required long Id { get; set; }

        [Required]
        [Column("NOMBRE")]
        public required string Nombre { get; set; }
    }
}
