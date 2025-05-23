using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.Entities
{
    [Table("BOTON")]
    public class Boton
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Texto{ get; set; }
    }
}
