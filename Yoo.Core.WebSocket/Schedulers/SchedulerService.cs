
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Yoo.Core.Business.Interface;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;
using Yoo.Core.WebSocket.Hubs;

namespace Yoo.Core.WebSocket.Schedulers
{
    public class SchedulerService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _STCPTimer;
        private Timer _CFFCTimer;
        private Timer _BOCPTimer;
        private Timer _EVCPTimer;

        public SchedulerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();
                var _authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var _hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DataHub>>();

                _STCPTimer = new Timer(async _ =>
                {
                    var result = await _assetService.GetAssetsByAssetType(AssetType.STOCKMARKETINDEX);
                    await _hubContext.Clients.All.SendAsync(GameType.STOCKCP, JsonConvert.SerializeObject(result));
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

                _CFFCTimer = new Timer(async _ =>
                {
                    var result = await _assetService.GetAssetsByAssetType(AssetType.CRYPTO);
                    await _hubContext.Clients.All.SendAsync(GameType.CRYPTOFFC, JsonConvert.SerializeObject(result));
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));


                List<AgentConfig> agentConfigList = await _authService.GetAllAgentConfigs();

                if(agentConfigList != null)
                {
                    foreach (AgentConfig agentConfig in agentConfigList)
                    {
                        _BOCPTimer = new Timer(async _ =>
                        {
                            Random _random = new Random();
                            var ok = _random.Next(1000, 5000 + 1).ToString();
                            await _hubContext.Clients.Group($"{GameType.BOXOFFICECP}_{agentConfig.AgentCode}").SendAsync(GameType.BOXOFFICECP, "BoxOffice-" + ok);
                        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

                        _EVCPTimer = new Timer(async _ =>
                        {
                            Random _random = new Random();
                            var ok = _random.Next(1000, 5000 + 1).ToString();
                            await _hubContext.Clients.Group($"{GameType.EVENTCP}_{agentConfig.AgentCode}").SendAsync(GameType.EVENTCP, "MH370-" + ok);
                        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                    }
                }
                
            }

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _STCPTimer?.Change(Timeout.Infinite, 0);
            _CFFCTimer?.Change(Timeout.Infinite, 0);
            _BOCPTimer?.Change(Timeout.Infinite, 0);
            _EVCPTimer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _STCPTimer.Dispose();
            _CFFCTimer.Dispose();
            _BOCPTimer.Dispose();
            _EVCPTimer.Dispose();
        }

    }
}
