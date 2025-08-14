using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Satizen_Api.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}