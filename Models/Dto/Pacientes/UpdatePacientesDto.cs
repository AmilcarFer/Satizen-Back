namespace Satizen_Api.Models.Dto.Pacientes
{
    public class UpdatePacientesDto
    {
        public int idPaciente {  get; set; }
        public int idUsuario { get; set; }
        public int idInstitucion { get; set; }
        public string? nombrePaciente { get; set; }
        public string? apellido { get; set; }
        public string? dni { get; set; }
        public int numeroHabitacionPaciente { get; set; }
        public string? observacionPaciente { get; set; }
    }
}
