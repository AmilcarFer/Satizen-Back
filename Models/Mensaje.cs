using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class Mensaje
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int idAutor {  get; set; }
        [ForeignKey("idAutor")]
        public virtual Usuario Autor { get; set; }

        public int idReceptor { get; set; }
        [ForeignKey("idReceptor")]
        public virtual Usuario Receptor { get; set; }
        public bool Entregado { get; set; }
        public bool Visto { get; set; }

        public string contenidoMensaje { get; set; }
        public DateTime Timestamp { get; set; }

        public DateTime? FechaLectura { get; set; }
        public string? FileUrl { get; set; }
    }
}
