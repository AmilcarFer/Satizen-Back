using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models
{
    public class Turno
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idTurno { get; set; }
        public string Nombre { get; set; }
    }

}