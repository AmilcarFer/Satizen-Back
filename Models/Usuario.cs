using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idUsuario {  get; set; }
        public string nombreUsuario { get; set; }
        public string correo { get; set; }
        public string password {  get; set; }

        public int? idRoles { get; set; }
        [ForeignKey("idRoles")]
        public virtual Roles Roles { get; set; }

        public DateTime fechaCreacion { get; set; }
        public DateTime? fechaActualizacion { get; set; }

        public DateTime? fechaEliminacion { get; set; }

    }
}
