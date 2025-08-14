using Satizen_Api.Models;

public class AsignacionDto
{
    public int idAsignacion { get; set; }
    public int idPersonal { get; set; }
    public int idSector { get; set; }
    public string diaSemana { get; set; }
    public string turno { get; set; }
    public DateTime horaInicio { get; set; }
    public TimeSpan horaFinalizacion { get; set; }
}
