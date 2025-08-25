using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.Usuarios
{
    public class CambiarPasswordDto
    {
        [Required]
        public string password { get; set; }
        [Required]
        public string recoveryToken { get; set; }

    }
}
