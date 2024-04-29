using Dapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yoo.Core.Common.Storage;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.DataAccess.YooMain
{
    public class GameDAO
    {
        public async static Task<List<GameInfo>> GetGameInfo(string gameType, int gameState, int page, int pageSize, string sort)
        {
            int offset = (page - 1) * pageSize;
            DynamicParameters args = new DynamicParameters();
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string sql = "SELECT *, COUNT(*) over() as TotalCount FROM GameInfo WHERE GameType = @GameType AND State = @GameState";
                sql += (sort == "ASC") ? " ORDER BY CreatedAt ASC" : " ORDER BY CreatedAt DESC";
                sql += " LIMIT @PageSize OFFSET @OffSet;";

                args.Add("@GameType", gameType);
                args.Add("@GameState", gameState);
                args.Add("@Sorting", sort);
                args.Add("@PageSize", pageSize);
                args.Add("@OffSet", offset);

                var result = await db.QueryAsync<GameInfo>(sql, args);

                return result.ToList();
            }
        }

        public async static Task<GameConfig> GetGameConfig(string agentCode, string gameType)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM GameConfig WHERE GameType = @GameType AND AgentCode = @AgentCode LIMIT 1";

                var result = await db.QueryFirstOrDefaultAsync<GameConfig>(query, new
                {
                    AgentCode = agentCode,
                    GameType = gameType,
                });

                return result;
            }
        }

        public async static Task<List<GameConfig>> GetAllGameConfigByAgentCode(string agentCode)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM GameConfig WHERE AgentCode = @AgentCode";

                var result = await db.QueryAsync<GameConfig>(query, new
                {
                    AgentCode = agentCode,

                });

                return result.ToList();
            }
        }

        public async static Task<List<GameOption>> GetAllGameOptionByRoundIds(string[] roundIds, string agentCode)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM GameOption WHERE RoundId IN @RoundIds AND AgentCode = @AgentCode";

                var result = await db.QueryAsync<GameOption>(query, new
                {
                    RoundIds = roundIds,
                    AgentCode = agentCode
                });

                return result.ToList();
            }
        }

        public async static Task<List<GameConfig>> GetGameLobby(string agentCode)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string squery = "SELECT * FROM GameConfig gc " +
                    "LEFT JOIN GameManagement gm ON gc.GameType = gm.GameType " +
                    "WHERE gc.AgentCode = @AgentCode " +
                    "AND gc.IsActive = 1 " +
                    "AND gm.IsActive = 1 " +
                    "AND ( (gm.MaintenanceStart IS NULL AND gm.MaintenanceEnd IS NULL) OR (NOW() NOT BETWEEN gm.MaintenanceStart AND gm.MaintenanceEnd) ) " +
                    "ORDER BY gc.Rank";
                var result = await db.QueryAsync<GameConfig>(squery, new { AgentCode = agentCode });
                return result.ToList();
            }
        }

        public async static Task<List<GameInfo>> GetGameResult(string gameType, string gameCode, DateTime fromDate, DateTime toDate, int page, int pageSize, string sort)
        {
            int offset = (page - 1) * pageSize;
            DynamicParameters args = new DynamicParameters();
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string sql = "SELECT *, COUNT(*) over() as TotalCount FROM GameInfo WHERE (ResultTime BETWEEN @FromDate AND @ToDate) AND State = @State";

                args.Add("@FromDate", fromDate);
                args.Add("@ToDate", toDate);
                args.Add("@State", GameState.END);
                args.Add("@Sorting", sort);
                args.Add("@PageSize", pageSize);
                args.Add("@OffSet", offset);

                if (!string.IsNullOrEmpty(gameType))
                {
                    sql += " AND GameType = @GameType";
                    args.Add("@GameType", gameType);
                }
                if (!string.IsNullOrEmpty(gameCode))
                {
                    sql += " AND GameCode = @GameCode";
                    args.Add("@GameCode", gameCode);
                }

                sql += (sort == "ASC") ? " ORDER BY Id ASC" : " ORDER BY Id DESC";
                sql += " LIMIT @PageSize OFFSET @OffSet;";

                var result = await db.QueryAsync<GameInfo>(sql, args);

                return result.ToList();
            }

        }

    }
}
