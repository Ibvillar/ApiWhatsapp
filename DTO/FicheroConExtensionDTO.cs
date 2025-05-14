using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.DTO
{
    public class FicheroConExtensionDTO: FicheroDTO
    {
        [Column("EXTENSION")]
        public string Extension { get; set; }
    }
}
