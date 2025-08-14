using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Usuarios
{
    public class UsuarioUpdateDto
    {
        [Required]
        public int idUsuario { get; set; }
        [Required]
        public string nombreUsuario { get; set; }
        [Required]
        public string correo {  get; set; }
        //[Required]
        //public string password { get; set; }
        [Required]
        public int? idRoles { get; set; }

    }
}
