using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.Entities
{
    [Table("LOCALIZACION")]
    public class Localizacion
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("LONGITUD")]
        public double Longitud {  get; set; }

        [Required]
        [Column("LATITUD")]
        public double Latitud { get; set; }

        [Column("DIA")]
        public DateOnly Dia {  get; set; }

        [Required]
        [Column("ID_TELEFONO")]
        public long IdTelefono { get; set; }
    }
}