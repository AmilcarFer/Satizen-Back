using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Sector
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int idSector { get; set; }

        public int idInstitucion { get; set; }
        [ForeignKey("idInstitucion")]
        public virtual Institucion Instituciones { get; set; }

        public string nombreSector { get; set; }
        public string descripcionSector { get; set; }
        public DateTime fechaCreacion { get; set; }
        public DateTime? fechaActualizacion { get; set; }
        public DateTime? fechaEliminacion { get; set; }

    }
}
