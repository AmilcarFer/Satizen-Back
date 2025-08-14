namespace Satizen_Api.Models.Dto.Mensaje
{
    public class CreateMensajeDto
    {
        public int idAutor{ get; set; } 
        public int idReceptor { get; set; } 
        public string contenidoMensaje { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
