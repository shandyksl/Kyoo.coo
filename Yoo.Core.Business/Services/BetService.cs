using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Business.Services
{
    public class BetService : IBetService
    {
        private readonly ILogger<BetService> _logger;
        private readonly IGameService _gameService;

        public BetService(ILogger<BetService> logger, IGameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }

        public async Task<object> GetBetHistory(string agentCode, string loginName, string gameType, string gameCode, DateTime fromDate, DateTime toDate, int page, int pageSize, string sort)
        {
            if (!string.IsNullOrEmpty(loginName))
            {
                var player = await PlayerDAO.GetPlayer(agentCode, loginName);
                if (player == null)
                {
                    _logger.LogInformation("GetBetHistory报错:" + CommonFunction.GetErrorDesc(106));
                    return CommonFunction.GetErrorCode(106); //player_not_found
                }
            }
            var result = await BetDAO.GetBetHistory(agentCode, loginName, gameType, gameCode, fromDate, toDate, page, pageSize, sort);

            //if empty then return
            if (result.Count == 0)
            {
                return CommonFunction.GetSuccessCode(new
                {
                    TotalCount = 0,
                    TotalPages = 0,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Records = result
                });
            }

            List<GameConfig> gameConfigs = new List<GameConfig>();
            List<GameOption> gameOptions = new List<GameOption>();
            List<BetDetail> betDetailList = new List<BetDetail>();
            var roundIds = result.Select(r => r.RoundId).ToArray();

            gameConfigs = await GameDAO.GetAllGameConfigByAgentCode(agentCode);
            gameOptions = await GameDAO.GetAllGameOptionByRoundIds(roundIds, agentCode);

            var betHistoryIds = result.Select(r => r.Id).ToArray();
            int totalCount = result.First().TotalCount;
            decimal totalBetAmount = result.First().TotalBetAmount;
            decimal totalWinAmount = result.First().TotalWinAmount;

            betDetailList = await BetDAO.GetBetDetail(betHistoryIds);

            var betHistory = result.Select(item =>
            {
                List<GameOption> gameOptionsByBetHistory = new List<GameOption>();

                if (item.GameType == GameType.CRYPTOFFC || item.GameType == GameType.STOCKCP)
                {
                    var gameConfig = gameConfigs.Where(o => o.GameType == item.GameType).FirstOrDefault();

                    JArray gameSetting = JArray.Parse(gameConfig.GameSettings);
                    gameOptionsByBetHistory = gameSetting.Select(option => new GameOption
                    {
                        OptionCode = (string)option["OptionCode"],
                        OptionName = (string)option["OptionName"]
                    }).ToList();
                }
                else if (item.GameType == GameType.BOXOFFICECP || item.GameType == GameType.EVENTCP)
                {
                    gameOptionsByBetHistory = gameOptions.Where(obj => obj.RoundId == item.RoundId).ToList();
                }

                var betDetail = betDetailList
                    .Where(obj => obj.BetHistoryId == item.Id)
                    .Select(obj =>
                    {
                        var option = gameOptionsByBetHistory.FirstOrDefault(o => o.OptionCode == obj.BetCode);

                        return new
                        {
                            obj.BetType,
                            obj.BetCode,
                            OptionName = (option != null) ? option.OptionName : "",
                            obj.Odds,
                            obj.BetAmount,
                            obj.WinAmount,
                            obj.BetResult
                        };
                    }).ToList();

                return new
                {
                    item.GameType,
                    item.GameCode,
                    item.TransactionId,
                    item.RoundId,
                    item.GameIntro,
                    item.BetAmount,
                    item.WinAmount,
                    item.BetTime,
                    BetDetail = betDetail,
                    item.SettleStatus,
                    item.SettleTime
                };
            }).ToList();

            return CommonFunction.GetSuccessCode(new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                TotalBetAmount = totalBetAmount,
                TotalWinAmount = totalWinAmount,
                Records = betHistory
            });
        }

        public async Task<object> GetBetDetail(string agentCode, string? loginName, string transactionId)
        {
            if (!string.IsNullOrEmpty(loginName))
            {
                var player = await PlayerDAO.GetPlayer(agentCode, loginName);
                if (player == null) return CommonFunction.GetErrorCode(106); //player_not_found
            }

            var result = await BetDAO.GetSingleBetHistory(agentCode, loginName, transactionId);

            if (result == null)
            {
                _logger.LogInformation("GetBetHistory报错:" + CommonFunction.GetErrorDesc(401));
                return CommonFunction.GetErrorCode(401); //betdetail_not_found
            }

            int[] historyIds = { result.Id };
            string[] roundIds = { result.RoundId };
            var betDetail = await BetDAO.GetBetDetail(historyIds);
            List<GameOption> gameOptionsByBetHistory = new List<GameOption>();

            if (betDetail.Count == 0)
            {
                _logger.LogInformation("GetBetHistory报错:" + CommonFunction.GetErrorDesc(401));
                return CommonFunction.GetErrorCode(401); //betdetail_not_found
            }

            if (result.GameType == GameType.CRYPTOFFC || result.GameType == GameType.STOCKCP)
            {
                var gameconfig = await _gameService.GetGameConfigData(result.AgentCode, result.GameType);

                JArray gameSettings = JArray.Parse(gameconfig.GameSettings);
                var game = gameSettings.FirstOrDefault(o => (string)o["GameCode"] == result.GameCode);

                if (game != null)
                {
                    var options = game["GameOptions"];

                    gameOptionsByBetHistory = options.Select(option => new GameOption
                    {
                        OptionCode = (string)option["OptionCode"],
                        OptionName = (string)option["OptionName"]
                    }).ToList();
                }
            }
            else if (result.GameType == GameType.BOXOFFICECP || result.GameType == GameType.EVENTCP)
            {
                var gameOptions = await GameDAO.GetAllGameOptionByRoundIds(roundIds, result.AgentCode);
                gameOptionsByBetHistory = gameOptions.Where(obj => obj.RoundId == result.RoundId).ToList();
            }

            var betDetailList = betDetail.Select(obj =>
            {
                var option = gameOptionsByBetHistory.FirstOrDefault(o => o.OptionCode == obj.BetCode);

                return new
                {
                    obj.BetType,
                    obj.BetCode,
                    OptionName = (option != null) ? option.OptionName : "",
                    obj.Odds,
                    obj.BetAmount,
                    obj.WinAmount,
                    obj.BetResult
                };
            }).ToList();

            return CommonFunction.GetSuccessCode(new
            {
                result.GameType,
                result.GameCode,
                result.TransactionId,
                result.RoundId,
                result.GameIntro,
                result.BetAmount,
                result.WinAmount,
                result.BetTime,
                BetDetail = betDetailList,
                result.SettleStatus,
                result.SettleTime
            });
        }
    }
}
