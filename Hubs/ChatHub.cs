using Microsoft.AspNetCore.SignalR;
using Satizen_Api.Data;
using Satizen_Api.Models;
using Satizen_Api.Models.Dto.Mensaje;
using System;

namespace Satizen_Api.Hubs
{
    /// <summary>
    /// Hub de chat para manejo de mensajes en tiempo real.
    /// Permite a los clientes unirse a conversaciones, enviar
    /// mensajes y actualizar estados como entregado y visto.
    /// </summary>
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

