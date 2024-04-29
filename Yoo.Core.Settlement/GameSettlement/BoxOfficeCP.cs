using Yoo.Core.Common.Helper;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Settlement.GameSettlement
{
    public class BoxOfficeCP : BaseSettlement
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

                var bocpSettlePrice = await SettlementDAO.GetBOCPSettlePrice(GameCode, RoundId);
                if (bocpSettlePrice == 0)
                {
                    return CommonFunction.GetErrorCode(109);
                }

                var optionValue = await SettlementDAO.GetOptionValue(RoundId);
                if (optionValue == 0)
                {
                    return CommonFunction.GetErrorCode(109);
                }
                await SettlementDAO.UpdateBOAndEventGameInfoResult(RoundId);

                decimal totalWinAmount = 0;

                foreach (var detail in betDetailObj)
                {
                    if (detail.Status == 1)
                    {
                        decimal winAmount = CalculateWinAmount(detail, optionValue, bocpSettlePrice);
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
        private decimal CalculateWinAmount(BetDetail betDetail, decimal optionValue, decimal bocpSettlePrice)
        {
            decimal winAmount = 0;

            switch (betDetail.BetType)
            {
                case 4:
                    if (betDetail.BetCode == "greater")
                    {
                        if (bocpSettlePrice > optionValue)
                        {
                            winAmount = betDetail.BetAmount * betDetail.Odds;
                        }
                        else
                        {
                            winAmount = (-betDetail.BetAmount);
                        }
                    }
                    else if(betDetail.BetCode == "smaller")
                    {
                        if (bocpSettlePrice < optionValue)
                        {
                            winAmount = betDetail.BetAmount * betDetail.Odds;
                        }
                        else
                        {
                            winAmount = (-betDetail.BetAmount);
                        }
                    }
                    break;

                default:
                    break;
            }

            return winAmount;
        }
    }

}
