using System.ComponentModel.DataAnnotations.Schema;

namespace ApiWhatsapp.Entities
{
    public class Terceros
    {
        [Column("APE_002")]
        public string nombre {  get; set; }

        [Column("PAI_002")]
        public string pais {  get; set; }

        [Column("TF1_002")]
        public int numero1 { get; set; }

        [Column("TF2_002")]
        public int numero2 { get; set; }
    }
}
