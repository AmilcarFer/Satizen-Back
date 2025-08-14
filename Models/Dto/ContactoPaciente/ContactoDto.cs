using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Satizen_Api.Models.Dto.ContactoPaciente
{
    public class ContactoDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idContacto { get; set; }
        [ForeignKey("idPaciente")]
        public int idPaciente { get; set; }
        public int celularPaciente { get; set; }
        public int celularAcompananteP { get; set; }
        public DateTime FechaInicioValidez { get; set; }
        public string estadoContacto { get; set; }
        public DateTime? eliminado { get; set; }
    }
}
