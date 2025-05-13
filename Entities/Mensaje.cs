using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.Entitties
{
    public class Mensaje
    {
        [Key]
        [Column("ID")]
        public int Id  { get; set; }

        [Required]
        [Column("ID_ORIGEN")]
        public required long IdOrigen { get; set; }

        [Required]
        [Column("ID_DESTINO")]
        public required long IdDestino { get; set; }

        [Column("TEXTO")]
        public string Texto { get; set; }

        [Required]
        [Column("FECHA")]
        public required DateTime Fecha { get; set; }

        [Column("ID_FICHERO")]
        public int IdFichero { get; set; }

        [Column("LEIDO")]
        public bool Leido { get; set; }
    }
}
