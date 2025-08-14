using System.ComponentModel.DataAnnotations;


namespace Satizen_Api.Models.Dto.Sectores
{
    public class SectoresCreateDto
    {
        public int idInstitucion { get; set; }
        public string nombreSector { get; set; }
        public string descripcionSector { get; set; }

    }
}
