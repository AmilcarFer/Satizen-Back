namespace Satizen_Api.Models.Dto.Dispositivos
{
    public class DispositivoUpdateDto
    {
        public int idDispositivo {  get; set; }
        public int idPersonal { get; set; }

        public string numDispositivo { get; set; }
        public string observacionDispositivo { get; set; }
        public string estadoDispositivo { get; set; }
    }
}
