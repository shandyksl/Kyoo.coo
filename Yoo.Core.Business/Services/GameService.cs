using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.Common.Storage;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Business.Services
{
    public class GameService : IGameService
    {
        private readonly ILogger<GameService> _logger;
        private readonly IDatabase _redis;

        public GameService(ILogger<GameService> logger, RedisHelper client)
        {
            _logger = logger;
            _redis = client.GetDatabase();
        }

        public async Task<GameConfig> GetGameConfigData(string agentCode, string gameType)
        {
            string redisKey = "GameConfigData:" + agentCode + ":" + gameType;
            var result = await _redis.StringGetAsync(redisKey);

            if (result == RedisValue.Null)
            {
                var gameConfig = await GameDAO.GetGameConfig(agentCode, gameType);
                if (gameConfig == null) return null;
                await _redis.StringSetAsync(redisKey, JsonConvert.SerializeObject(gameConfig), TimeSpan.FromSeconds(300));
                return gameConfig;
            }
            else
            {
                return JsonConvert.DeserializeObject<GameConfig>(result);
            }
        }

        public async Task<object> GetGameLobby(string agentCode)
        {
            string redisKey = "GameLobbyData:" + agentCode;
            var result = await _redis.StringGetAsync(redisKey);
            List<GameConfig> gameLobby = new List<GameConfig>();

            if (result == RedisValue.Null)
            {
                gameLobby = await GameDAO.GetGameLobby(agentCode);
                if (gameLobby == null)
                {
                    _logger.LogInformation("GetGameLobby报错:" + CommonFunction.GetErrorDesc(115));
                    return CommonFunction.GetErrorCode(115); //gameconfig_not_found
                }

                await _redis.StringSetAsync(redisKey, JsonConvert.SerializeObject(gameLobby), TimeSpan.FromSeconds(5));
            }
            else
            {
                gameLobby = JsonConvert.DeserializeObject<List<GameConfig>>(result);
            }

            var returnValue = gameLobby.Select(item =>
            {
                return new
                {
                    item.GameName,
                    item.GameType,
                    item.GameBadge,
                    item.Rank,
                };
            }).ToList();

            return CommonFunction.GetSuccessCode(returnValue);
        }

        public async Task<object> GetGameInfo(string agentCode, string gameType, int page, int pageSize, string sort)
        {
            var result = await GameDAO.GetGameInfo(gameType, GameState.OPEN, page, pageSize, sort);

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

            GameConfig gameConfig = new GameConfig();
            List<GameOption> gameOptions = new List<GameOption>();
            var roundIds = result.Select(r => r.RoundId).ToArray();
            int totalCount = result.First().TotalCount;
            dynamic gameSetting = new ExpandoObject();

            if (gameType == GameType.CRYPTOFFC || gameType == GameType.STOCKCP)
            {
                gameConfig = await GetGameConfigData(agentCode, gameType);
                gameSetting = gameConfig.GameSettings;
                if (CommonFunction.isValidJson(gameConfig.GameSettings))
                {
                    gameSetting = JsonConvert.DeserializeObject(gameConfig.GameSettings);
                }
            }
            else if (gameType == GameType.BOXOFFICECP || gameType == GameType.EVENTCP)
            {
                gameOptions = await GameDAO.GetAllGameOptionByRoundIds(roundIds, agentCode);
            }

            var game = result.Select(item =>
            {
                if (gameType == GameType.BOXOFFICECP || gameType == GameType.EVENTCP)
                {
                    gameSetting = gameOptions
                        .Where(obj => obj.RoundId == item.RoundId)
                        .Select(obj => (dynamic) new
                        {
                            obj.OptionType,
                            obj.OptionCode,
                            obj.OptionName,
                            obj.Odds,
                            obj.Rank,
                        }).ToList();
                }

                return new
                {
                    item.GameType,
                    item.GameCode,
                    item.GameIntro,
                    item.RoundId,
                    item.ResultPrice,
                    item.IssueStartTime,
                    item.IssueEndTime,
                    Options = gameSetting,
                    item.State,
                    item.StartBuyTime,
                    item.EndBuyTime,
                };
            }).ToList();

            return CommonFunction.GetSuccessCode(new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Records = game
            });
        }

        public async Task<object> GetGameResult(string gameType, string gameCode, DateTime fromDate, DateTime toDate, int page, int pageSize, string Sort)
        {
            var result = await GameDAO.GetGameResult(gameType, gameCode, fromDate, toDate, page, pageSize, Sort);

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

            var res = result.Select(item =>
            {
                dynamic resultInfo = item.ResultInfo;
                if (!string.IsNullOrEmpty(item.ResultInfo) && CommonFunction.isValidJson(item.ResultInfo))
                {
                    resultInfo = JsonConvert.DeserializeObject(item.ResultInfo);
                }

                return new
                {
                    item.GameType,
                    item.GameCode,
                    item.GameIntro,
                    item.RoundId,
                    item.IssueStartTime,
                    item.IssueEndTime,
                    item.StartBuyTime,
                    item.EndBuyTime,
                    item.ResultPrice,
                    ResultInfo = resultInfo,
                    item.ResultTime,
                };
            }).ToList();
            int totalCount = result.First().TotalCount;

            return CommonFunction.GetSuccessCode(new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Records = res
            });

        }
    }
}
