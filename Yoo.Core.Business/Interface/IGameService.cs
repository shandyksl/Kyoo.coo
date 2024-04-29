using Yoo.Core.Model.DbModels;
using System.Threading.Tasks;
using Yoo.Core.Model.DTO;
using System;
using System.Collections.Generic;

namespace Yoo.Core.Business.Interface
{
    public interface IGameService
    {
        Task<GameConfig> GetGameConfigData(string agentCode, string gameType);
        Task<object> GetGameLobby(string agentCode);
        Task<object> GetGameInfo(string agentCode, string gameCode, int page, int pageSize, string sort);
        Task <object> GetGameResult(string gameCode, string roundCode, DateTime fromDate, DateTime toDate, int page, int pageSize, string Sort);
    }
}
