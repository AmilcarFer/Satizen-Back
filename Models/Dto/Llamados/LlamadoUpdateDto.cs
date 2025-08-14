using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Llamados
{
    public class LlamadoUpdateDto
    {
        [Required]
        public int idLlamado { get; set; }
        [Required]
        public int idPaciente { get; set; } //clave foranea (tabla pacientes)
        [Required]
        public int? idPersonal { get; set; }//clave foranea (tabla personal)
        [Required]
        public string estadoLlamado { get; set; }
        [Required]
        public string prioridadLlamado { get; set; }
        [Required]
        public string observacionLlamado { get; set; }

    }
}


