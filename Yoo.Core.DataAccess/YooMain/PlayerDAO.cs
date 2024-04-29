using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;
using Yoo.Core.Common.Storage;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.DataAccess.YooMain
{
    public class PlayerDAO
    {
        #region Player
        public async static Task<Player> GetPlayer(string agentCode, string loginName)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "select * from Player where LoginName = @loginName and AgentCode = @agentCode LIMIT 1";
                var player = await db.QueryFirstOrDefaultAsync<Player>(query, new
                {
                    loginName,
                    agentCode
                });
                return player;
            }
        }

        public async static Task<bool> InsertPlayer(string agentCode, string loginName, string currencyCode, string ipAddress)
        {
            try
            {
                using (var db = DBContainer.DB.GetMainConnection())
                {
                    string query = @"INSERT INTO Player (AgentCode,LoginName,CurrencyCode,IpAddress,Status)
                                   VALUES(@agentCode,@loginName,@currencyCode,@ipAddress,@status)";
                    return await db.ExecuteAsync(query, new
                    {
                        agentCode,
                        loginName,
                        currencyCode,
                        ipAddress,
                        status = PlayerStatus.ACTIVE
                    }) > 0;
                };
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async static Task<bool> LoginPlayer(string agentCode, string loginName, string? nickName, string languageCode)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "UPDATE Player SET ";

                if (!string.IsNullOrEmpty(nickName))
                {
                    query += "Nickname = @Nickname,";
                }

                query += "LanguageCode = @LanguageCode, " +
                    "LastLoginTime = now(), " +
                    "UpdatedAt = now() " +
                    "WHERE LoginName = @loginName AND AgentCode = @agentCode";

                return await db.ExecuteAsync(query, new
                {
                    AgentCode = agentCode,
                    LoginName = loginName,
                    Nickname = nickName,
                    LanguageCode = languageCode,
                }) > 0;
            };
        }
        #endregion

        public async static Task<Transaction> GetTransactionByTransactionId(string transactionId)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM Transaction WHERE TransactionId = @TransactionId LIMIT 1";
                var transaction = await db.QueryFirstOrDefaultAsync<Transaction>(query, new
                {
                    TransactionId = transactionId
                });
                return transaction;
            }
        }

        public async static Task<Transaction> GetTransactionByTransactionIdAndPlayer(string transactionId, string agentCode, string loginName)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM Transaction WHERE TransactionId = @TransactionId AND AgentCode = @AgentCode AND LoginName = @LoginName LIMIT 1";
                var transaction = await db.QueryFirstOrDefaultAsync<Transaction>(query, new
                {
                    TransactionId = transactionId,
                    AgentCode = agentCode,
                    LoginName = loginName
                });
                return transaction;
            }
        }

        public async static Task<Transaction> GetAllTransactionByAgentCodeAndPlayer(string agentCode, string loginName)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "SELECT * FROM Transaction WHERE AgentCode = @AgentCode AND LoginName = @LoginName LIMIT 1";
                var transaction = await db.QueryFirstOrDefaultAsync<Transaction>(query, new
                {
                    
                    AgentCode = agentCode,
                    LoginName = loginName
                });
                return transaction;
            }
        }

        public async static Task<bool> InsertPlayerAuthLog(string agentCode, string loginName, string token, string platform)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string query = "INSERT INTO PlayerAuthLog (AgentCode, LoginName, Token, Platform) " +
                     "VALUES (@AgentCode, @LoginName, @Token, @Platform)";

                return await db.ExecuteAsync(query, new
                {
                    AgentCode = agentCode,
                    LoginName = loginName,
                    Token = token,
                    Platform = platform
                }) > 0;
            }
        }

        public async static Task<string> PerformTransfer(string agentCode, string loginName, string transactionId, decimal amount)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                return await db.ExecuteScalarAsync<string>("SpTransfer", new
                {
                    inAgentCode = agentCode,
                    inLoginName = loginName,
                    inTransactionRef = transactionId,
                    inAmount = amount,
                    inTransactionType = (amount > 0) ? TransactionType.TRANSFERIN : TransactionType.TRANSFEROUT,
                    inCreatedBy = "PerformTransfer"
                }, commandType: CommandType.StoredProcedure);
            };
        }
    }
}

