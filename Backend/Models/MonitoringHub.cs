using Microsoft.AspNetCore.SignalR;

namespace Backend.Models
{
    public class MonitoringHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("MonitoringUpdate", new
            {
                Type = "Info",
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}