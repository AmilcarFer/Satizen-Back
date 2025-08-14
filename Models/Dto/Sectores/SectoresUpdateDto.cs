using System.ComponentModel.DataAnnotations;


namespace Satizen_Api.Models.Dto.Sectores
{
    public class SectoresUpdateDto
    {
        [Required]
        public int idSector {  get; set; }
        [Required]
        public int idInstitucion { get; set; }
        [Required]
        public string nombreSector { get; set; }
        [Required]
        public string descripcionSector { get; set; }

    }
}
