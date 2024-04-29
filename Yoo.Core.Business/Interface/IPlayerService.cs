using Yoo.Core.Model.DbModels;
using Yoo.Core.Model.DTO;
using System.Threading.Tasks;
using System;

namespace Yoo.Core.Business.Interface
{
    public interface IPlayerService
    {
        Task<object> CreatePlayer(string agentCode, string loginName, string currencyCode, string ipAddress);
        Task<Player?> GetPlayerByLoginNameAgentCode(string agentCode, string loginName);
        Task<object> LoginPlayer(string agentCode, string loginName, string languageCode, string currencyCode, string nickName, string platform);
        Task<object> GetPlayerBalance(string agentCode, string loginName);
        Task<object> GetPlayerInfo(string agentCode, string loginName);

        Task<object> Transfer(string agentCode, string loginName, string transactionId, decimal amount);
        Task<object> CheckTransferStatus(string agentCode, string loginName, string transactionId);
    }
}
