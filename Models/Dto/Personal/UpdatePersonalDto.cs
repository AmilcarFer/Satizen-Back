using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Personal
{
    public class UpdatePersonalDto
    {
        public int idPersonal { get; set; }

        public int idInstitucion { get; set; }
        public int idUsuario { get; set; }

        [Required]
        public string? nombrePersonal { get; set; }
        //[Required]
        //public int? rolPersonal { get; set; }
        [Required]
        public int celularPersonal { get; set; }
        [Required]
        public int telefonoPersonal { get; set; }
        [Required]
        public string? correoPersonal { get; set; }
    }
}