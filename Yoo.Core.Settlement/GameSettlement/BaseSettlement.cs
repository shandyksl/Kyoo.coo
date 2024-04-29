using System;
using Yoo.Core.Common.Helper;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Settlement.GameSettlement
{
	public class BaseSettlement
	{
        public async Task<List<BetDetail>> GetAllByRoundId(string GameType, string RoundId, string GameCode)
        {
            var game = await SettlementDAO.GetAllByRoundId(GameType, GameCode, RoundId);
            if (game == null)
            {
                return new List<BetDetail> { CommonFunction.GetErrorCode(109) }; // 返回包含错误代码的列表
            }
            var resultList = new List<BetDetail>();
            foreach (var item in game)
            {
                var betDetails = await SettlementDAO.GetBetDetailById(item.Id);
                foreach (var detail in betDetails)
                {
                    resultList.Add(detail);
                }
            }
            return resultList;
        }
    }
}

