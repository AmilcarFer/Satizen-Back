namespace Satizen_Api.Models.Dto.Llamados
{
    public class PacienteCreateLlamado
    {
        public int idPaciente { get; set; }
        public required string prioridadLlamado { get; set; }

        public required string observacionLlamado { get; set; }
    }
}
