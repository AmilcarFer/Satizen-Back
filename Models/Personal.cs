using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models
{
    public class Personal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idPersonal { get; set; }

        public int idInstitucion { get; set; }
        [ForeignKey("idInstitucion")]
        public virtual Institucion? Instituciones { get; set; }

        public int idUsuario { get; set; }
        [ForeignKey("idUsuario")]
        public virtual Usuario Usuarios { get; set; }


        [Required]
        public string? nombrePersonal { get; set; }

        //[Required]
        //public int? rolPersonal { get; set; }
        //[ForeignKey("idRol")]
        //public virtual Roles Roles { get; set; }

        [Required]
        public int celularPersonal { get; set; }
        [Required]
        public int? telefonoPersonal { get; set; }
        [Required]
        public string? correoPersonal { get; set; }

        public DateTime? fechaEliminacion { get; set; }
    }
}
