using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Yoo.Core.Business.Interface;
using Yoo.Core.Middleware.Filters;
using Yoo.Core.Model.Common;

namespace Yoo.Core.WebSocket.Hubs
{
    public class DataHub : Hub
    {
        private readonly IAuthService _authService;

        public DataHub(IAuthService authService)
        {
            _authService = authService;
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.GetHttpContext()!.Items["AuthenticatedUser"] as AuthenticatedUser;
            if (user == null)
            {
                Context.Abort();
                return;
            }

            var agent = await _authService.GetAgentConfigByAgentCode(user.AgentCode);
            if (agent == null)
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"{GameType.BOXOFFICECP}_{agent.AgentCode}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{GameType.EVENTCP}_{agent.AgentCode}");

            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has connected to Main");
        }
    }
}
