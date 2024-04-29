using Yoo.Core.Model.DTO;
using System.Threading.Tasks;
using System;

namespace Yoo.Core.Business.Interface
{
    public interface IBetService
    {
        Task<object> GetBetHistory(string agentCode, string loginName,string gameType, string gameCode, DateTime fromDate, DateTime toDate, int page, int pageSize, string sort);

        Task<object> GetBetDetail(string agentCode, string loginName, string transactionId);
    }
}
