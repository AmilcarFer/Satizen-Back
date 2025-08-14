using System.ComponentModel.DataAnnotations;


namespace Satizen_Api.Models.Dto.Sectores
{
    public class SectoresDto
    {
        public int idSector { get; set; }
        public int idInstitucion { get; set; }
        public string nombreSector { get; set; }
        public string descripcionSector { get; set; }
        public DateTime fechaCreacion { get; set; }
        public DateTime fechaActualizacion { get; set; }
        public DateTime fechaEliminacion { get; set; }

    }
}
