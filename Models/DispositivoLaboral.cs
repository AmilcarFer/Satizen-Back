using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class DispositivoLaboral
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDispositivo {  get; set; }

        public int idPersonal { get; set; }
        [ForeignKey("idPersonal")]
        public virtual Personal Personals { get; set; }

        public string numDispositivo { get; set; }
        public string? observacionDispositivo { get; set; }
        public string estadoDispositivo { get; set; }
        public DateTime fechaCreacion { get; set; }
        public DateTime? fechaActualizacion { get; set; }

        public DateTime? fechaEliminacion { get; set; }
    }
}
