using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.Entities
{
    public class Token
    {
        [Column("TOKEN")]
        public string token {  get; set; }

        [Column("EXPIRATION")]
        public DateTime expiracion { get; set; }

        [Column("ID_TELEFONO")]
        public long IdTelefono { get; set; }
    }
}
