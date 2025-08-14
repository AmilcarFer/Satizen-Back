using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Roles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRol { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }

        public int? idPermiso { get; set; }
        [ForeignKey("idPermiso")]
        public virtual Permiso Permiso { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }
}
