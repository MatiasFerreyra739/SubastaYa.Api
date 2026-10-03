using Microsoft.AspNetCore.SignalR;

namespace SubastaYa.Api.Hubs
{
    public class SubastaHub : Hub
    {
        public async Task UnirseASubasta(int subastaId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"subasta-{subastaId}");
        }

        public async Task SalirDeSubasta(int subastaId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"subasta-{subastaId}");
        }
    }
}