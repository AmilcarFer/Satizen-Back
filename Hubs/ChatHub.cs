using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Mensaje;
using System;
using System.Threading.Tasks;

namespace Satizen_Api.Hubs
{
    /// <summary>
    /// Hub de chat para manejo de mensajes en tiempo real.
    /// Permite a los clientes unirse a conversaciones, enviar
    /// mensajes y actualizar estados como entregado y visto.
    /// </summary>
    /// 
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string GetGroupName(int userA, int userB)
        {
            return $"{Math.Min(userA, userB)}-{Math.Max(userA, userB)}";
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"[SignalR] Conectado: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"[SignalR] Desconectado: {Context.ConnectionId} - Error: {exception?.Message}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinConversation(int otherUserId)
        {
            try
            {
                var userIdStr = Context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                // Usamos int.TryParse que no lanza excepci�n si falla.
                if (!int.TryParse(userIdStr, out int userId))
                {
                    // Si el valor del claim no es un n�mero v�lido, lo registramos y salimos.
                    Console.WriteLine($"[SignalR Hub Error] El 'nameidentifier' claim con valor '{userIdStr}' no es un entero v�lido.");
                    return;
                }

                // Si llegamos aqu�, userId es v�lido y podemos unir al grupo de forma segura.
                Console.WriteLine($"[SignalR] Usuario {userId} uni�ndose a la conversaci�n con {otherUserId}.");
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(userId, otherUserId));
            }
            catch (Exception ex)
            {
                // Atrapamos cualquier otra excepci�n inesperada para que no cierre la conexi�n.
                Console.WriteLine($"[SignalR] Excepci�n en JoinConversation: {ex.Message}");
            }
        }

        public async Task SendMessage(CreateMensajeDto dto)
        {
            try
            {
                Console.WriteLine($"[SignalR] SendMessage de {dto.idAutor} para {dto.idReceptor}: '{dto.contenidoMensaje}'");
                if (dto == null || dto.idAutor <= 0 || dto.idReceptor <= 0)
                {
                    Console.WriteLine("[SignalR] Error: dto inv�lido.");
                    return;
                }

                var message = new Mensaje
                {
                    idAutor = dto.idAutor,
                    idReceptor = dto.idReceptor,
                    contenidoMensaje = dto.contenidoMensaje,
                    Timestamp = DateTime.UtcNow,
                    Entregado = false,
                    Visto = false
                };

                _context.Mensajes.Add(message);
                await _context.SaveChangesAsync();

                await Clients.Group(GetGroupName(message.idAutor, message.idReceptor))
                    .SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] Excepci�n en SendMessage: {ex.Message}");
            }
        }

        public async Task ConfirmDelivery(int messageId)
        {
            try
            {
                Console.WriteLine($"[SignalR] ConfirmDelivery: {messageId}");
                var mensaje = await _context.Mensajes.FindAsync(messageId);
                if (mensaje != null && !mensaje.Entregado)
                {
                    mensaje.Entregado = true;
                    await _context.SaveChangesAsync();

                    await Clients.Group(GetGroupName(mensaje.idAutor, mensaje.idReceptor))
                        .SendAsync("MessageDelivered", messageId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] Excepci�n en ConfirmDelivery: {ex.Message}");
            }
        }

        public async Task ConfirmRead(int messageId)
        {
            try
            {
                Console.WriteLine($"[SignalR] ConfirmRead: {messageId}");
                var mensaje = await _context.Mensajes.FindAsync(messageId);
                if (mensaje != null && !mensaje.Visto)
                {
                    mensaje.Visto = true;
                    mensaje.FechaLectura = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await Clients.Group(GetGroupName(mensaje.idAutor, mensaje.idReceptor))
                        .SendAsync("MessageRead", messageId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] Excepci�n en ConfirmRead: {ex.Message}");
            }
        }
    }
}
