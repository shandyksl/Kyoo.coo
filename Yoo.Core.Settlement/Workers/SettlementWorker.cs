using System;
using Newtonsoft.Json.Linq;
using Yoo.Core.Common.Helper;
using Yoo.Core.Common.Storage;
using Yoo.Core.Settlement.GameSettlement;

namespace Yoo.Core.Settlement.Workers
{
	public class SettlementWorker : BackgroundService
    {
        private readonly ILogger<SettlementWorker> _logger;
        private readonly RabbitMQHelper _rabbitMQHelper;

        public SettlementWorker(ILogger<SettlementWorker> logger, RabbitMQHelper rabbitMQHelper)
        {
            _logger = logger;
            _rabbitMQHelper = rabbitMQHelper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var exchange = "yoo_settlement";
            var queue = "yoo_crypto_queue";
            var routingKey = "yoo_crypto";

            _logger.LogInformation("SettlementWorker running at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                _rabbitMQHelper.ConsumeMessages(exchange, queue, routingKey, async message =>
                {
                    _logger.LogInformation($"[RabbitMQ][Consume][{routingKey}:{queue}] - {message}");
                    await HandleReceivedMessage(message);
                });

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task HandleReceivedMessage(string message)
        {
            //解析消息内容
            dynamic parsedMessage = JObject.Parse(message);
            //执行游戏结算逻辑
            await PerformGameSettlement(parsedMessage.gameType.ToString(), parsedMessage.roundId.ToString(), parsedMessage.gameCode.ToString());
        }

        public async Task<object> PerformGameSettlement(string gameType, string roundId, string gameCode)
        {
            object result = null;

            switch (gameType)
            {
                case "CFFC":
                case "STCP":
                    CryptoAndStock crytoAndStock = new CryptoAndStock();
                    result = await crytoAndStock.CryptoSettlement(gameType, roundId, gameCode);
                    break;

                case "BOCP":
                    BoxOfficeCP boxOfficeCP = new BoxOfficeCP();
                    result = await boxOfficeCP.BOCPSettlement(gameType, roundId, gameCode);
                    break;

                case "EventCP":
                    EventCP eventCP = new EventCP();
                    result = await eventCP.BOCPSettlement(gameType, roundId, gameCode);
                    break;

                default:

                    result = CommonFunction.GetErrorCode(111);
                    break;
            }

            return result;
        }
    }
}