using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Llamados
{
    public class AsignarLlamadoDto
    {
        [Required]
        public int idPersonal { get; set; }

    }
}
