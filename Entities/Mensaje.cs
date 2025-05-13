using System.ComponentModel.DataAnnotations;

namespace ApiWhatsapp.Entitties
{
    public class Mensaje
    {
        [Key]
        public int Id  { get; set; }

        [Required]
        public required long IdOrigen { get; set; }

        [Required]
        public required long IdDestino { get; set; }

        public string Texto { get; set; }

        [Required]
        public required DateTime Fecha { get; set; }

        public int IdFichero { get; set; }

        public bool Leido { get; set; }
    }
}
