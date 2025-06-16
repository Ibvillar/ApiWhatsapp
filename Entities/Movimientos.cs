using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiRestDatosComunes.Entities
{
    [Table("MOVIMIENTOS")]
    public class Movimientos
    {
        [Key, Column("IDMOVIMIENTOS")]
        public int Id { get; set; }

        [Column("HORA")]
        public required TimeSpan Hora { get; set; }

        [Column("IDDIAS")]
        public required int IdDias { get; set; }

        [Column("IDTIPOS_MOVIMIENTOS")]
        public required int IdTiposMovimientos { get; set; }
    }
}
