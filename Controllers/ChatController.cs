using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Mensaje;

namespace Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensajesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MensajesController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/Mensajes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajes()
        {
            return await _context.Mensajes.OrderBy(m => m.Timestamp).ToListAsync();
        }


        // GET: api/Mensajes/EntreUsuarios/{idAutor}/{idReceptor}
        [HttpGet("EntreUsuarios/{idAutor}/{idReceptor}")]
        public async Task<ActionResult<IEnumerable<Mensaje>>> GetMensajesEntreUsuarios(int idAutor, int idReceptor)
        {
            var mensajes = await _context.Mensajes
                .Where(m => (m.idAutor == idAutor && m.idReceptor == idReceptor) || (m.idAutor == idReceptor && m.idReceptor == idAutor))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            if (mensajes == null || !mensajes.Any())
            {
                return Ok();
            }

            return mensajes;
        }

        // POST: api/Mensajes
        [HttpPost]
        public async Task<ActionResult<Mensaje>> PostMensaje(CreateMensajeDto CreateMensajeDto)
        {
            var autor = await _context.Usuarios.FindAsync(CreateMensajeDto.idAutor);
            var receptor = await _context.Usuarios.FindAsync(CreateMensajeDto.idReceptor);

            if (autor == null)
            {
                return BadRequest("El autor seleccionado no existe.");
            }

            if (receptor == null)
            {
                return BadRequest("El receptor seleccionado no existe.");
            }

            var nuevoMensaje = new Mensaje
            {
                idAutor = CreateMensajeDto.idAutor,
                idReceptor = CreateMensajeDto.idReceptor,
                contenidoMensaje = CreateMensajeDto.contenidoMensaje,
                Timestamp = DateTime.UtcNow,
                Entregado = false,
                Visto = false
            };

            _context.Mensajes.Add(nuevoMensaje);
            await _context.SaveChangesAsync();

            string groupName = $"{Math.Min(nuevoMensaje.idAutor, nuevoMensaje.idReceptor)}-{Math.Max(nuevoMensaje.idAutor, nuevoMensaje.idReceptor)}";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", nuevoMensaje);

            return CreatedAtAction(nameof(GetMensajes), new { id = nuevoMensaje.Id }, nuevoMensaje);
        }

        [HttpPost("entregado/{id}")]
        public async Task<IActionResult> MarcarEntregado(int id)
        {
            var mensaje = await _context.Mensajes.FindAsync(id);
            if (mensaje == null)
            {
                return NotFound();
            }

            mensaje.Entregado = true;
            await _context.SaveChangesAsync();

            string groupName = $"{Math.Min(mensaje.idAutor, mensaje.idReceptor)}-{Math.Max(mensaje.idAutor, mensaje.idReceptor)}";
            await _hubContext.Clients.Group(groupName).SendAsync("MessageDelivered", id);

            return NoContent();
        }

        [HttpPost("visto/{id}")]
        public async Task<IActionResult> MarcarVisto(int id)
        {
            var mensaje = await _context.Mensajes.FindAsync(id);
            if (mensaje == null)
            {
                return NotFound();
            }

            mensaje.Visto = true;
            mensaje.FechaLectura = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            string groupName = $"{Math.Min(mensaje.idAutor, mensaje.idReceptor)}-{Math.Max(mensaje.idAutor, mensaje.idReceptor)}";
            await _hubContext.Clients.Group(groupName).SendAsync("MessageRead", id);

            return NoContent();
        }





        [HttpDelete("borrar-todo")]
        public IActionResult BorrarTodo()
        {
            // Obtiene todos los registros de la tabla
            var todaelhistorial = _context.Mensajes.ToList();

            // Elimina todos los registros
            _context.Mensajes.RemoveRange(todaelhistorial);
            _context.SaveChanges();

            return NoContent(); // Devuelve 204 No Content
        }



    }
}
