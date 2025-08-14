using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[Authorize]
public class ChatHub : Hub
{
    private static Dictionary<string, string> ConnectedUsers = new Dictionary<string, string>();
    private static Dictionary<string, HashSet<string>> GroupMembers = new Dictionary<string, HashSet<string>>();
    private static readonly Dictionary<string, HashSet<string>> groupConnections = new Dictionary<string, HashSet<string>>();
    private static readonly Dictionary<string, int> groupUserCount = new Dictionary<string, int>();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var nombre = Context.User.FindFirst(ClaimTypes.Name)?.Value;

        if (userId != null)
        {
            ConnectedUsers[userId] = nombre;

            // Crear grupos de pares (u1-u2, u1-u3, etc.)
            foreach (var otherUserId in ConnectedUsers.Keys)
            {
                if (otherUserId != userId)
                {
                    string groupName = GetGroupName(userId, otherUserId);

                    // Registrar el grupo solo si no existe
                    if (!GroupMembers.ContainsKey(groupName))
                    {
                        GroupMembers[groupName] = new HashSet<string>();
                    }

                    // Agregar ambos usuarios al grupo si tiene menos de 2 miembros
                    if (GroupMembers[groupName].Count < 2 &&
                        !GroupMembers[groupName].Contains(userId))
                    {
                        GroupMembers[groupName].Add(userId);
                        GroupMembers[groupName].Add(otherUserId);

                        // Agregar al grupo SignalR
                        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    }
                }
            }

            // Actualizar lista de usuarios conectados
            var userList = ConnectedUsers.Select(kvp => new { UserId = kvp.Key, Nombre = kvp.Value }).ToList();
            await Clients.All.SendAsync("UpdateConnectedUsers", userList);
            await Clients.All.SendAsync("UpdateUserCount", ConnectedUsers.Count);

            // Imprimir estado actual de los grupos
            PrintGroupStatus();
        }

        Console.WriteLine($"Usuario conectado: {Context.ConnectionId}, ID: {userId}, Nombre: {nombre}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            ConnectedUsers.Remove(userId);

            // Eliminar al usuario de todos los grupos
            foreach (var groupName in GroupMembers.Keys.ToList())
            {
                if (GroupMembers[groupName].Contains(userId))
                {
                    GroupMembers[groupName].Remove(userId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                    // Eliminar grupos vacíos
                    if (GroupMembers[groupName].Count == 0)
                    {
                        GroupMembers.Remove(groupName);
                    }
                }
            }

            // Actualizar lista de usuarios conectados
            await Clients.All.SendAsync("UpdateConnectedUsers", ConnectedUsers.ToList());
            await Clients.All.SendAsync("UpdateUserCount", ConnectedUsers.Count);

            // Imprimir estado actual de los grupos
            PrintGroupStatus();
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToGroup(string otherUserId, string contenidoMensaje)
    {
        var senderId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (senderId == null) return;

        string groupName = GetGroupName(senderId, otherUserId);

        // Verificar que el grupo existe y tiene exactamente dos miembros
        if (GroupMembers.ContainsKey(groupName) && GroupMembers[groupName].Count == 2)
        {
            await Clients.OthersInGroup(groupName).SendAsync("ReceiveGroupMessage", senderId, contenidoMensaje);
        }
        else
        {
            Console.WriteLine($"El grupo {groupName} no es válido o no tiene dos miembros.");
        }
    }

    // Genera un nombre de grupo único basado en dos IDs de usuario
    private string GetGroupName(string userId1, string userId2)
    {
        var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id);
        return $"Group-{string.Join("-", sortedIds)}";
    }

    public async Task JoinGroup(int idAutor, int idReceptor)
    {
        string groupName = GetGroupName(idAutor.ToString(), idReceptor.ToString()); // Convertir a string
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"groupName: {groupName}, connectionId: {connectionId}");

        // Asegurarse de que el grupo exista
        if (!groupUserCount.ContainsKey(groupName))
        {
            groupUserCount[groupName] = 0;
            groupConnections[groupName] = new HashSet<string>();
        }

        // Verificar si la conexión ya está en el grupo
        if (!groupConnections[groupName].Contains(connectionId))
        {
            groupConnections[groupName].Add(connectionId);

            // Solo permitir unirse si el grupo tiene menos de 2 usuarios
            if (groupUserCount[groupName] < 2)
            {
                groupUserCount[groupName]++;
                Console.WriteLine($"groupUserCount: {groupUserCount[groupName]}, connectionId: {connectionId}");


                await Groups.AddToGroupAsync(connectionId, groupName);
                await Clients.Group(groupName).SendAsync("UpdateGroupCount", groupUserCount[groupName]);
            }
        }
        else
        {
            Console.WriteLine("El usuario ya está en el grupo.");
        }
    }


    public async Task LeaveGroup(int idAutor, int idReceptor)
    {
        string groupName = GetGroupName(idAutor.ToString(), idReceptor.ToString()); // Convertir a string
        string connectionId = Context.ConnectionId;

        if (groupConnections.ContainsKey(groupName))
        {
            // Remover la conexión del grupo
            groupConnections[groupName].Remove(connectionId);

            // Si no hay más conexiones en el grupo, eliminar el grupo y el contador
            if (groupConnections[groupName].Count == 0)
            {
                groupConnections.Remove(groupName);
                groupUserCount.Remove(groupName);
            }
            else
            {
                // Actualizar el contador de usuarios para el grupo
                groupUserCount[groupName]--;
            }

            // Remover el usuario del grupo en SignalR
            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            // Enviar el conteo actualizado a todos los usuarios en el grupo
            await Clients.Group(groupName).SendAsync("UpdateGroupCount", groupUserCount.ContainsKey(groupName) ? groupUserCount[groupName] : 0);
        }
    }

    public async Task Disconnect()
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            ConnectedUsers.Remove(userId);

            // Remover al usuario de todos los grupos
            foreach (var groupName in GroupMembers.Keys.ToList())
            {
                if (GroupMembers[groupName].Contains(userId))
                {
                    GroupMembers[groupName].Remove(userId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                    // Eliminar grupos vacíos
                    if (GroupMembers[groupName].Count == 0)
                    {
                        GroupMembers.Remove(groupName);
                    }
                }
            }

            // Actualizar lista de usuarios conectados
            await Clients.All.SendAsync("UpdateConnectedUsers", ConnectedUsers.ToList());
            await Clients.All.SendAsync("UpdateUserCount", ConnectedUsers.Count);

            // Imprimir estado actual de los grupos
            PrintGroupStatus();

            Console.WriteLine($"Usuario desconectado manualmente: {Context.ConnectionId}, ID: {userId}");
        }

    }

    private void PrintGroupStatus()
    {
        Console.WriteLine("Estado actual de los grupos:");
        foreach (var group in GroupMembers)
        {
            Console.WriteLine($"Grupo: {group.Key}");
            Console.WriteLine($"Miembros: {string.Join(", ", group.Value)}");
        }
    }
}