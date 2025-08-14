using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Llamado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idLlamado {  get; set; }

        public int idPaciente { get; set; }
        [ForeignKey("idPaciente")]
        public virtual Paciente Pacientes { get; set; }

        public int? idPersonal { get; set; }
        [ForeignKey("idPersonal")]
        public virtual Personal Personals { get; set; }

        public DateTime fechaHoraLlamado { get; set; }
        public string estadoLlamado { get; set; }
        public string prioridadLlamado { get; set; }
        public string observacionLlamado { get; set; }
        public DateTime? fechaEliminacion { get; set; }

    }

}

