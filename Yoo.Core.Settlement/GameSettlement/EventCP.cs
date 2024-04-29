using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoo.Core.Common.Helper;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Settlement.GameSettlement
{
    public class EventCP : BaseSettlement
    {
        public async Task<object> BOCPSettlement(string GameType, string RoundId, string GameCode)
        {
            if (await SettlementDAO.GameInfoResult(RoundId))
            {
                var betDetailObj = await GetAllByRoundId(GameType, RoundId, GameCode);
                if (betDetailObj == null || !betDetailObj.Any())
                {
                    return CommonFunction.GetErrorCode(109);
                }

                var eventCPResultInfo = await SettlementDAO.GetEventCPResultInfo(GameCode, RoundId);
                if (eventCPResultInfo == null)
                {
                    return CommonFunction.GetErrorCode(109);
                }

                var getOptionName = await SettlementDAO.GetOptionNames(RoundId);
                if (getOptionName == null)
                {
                    return CommonFunction.GetErrorCode(109);
                }
                await SettlementDAO.UpdateBOAndEventGameInfoResult(RoundId);
                decimal totalWinAmount = 0;

                foreach (var detail in betDetailObj)
                {
                    if (detail.Status == 1)
                    {
                        decimal winAmount = CalculateWinAmount(detail, getOptionName, eventCPResultInfo);
                        if (winAmount != 0)
                        {
                            await SettlementDAO.UpdateWinBetDetailSettlement(detail.Id, detail.BetHistoryId, winAmount);
                            Console.WriteLine("winAmount :" + winAmount);

                        }
                    }
                    else
                    {
                        Console.WriteLine("The status is 2 already！");
                    }
                }

                foreach (var betHistoryId in betDetailObj.Select(d => d.BetHistoryId).Distinct())
                {
                    if (await SettlementDAO.AllBetHistorySettled(betHistoryId))
                    {
                        if (await SettlementDAO.AllBetDetailSettled(betHistoryId))
                        {
                            object totalWinAmountObject = await SettlementDAO.TotalWinAmount(betHistoryId);
                            if (totalWinAmountObject == null)
                            {
                                return CommonFunction.GetErrorCode(109);
                            }
                            totalWinAmount = Convert.ToDecimal(totalWinAmountObject);
                            await SettlementDAO.UpdateBetHistorySettlement(betHistoryId, totalWinAmount);
                        }
                        else
                        {
                            Console.WriteLine("betdetails haven finish calculate, cannot procees the total win amount！");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The status is 3 already！");
                    }
                }
                return totalWinAmount;
            }
            else
            {
                Console.WriteLine(RoundId + ": State is 4！");
                return null;
            }
        }
        private decimal CalculateWinAmount(BetDetail betDetail, List<string> getOptionName, string eventCPResultInfo)
        {
            decimal winAmount = 0;
            var resultInfoObject = JObject.Parse(eventCPResultInfo);

            foreach (var kvp in resultInfoObject)
            {

                if (getOptionName.Contains(kvp.Value.ToString()))
                {

                    if (betDetail.BetType == 5 && (betDetail.BetCode == kvp.Key))
                    {
                        winAmount = betDetail.BetAmount * betDetail.Odds;
                        break; 
                    }
                    else
                    {
                        winAmount = (-betDetail.BetAmount);
                    }
                }
            }

            return winAmount;
        }

    }
}
