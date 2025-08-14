using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satizen_Api.Models
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRefreshToken { get; set; }

        public int idUsuario { get; set; }
        [ForeignKey("idUsuario")]
        public virtual Usuario Usuario { get; set; }

        public string token { get; set; }
        public string refreshToken { get; set; }
        public DateTime fechaCreacion { get; set; }
        public DateTime fechaExpiracion { get; set; }
        public bool esActivo { get; set; }

        public static implicit operator string(RefreshToken v)
        {
            throw new NotImplementedException();
        }
    }
}
