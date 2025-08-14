using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models
{
    public class Institucion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idInstitucion { get; set; }
        public string? nombreInstitucion { get; set; }
        public string? direccionInstitucion { get; set; }
        public string? telefonoInstitucion { get; set; }
        public string? correoInstitucion { get; set; }
        public string? celularInstitucion { get; set; }
        public DateTime? eliminado { get; set; }
    }
   
}

