using Satizen_Api.Models;

namespace Satizen_Api.Models.Dto.Asignaciones
{
    public class ActualizarAsignacionDto
    {
        public int idAsignacion {  get; set; }
        public int idPersonal { get; set; }
        public int idSector { get; set; }
        public string diaSemana { get; set; }
        public int idTurno { get; set; }
        public DateTime horaInicio { get; set; }
        public TimeSpan horaFinalizacion { get; set; }
    }
}