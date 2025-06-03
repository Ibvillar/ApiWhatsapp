using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.Entitties
{
    public class Telefono
    {
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("NUMERO")]
        public required int Numero { get; set; }

        [Required]
        [Column("PREFIJO")]
        public required short Prefijo { get; set; }

        [Required]
        [Column("NOMBRE")]
        public required string Nombre { get; set; }

        [Required]
        [Column("ID_TERCEROS")]
        public int IdTerceros { get; set; }

        [Required]
        [Column("ID_GENERALES")]
        public string IdGenerales {  get; set; }

        [Required]
        [Column("TOKEN")]
        public string Token { get; set; }

        [Required]
        [Column("UBICACION")]
        public bool ubicacion {  get; set; }
    }
}
