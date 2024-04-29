using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
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
    public class PlayerService : IPlayerService
    {
        private readonly ILogger<PlayerService> _logger;
        private readonly IDatabase _redis;
        private readonly IAuthService _authService;
        private readonly IGameService _gameService;

        public PlayerService(ILogger<PlayerService> logger, RedisHelper redis, IGameService gameService, IAuthService authService)
        {
            _logger = logger;
            _redis = redis.GetDatabase();
            _gameService = gameService;
            _authService = authService;
        }

        public async Task<object> CreatePlayer(string agentCode, string loginName, string currencyCode, string ipAddress)
        {
            var agent = await _authService.GetAgentConfigByAgentCode(agentCode);
            if (agent == null)
            {
                _logger.LogInformation("CreatePlayer报错:" + CommonFunction.GetErrorDesc(109));
                return CommonFunction.GetErrorCode(109); // agent_not_found
            }
            if (!agent.IsActive)
            {
                _logger.LogInformation("CreatePlayer报错:" + CommonFunction.GetErrorDesc(104));
                return CommonFunction.GetErrorCode(104); // agent_disable
            }

            var playerExist = await PlayerDAO.GetPlayer(agent.AgentCode, loginName);
            if (playerExist != null)
            {
                _logger.LogInformation("CreatePlayer报错:" + CommonFunction.GetErrorDesc(110));
                return CommonFunction.GetErrorCode(110); //player_name_exists
            }

            bool isSuccess = await PlayerDAO.InsertPlayer(agentCode, loginName, currencyCode, ipAddress);
            if (!isSuccess)
            {
                _logger.LogInformation("CreatePlayer报错:" + CommonFunction.GetErrorDesc(902));
                return CommonFunction.GetErrorCode(902);
            }
            else
            {
                //get player again to validate
                var playerInfo = await this.GetPlayerByLoginNameAgentCode(agentCode, loginName);
                if (playerInfo == null)
                {
                    _logger.LogInformation("CreatePlayer报错:" + CommonFunction.GetErrorDesc(106));
                    return CommonFunction.GetErrorCode(106); //player_not_found
                }

                return CommonFunction.GetSuccessCode(new
                {
                    playerInfo.AgentCode,
                    playerInfo.LoginName,
                    playerInfo.CurrencyCode,
                });
            }
        }

        public async Task<Player?> GetPlayerByLoginNameAgentCode(string agentCode, string loginName)
        {
            return await PlayerDAO.GetPlayer(agentCode, loginName);
        }

        public async Task<object> LoginPlayer(string agentCode, string loginName, string languageCode, string currencyCode, string nickName, string platform)
        {
            var agent = await _authService.GetAgentConfigByAgentCode(agentCode);
            if (agent == null)
            {
                _logger.LogInformation("LoginPlayer报错:" + CommonFunction.GetErrorDesc(109));
                return CommonFunction.GetErrorCode(109); // agent_not_found
            }
            if (!agent.IsActive)
            {
                _logger.LogInformation("LoginPlayer报错:" + CommonFunction.GetErrorDesc(104));
                return CommonFunction.GetErrorCode(104); // agent_disable
            }

            var player = await PlayerDAO.GetPlayer(agent.AgentCode, loginName);
            if (player == null)
            {
                _logger.LogInformation("LoginPlayer报错:" + CommonFunction.GetErrorDesc(106));
                return CommonFunction.GetErrorCode(106); //player_not_found
            }
            if (player.Status != PlayerStatus.ACTIVE)
            {
                _logger.LogInformation("LoginPlayer报错:" + CommonFunction.GetErrorDesc(102));
                CommonFunction.GetErrorCode(102); //invalid_player
            }

            bool isSuccess = await PlayerDAO.LoginPlayer(agentCode, loginName, nickName, languageCode);
            if (!isSuccess)
            {
                _logger.LogInformation("LoginPlayer报错:" + CommonFunction.GetErrorDesc(111));
                return CommonFunction.GetErrorCode(111);
            }
            else
            {
                var jwt = await _authService.GenerateBearerToken(player.AgentCode, player.LoginName, player.CurrencyCode, agent.ApiKey);

                await PlayerDAO.InsertPlayerAuthLog(player.AgentCode, player.LoginName, jwt, platform);

                return CommonFunction.GetSuccessCode(new
                {
                    player.AgentCode,
                    player.LoginName,
                    LanguageCode = languageCode,
                    player.CurrencyCode,
                    NickName = string.IsNullOrEmpty(nickName) ? player.NickName : nickName,
                    Url = $"/?token={jwt}"
                });
            }
        }

        public async Task<object> GetPlayerBalance(string agentCode, string loginName)
        {
            var player = await PlayerDAO.GetPlayer(agentCode, loginName);
            if (player == null)
            {
                _logger.LogInformation("GetPlayerBalance报错:" + CommonFunction.GetErrorDesc(106));
                return CommonFunction.GetErrorCode(106); //player_not_found
            }

            return CommonFunction.GetSuccessCode(new
            {
                player.Balance,
                player.CurrencyCode
            });
        }

        public async Task<object> GetPlayerInfo(string agentCode, string loginName)
        {
            var player = await PlayerDAO.GetPlayer(agentCode, loginName);
            if (player == null)
            {
                _logger.LogInformation("GetPlayerInfo报错:" + CommonFunction.GetErrorDesc(106));
                return CommonFunction.GetErrorCode(106); //player_not_found
            }

            return CommonFunction.GetSuccessCode(new
            {
                player.AgentCode,
                player.LoginName,
                player.LanguageCode,
                player.Balance,
                player.CurrencyCode,
                player.NickName,
            });
        }

        public async Task<object> Transfer(string agentCode, string loginName, string transactionId, decimal amount)
        {
            try
            {
                string txId = await PlayerDAO.PerformTransfer(agentCode, loginName, transactionId, amount);
                if (string.IsNullOrEmpty(txId))
                {
                    _logger.LogInformation("Transfer报错:" + CommonFunction.GetErrorDesc(902));
                    return CommonFunction.GetErrorCode(902);
                }
            }
            catch (MySqlException ex)
            {
                if (CommonFunction.isContainErrorCode(ex.Number))
                {
                    _logger.LogInformation("Transfer报错:" + CommonFunction.GetErrorDesc(ex.Number));
                    return CommonFunction.GetErrorCode(ex.Number);
                }
                else
                {
                    _logger.LogInformation($"SQL事务失败: {ex.Message}");
                    return CommonFunction.GetErrorCode(206); // failed_transfer
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Transfer报错: {CommonFunction.GetErrorDesc(206)} - {ex.Message}");
                return CommonFunction.GetErrorCode(206); // failed_transfer
            }


            //get player again to get latest balance 
            Player player = await PlayerDAO.GetPlayer(agentCode, loginName);

            return CommonFunction.GetSuccessCode(new
            {
                TransactionAmount = amount,
                TransactionReference = transactionId,
                CurrentBalance = player.Balance
            });
        }

        public async Task<object> CheckTransferStatus(string agentCode, string loginName, string transactionId)
        {
            Transaction transaction = await PlayerDAO.GetTransactionByTransactionIdAndPlayer(transactionId, agentCode, loginName);
            if (transaction == null)
            {
                _logger.LogInformation("CheckTransferStatus报错:" + CommonFunction.GetErrorDesc(202));
                return CommonFunction.GetErrorCode(202);
            } //transaction_not_found

            return CommonFunction.GetSuccessCode(new
            {
                transaction.TransactionId,
                transaction.TransferType,
                transaction.Amount
            });
        }
    }
}
