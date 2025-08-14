using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Institucion
{
    public class InstitucionCreateDto
    {

        [Required]
        [MaxLength(30)]
        public string? nombreInstitucion { get; set; }
        public string? direccionInstitucion { get; set; }
        public string? telefonoInstitucion { get; set; }
        public string? correoInstitucion { get; set; }
        public string? celularInstitucion { get; set; }
    }
}