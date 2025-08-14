using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Personal
{
    public class AddPersonalDto
    {
        public int idInstitucion { get; set; }
        [Required(ErrorMessage = "El nombre propio tiene que estar especificado")]
        public int idUsuario { get; set; }
        public string? nombrePersonal { get; set; }
        //[Required(ErrorMessage = "El rol propio tiene que estar especificado")]
        //public int? rolPersonal { get; set; }
        [Required(ErrorMessage = "El numero tiene que estar especificado")]
        public int celularPersonal { get; set; }
        [Required(ErrorMessage = "El numero tiene que estar especificado")]
        public int? telefonoPersonal { get; set; }
        //[RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El email no es correcto")]
        public string? correoPersonal { get; set; }


    }
}
