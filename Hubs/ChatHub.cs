using Microsoft.AspNetCore.SignalR;
using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Mensaje;
using System;

public class ChatHub : Hub
{
    private static Dictionary<string, string> ConnectedUsers = new Dictionary<string, string>();
    private static Dictionary<string, HashSet<string>> GroupMembers = new Dictionary<string, HashSet<string>>();
    private static readonly Dictionary<string, HashSet<string>> groupConnections = new Dictionary<string, HashSet<string>>();
    private static readonly Dictionary<string, int> groupUserCount = new Dictionary<string, int>();

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string GetGroupName(int userA, int userB)
        {
            return $"{Math.Min(userA, userB)}-{Math.Max(userA, userB)}";
        }

        public async Task JoinConversation(int userId, int otherUserId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(userId, otherUserId));
        }

        public async Task SendMessage(CreateMensajeDto dto)
        {
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

        public async Task ConfirmDelivery(int messageId)
        {
            var mensaje = await _context.Mensajes.FindAsync(messageId);
            if (mensaje != null && !mensaje.Entregado)
            {
                mensaje.Entregado = true;
                await _context.SaveChangesAsync();

                await Clients.Group(GetGroupName(mensaje.idAutor, mensaje.idReceptor))
                    .SendAsync("MessageDelivered", messageId);
            }
        }

        public async Task ConfirmRead(int messageId)
        {
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
    }
}

