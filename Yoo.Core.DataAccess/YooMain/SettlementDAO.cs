using Dapper;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Yoo.Core.Common.Storage;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.DataAccess.YooMain
{
    public class SettlementDAO
    {

        public async static Task<GameInfo> GetEndBuyTime(string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT EndBuyTime FROM GameInfo WHERE RoundId = @roundId LIMIT 1";

                var result = await db.QueryFirstOrDefaultAsync<GameInfo>(query, new
                {
                    RoundId = roundId,

                });

                return result;
            }
        }


        public async static Task<string> GetSettlePrice(string gameCode, string sqlFormattedDate)
        {
            using (var db = DBContainer.DB.GetAssetDbConnection())
            {
                string query = "SELECT Price FROM AssetPrice WHERE Symbol = @GameCode AND CreatedAt <= @SqlFormattedDate LIMIT 1";

                var result = await db.QueryFirstOrDefaultAsync<string>(query, new
                {
                    GameCode = gameCode,
                    SqlFormattedDate = sqlFormattedDate
                });

                return result;
            }
        }

        public async static Task<List<BetHistory>> GetAllByRoundId(string gameType, string gameCode, string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM BetHistory WHERE GameType = @GameType AND GameCode =@GameCode AND RoundId = @RoundId";

                var result = await db.QueryAsync<BetHistory>(query, new
                {
                    GameType = gameType,
                    RoundId = roundId,
                    GameCode = gameCode,
                });

                return result.ToList();
            }
        }


        public async static Task<List<BetDetail>> GetBetDetailById(int betHistoryId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM BetDetail WHERE BetHistoryId = @BetHistoryId ";

                var result = await db.QueryAsync<BetDetail>(query, new
                {
                    BetHistoryId = betHistoryId,
                });

                return result.ToList();
            }
        }


        public async static Task<List<BetDetail>> UpdateWinBetDetailSettlement(int detailId,int betHistoryId, decimal winAmount)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "UPDATE BetDetail SET WinAmount = @WinAmount, Status = 2  WHERE Id =@DetailId AND BetHistoryId = @BetHistoryId";

                var result = await db.QueryAsync<BetDetail>(query, new
                {
                    DetailId = detailId,    
                    BetHistoryId = betHistoryId,
                    WinAmount = winAmount
                });
                Console.WriteLine("Updated Win BetDetail Settlement!!");
                return result.ToList();
            }
        }
        public async static Task<List<BetDetail>> UpdateLossBetDetailSettlement(int detailId, int betHistoryId, decimal winAmount)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "UPDATE BetDetail SET Status = 2  WHERE Id =@DetailId AND BetHistoryId = @BetHistoryId";

                var result = await db.QueryAsync<BetDetail>(query, new
                {
                    DetailId = detailId,
                    BetHistoryId = betHistoryId,
                });
                Console.WriteLine("Updated Loss BetDetail Settlement!!");
                return result.ToList();
            }
        }

        public async static Task<bool> AllBetDetailSettled(int betHistoryId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT COUNT(*) FROM BetDetail WHERE BetHistoryId = @BetHistoryId AND Status = 1";
                var result = await db.ExecuteScalarAsync<int>(query, new { BetHistoryId = betHistoryId });
                return result == 0;
            }
        }
        public async static Task<bool> AllBetHistorySettled(int betHistoryId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT COUNT(*) FROM BetHistory WHERE Id = @BetHistoryId AND SettleStatus = 3";
                var result = await db.ExecuteScalarAsync<int>(query, new { BetHistoryId = betHistoryId });
                return result == 0;
            }
        }

        public async static Task<bool> GameInfoResult(string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM GameInfo WHERE RoundId = @RoundId AND State = 4";
                var result = await db.ExecuteScalarAsync<string>(query, new { RoundId = roundId });
                return result == null;
            }
        }
        public async static Task<decimal> TotalWinAmount(int betHistoryId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT SUM(WinAmount) FROM BetDetail WHERE BetHistoryId = @BetHistoryId AND Status = 2";

                var result = await db.ExecuteScalarAsync<decimal>(query, new
                {
                    BetHistoryId = betHistoryId,
                });
                Console.WriteLine("SELECTED TOTAL WIN AMOUNT !!");
                return result;
            }
        }

        public async static Task UpdateBetHistorySettlement(int betHistoryId, decimal totalWinAmount)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "UPDATE BetHistory SET WinAmount = @totalWinAmount, SettleStatus = 3 WHERE Id = @BetHistoryId";

                await db.ExecuteAsync(query, new
                {
                    BetHistoryId = betHistoryId,
                    TotalWinAmount = totalWinAmount
                });
                Console.WriteLine("Updated BetHistory Settlement!!");
            }
        }

        public async static Task<decimal> GetOptionValue(string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT OptionValue FROM GameOption WHERE RoundId = @RoundId LIMIT 1";

                var result = await db.QueryFirstOrDefaultAsync<decimal>(query, new
                {
                    RoundId = roundId,
                });

                return result;
            }
        }


        public async static Task<decimal> GetBOCPSettlePrice(string gameCode, string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT ResultPrice FROM GameInfo WHERE GameCode = @GameCode AND RoundId = @RoundId LIMIT 1";

                var result = await db.QueryFirstOrDefaultAsync<decimal>(query, new
                {
                    GameCode = gameCode,
                    RoundId = roundId
                });

                return result;
            }
        }
        public async static Task<string> GetEventCPResultInfo(string gameCode, string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT ResultInfo FROM GameInfo WHERE GameCode = @GameCode AND RoundId = @RoundId LIMIT 1";

                var result = await db.QueryFirstOrDefaultAsync<string>(query, new
                {
                    GameCode = gameCode,
                    RoundId = roundId
                });

                return result;
            }
        }

        public async static Task<List<string>> GetOptionNames(string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT OptionName FROM GameOption WHERE RoundId = @RoundId";

                var results = await db.QueryAsync<string>(query, new
                {
                    RoundId = roundId,
                });

                return results.ToList();
            }
        }
        public async static Task UpdateCryptoAndStockGameInfoResult(object price,string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "UPDATE GameInfo SET ResultPrice = @Price, State = 4 WHERE RoundId = @roundId";

                await db.ExecuteAsync(query, new
                {
                    Price = price,
                    RoundId= roundId
                });
                Console.WriteLine("Updated Crypto And StockGameInfo Result !!");
            }
        }

        public async static Task UpdateBOAndEventGameInfoResult( string roundId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "UPDATE GameInfo SET State = 4 WHERE RoundId = @roundId";

                await db.ExecuteAsync(query, new
                {
                    RoundId = roundId
                });
                Console.WriteLine("Updated BO And Event GameInfo Result !!");
            }
        }
    }

}
