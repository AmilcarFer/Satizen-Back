using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Llamados
{
    public class LlamadoCreateDto
    {
        public int idPaciente { get; set; } //clave foranea (tabla pacientes)
        public int? idPersonal { get; set; } //clave foranea (tabla personal)
        public required string estadoLlamado { get; set; }
        public required string prioridadLlamado { get; set; }
        public required string observacionLlamado { get; set; }
    }

}


