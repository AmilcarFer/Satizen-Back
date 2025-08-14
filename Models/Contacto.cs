
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Contacto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idContacto { get; set; }

        public int idPaciente {  get; set; }
        [ForeignKey("idPaciente")]
        public virtual Paciente Pacientes { get; set; }

        public long celularPaciente { get; set; }
        public long celularAcompananteP { get; set; }
        public DateTime FechaInicioValidez { get; set; }
        public string estadoContacto { get; set; }
        public DateTime? eliminado { get; set; }

    }
}
