using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Usuarios
{
    public class RecuperarPasswordDto
    {
        [EmailAddress]
        public string correo { get; set; }
    }
}
