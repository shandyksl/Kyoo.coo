using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;
using Yoo.Core.Model.DTO;

namespace Yoo.Core.Business.Interface
{
    public interface IOrderService
    {
        Task<object> PlaceBet(string agentCode, string loginName, string gameType, decimal betAmount, List<BetInfo> betInfo);
    }
}
