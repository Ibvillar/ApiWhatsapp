using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.Entitties
{
    public class Fichero
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("RUTA")]
        public required string Ruta { get; set; }

        [Required]
        [Column("EXTENSION")]
        public required string Extension { get; set; }
    }
}
