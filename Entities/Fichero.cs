using System.ComponentModel.DataAnnotations;

namespace ApiWhatsapp.Entitties
{
    public class Fichero
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Ruta { get; set; }

        [Required]
        public required string Extension { get; set; }
    }
}
