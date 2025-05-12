using System.ComponentModel.DataAnnotations;

namespace ApiWhatsapp.Entitties
{
    public class Telefono
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required int Prefijo { get; set; }

        [Required]
        public required int Numero { get; set; }

        [Required]
        public required string Nombre { get; set; }
    }
}
