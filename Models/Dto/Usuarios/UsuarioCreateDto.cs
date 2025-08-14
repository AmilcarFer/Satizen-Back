namespace Satizen_Api.Models.Dto.Usuarios
{
    public class UsuarioCreateDto
    {
        public string nombreUsuario { get; set; }
        public string correo {  get; set; }
        public string password { get; set; }
        public int idRoles { get; set; }

    }
}
