using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Asignacion
    {
        [Key]
        public int idAsignacion { get; set; }

        public int idPersonal { get; set; }
        [ForeignKey("idPersonal")]
        public virtual Personal Personals { get; set; }

        public int idSector { get; set; }
        [ForeignKey("idSector")]
        public virtual Sector Sectores { get; set; }

        public string diaSemana { get; set; }

        public int idTurno { get; set; } // Clave foránea
        [ForeignKey("idTurno")]
        public virtual Turno Turnos { get; set; } // Navegación

        public DateTime horaInicio { get; set; }
        public TimeSpan horaFinalizacion { get; set; }

        public DateTime? fechaEliminacion { get; set; }
    }
}
