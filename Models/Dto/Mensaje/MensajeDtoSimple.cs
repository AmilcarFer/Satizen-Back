namespace Satizen_Api.Models.Dto.Mensaje
{
    public class MensajeDtoSimple
    {
        public int Id { get; set; }
        public int idAutor { get; set; }
        public int idReceptor { get; set; }
        public string contenidoMensaje { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Entregado { get; set; }
        public bool Visto { get; set; }
        public DateTime? FechaLectura { get; set; }
        public string? FileUrl { get; set; }
    }
}
