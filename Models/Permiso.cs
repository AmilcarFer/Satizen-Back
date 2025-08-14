using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Permiso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idPermiso { get; set; }
        public string tipo {  get; set; }
    }
}
