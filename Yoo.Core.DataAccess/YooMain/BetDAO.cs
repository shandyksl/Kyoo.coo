using Dapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Yoo.Core.Common.Storage;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;
using Yoo.Core.Model.DTO;

namespace Yoo.Core.DataAccess.YooMain
{
    public class BetDAO
    {
        public async static Task<List<BetHistory>> GetBetHistory(string agentCode, string loginName, string gameType, string gameCode, DateTime fromDate, DateTime toDate, int page, int pageSize, string sort)
        {
            int offset = (page - 1) * pageSize;
            DynamicParameters args = new DynamicParameters();

            using (var db = DBContainer.DB.GetMainConnection())
            {
                string sql =
                    "SELECT bh.*," +
                    " COUNT(*) OVER() as TotalCount, " +
                    "SUM(bh.BetAmount) OVER() AS TotalBetAmount, " +
                    "SUM(bh.WinAmount) OVER() AS TotalWinAmount, " +
                    "gi.GameIntro AS GameIntro " +
                    "FROM BetHistory bh " +
                    "LEFT JOIN GameInfo gi ON bh.RoundId = gi.RoundId " +
                    "WHERE bh.AgentCode = @AgentCode AND bh.CreatedAt BETWEEN @FromDate AND @ToDate";
                args.Add("@AgentCode", agentCode);

                if (!string.IsNullOrEmpty(loginName))
                {
                    sql += " AND bh.LoginName = @LoginName";
                    args.Add("@LoginName", loginName);
                }
                if (!string.IsNullOrEmpty(gameType))
                {
                    sql += " AND bh.GameType = @GameType";
                    args.Add("@GameType", gameType);
                }
                if (!string.IsNullOrEmpty(gameCode))
                {
                    sql += " AND bh.GameCode = @GameCode";
                    args.Add("@GameCode", gameCode);
                }
                sql += (sort == "ASC") ? " ORDER BY bh.Id ASC" : " ORDER BY bh.Id DESC";
                sql += " LIMIT @PageSize OFFSET @OffSet;";

                args.Add("@FromDate", fromDate);
                args.Add("@ToDate", toDate);
                args.Add("@Sorting", sort);
                args.Add("@PageSize", pageSize);
                args.Add("@OffSet", offset);

                var result = await db.QueryAsync<BetHistory>(sql, args);

                return result.ToList();
            }
        }

        public async static Task<BetHistory> GetSingleBetHistory(string agentCode, string loginName, string transactionId)
        {
            DynamicParameters args = new DynamicParameters();
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT bh.*, gi.GameIntro AS GameIntro FROM BetHistory bh " +
                    "LEFT JOIN GameInfo gi ON bh.RoundId = gi.RoundId " +
                    "WHERE bh.TransactionId = @TransactionId";
                args.Add("@TransactionId", transactionId);

                if (!string.IsNullOrEmpty(agentCode))
                {
                    query += " AND bh.AgentCode = @AgentCode";
                    args.Add("@AgentCode", agentCode);
                }
                if (!string.IsNullOrEmpty(loginName))
                {
                    query += " AND bh.LoginName = @LoginName";
                    args.Add("@LoginName", loginName);
                }

                query += " LIMIT 1";

                var result = await db.QueryFirstOrDefaultAsync<BetHistory>(query, args);

                return result;
            }
        }
        public async static Task<List<BetDetail>> GetBetDetail(int[] betHistoryIds)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM BetDetail WHERE BetHistoryId IN @BetHistoryIds";
                var result = await db.QueryAsync<BetDetail>(query, new
                {
                    BetHistoryIds = betHistoryIds
                });

                return result.ToList();
            }
        }

        public async static Task<string> PlaceBet(string agentCode,
                                                  string loginName,
                                                  string currencyCode,
                                                  string transactionId,
                                                  string gameType,
                                                  string gameCode,
                                                  string roundId,
                                                  decimal betAmount,
                                                  DateTime betTime,
                                                  string betDetailQuery,
                                                  string createdBy)
        {
            DynamicParameters args = new DynamicParameters();

            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = ("CALL SpPlaceBet(@AgentCode,@LoginName,@CurrencyCode,@TransactionId,@GameType,@GameCode,@RoundId,@BetAmount,@BetTime,@BetDetailQuery,@CreatedBy)");

                args.Add("@AgentCode", agentCode);
                args.Add("@LoginName", loginName);
                args.Add("@CurrencyCode", currencyCode);
                args.Add("@TransactionId", transactionId);
                args.Add("@GameType", gameType);
                args.Add("@GameCode", gameCode);
                args.Add("@RoundId", roundId);
                args.Add("@BetAmount", betAmount);
                args.Add("@BetTime", betTime);
                args.Add("@BetDetailQuery", betDetailQuery);
                args.Add("@CreatedBy", createdBy);

                return await db.QuerySingleAsync<string>(query, args);
            }
        }

        public async static Task<int> GetLastBetHistoryId()
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string squery = "SELECT IFNULL(MAX(Id),0) FROM BetHistory";
                return await db.QuerySingleAsync<int>(squery);
            }
        }
    }
}
