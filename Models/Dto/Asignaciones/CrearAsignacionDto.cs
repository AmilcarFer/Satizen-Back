using Satizen_Api.Models;

namespace Satizen_Api.Models.Dto.Asignaciones
{
    public class CrearAsigcionDto
    {
        public int idPersonal { get; set; }
        public int idSector { get; set; }
        public string diaSemana { get; set; }
        public int idTurno { get; set; }
        public DateTime horaInicio { get; set; }
        public TimeSpan horaFinalizacion { get; set; }
    }

}