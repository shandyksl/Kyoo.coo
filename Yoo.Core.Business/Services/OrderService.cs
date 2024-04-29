using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.Common.Storage;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;
using Yoo.Core.Model.DTO;

namespace Yoo.Core.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly IDatabase _redis;
        public OrderService(ILogger<OrderService> logger, RedisHelper redis)
        {
            _logger = logger;
            _redis = redis.GetDatabase();
        }

        public async Task<object> PlaceBet(string agentCode, string loginName, string gameType, decimal totalBet, List<BetInfo> betInfo)
        {
            try
            {
                Player player = await PlayerDAO.GetPlayer(agentCode, loginName);
                if (player == null)
                {
                    _logger.LogInformation("PlaceBet报错:" + CommonFunction.GetErrorDesc(106));
                    return CommonFunction.GetErrorCode(106); // player_not_found
                }
                else if (player.Balance < totalBet)
                {
                    _logger.LogInformation("PlaceBet报错:" + CommonFunction.GetErrorDesc(113));
                    return CommonFunction.GetErrorCode(113); // wallet_insufficient_balance
                }

                List<object> successTx = new List<object>();

                foreach (var bet in betInfo)
                {
                    int lastBetHistoryId = await BetDAO.GetLastBetHistoryId() + 1;
                    string pid = GenerateTransactionId(lastBetHistoryId);

                    string betDetail = GenerateBetDetailQuery(lastBetHistoryId, bet.BetOptions);

                    string txId = await BetDAO.PlaceBet(agentCode, loginName, player.CurrencyCode, pid, gameType, bet.GameCode, bet.RoundId, bet.BetAmount, DateTime.UtcNow , betDetail, "PlaceBet");

                    if (string.IsNullOrEmpty(txId))
                    {
                        _logger.LogInformation("PlaceBet报错:" + CommonFunction.GetErrorDesc(902));
                        return CommonFunction.GetErrorCode(902);
                    }
                    else
                    {
                        successTx.Add(new
                        {
                            bet.RoundId,
                            TransactionId = txId,
                            TransactionAmount = bet.BetAmount
                        });
                    }
                }

                // 获取最新余额
                var playerLatest = await PlayerDAO.GetPlayer(player.AgentCode, player.LoginName);

                return CommonFunction.GetSuccessCode(new
                {
                    CurrentBalance = playerLatest.Balance,
                    BetInfo = successTx
                });

            }
            catch (MySqlException ex)
            {
                if (CommonFunction.isContainErrorCode(ex.Number))
                {
                    _logger.LogInformation("PlaceBet报错:" + CommonFunction.GetErrorDesc(ex.Number));
                    return CommonFunction.GetErrorCode(ex.Number);
                }
                else
                {
                    _logger.LogInformation($"SQL事务失败: {ex.Message}");
                    return CommonFunction.GetErrorCode(503); // place_bet_failed
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"PlaceBet报错: {CommonFunction.GetErrorDesc(503)} - {ex.Message}");
                return CommonFunction.GetErrorCode(503); // place_bet_failed
            }
        }

        private static string GenerateTransactionId(long genId)
        {
            // YYGP + yyyyMMddHHmm + 00000
            DateTime now = DateTime.Now;
            string dateString = now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
            string genIdString = genId.ToString("D5");

            return "YYGP" + dateString + genIdString;
        }

        private static string GenerateBetDetailQuery(int betHistoryId, List<BetOption> option)
        {
            StringBuilder betDetailSQL = new StringBuilder();

            foreach (BetOption betOption in option)
            {
                if (betDetailSQL.Length > 0)
                {
                    betDetailSQL.Append(',');
                }
                else
                {
                    betDetailSQL.Append("INSERT INTO BetDetail(BetHistoryId,BetType,BetCode,Odds,BetAmount)VALUES");
                }

                betDetailSQL.Append("(" + betHistoryId + ",'" +
                                    betOption.BetType + "','" +
                                    betOption.BetCode + "'," +
                                    betOption.Odds + "," +
                                    betOption.BetAmount + ")"
                                    );
            }

            return betDetailSQL.ToString();
        }
    }
}
