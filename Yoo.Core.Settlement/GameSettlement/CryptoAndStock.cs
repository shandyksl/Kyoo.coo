using Yoo.Core.Common.Helper;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Settlement.GameSettlement
{
    public class CryptoAndStock : BaseSettlement
    {
        public async Task<string> GetSettlePrice(string GameType, string RoundId, string GameCode)
        {
            if (await SettlementDAO.GameInfoResult(RoundId))
            {
                var game = await SettlementDAO.GetEndBuyTime(RoundId);
                if (game == null)
                {
                    return CommonFunction.GetErrorCode(109); //agent not found
                }
                DateTime EndBuyTime = game.EndBuyTime;
                DateTime SettleTime = EndBuyTime.AddMinutes(5);
                string sqlFormattedDate = SettleTime.ToString("yyyy-MM-dd HH:mm:ss");
                var price = await SettlementDAO.GetSettlePrice(GameCode, sqlFormattedDate);
                await SettlementDAO.UpdateCryptoAndStockGameInfoResult(price, RoundId);


                if (price == null)
                {
                    return CommonFunction.GetErrorCode(109); //agent not found
                }
                else
                {
                    return price;
                }
            }
            else
            {
                Console.WriteLine(RoundId +": State is 4！");
                return null;
            }
        }

        public async Task<object> CryptoSettlement(string GameType, string RoundId, string GameCode)
        {
            decimal totalWinAmount = 0;

            var betDetailObj = await GetAllByRoundId(GameType, RoundId, GameCode);
            if (betDetailObj == null || !betDetailObj.Any())
            {
                return CommonFunction.GetErrorCode(109);
            }
            var settlePrice = await GetSettlePrice(GameType, RoundId, GameCode);
            if (settlePrice == null)
            {
                return CommonFunction.GetErrorCode(109);
            }
            
               
            foreach (var detail in betDetailObj)
            {
                if (detail.Status == 1)
                {
                    decimal winAmount = CalculateWinAmount(detail, settlePrice);
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

        private decimal CalculateWinAmount(BetDetail betDetail, string settlePrice)
        {
            decimal winAmount = 0;
            char lastDigit = settlePrice[settlePrice.Length - 1];
            Console.WriteLine("lastDigit！" + lastDigit);
            switch (betDetail.BetType)
                {

                    case 1:
                        if (betDetail.BetCode == "small")
                        {
                            if (lastDigit <= 4)
                            {
                                winAmount = betDetail.BetAmount * betDetail.Odds;

                            }
                            else
                            {
                                winAmount = (-betDetail.BetAmount);
                            }
                        }
                        else if (betDetail.BetCode == "big")
                        {
                            if (lastDigit > 4)
                            {
                                winAmount = betDetail.BetAmount * betDetail.Odds;

                            }
                            else
                            {
                                winAmount = (-betDetail.BetAmount);
                            }
                        }

                        break;

                    case 2:
                        if (betDetail.BetCode == "single")
                        {
                            if (lastDigit == 1 || lastDigit == 3 || lastDigit == 5 || lastDigit == 7 || lastDigit == 9)
                            {
                                winAmount = betDetail.BetAmount * betDetail.Odds;

                            }
                            else
                            {
                                winAmount = (-betDetail.BetAmount);
                            }
                        }
                        else if (betDetail.BetCode == "double")
                        {
                            if (lastDigit == 0 || lastDigit == 2 || lastDigit == 4 || lastDigit == 6 || lastDigit == 8)
                            {
                                winAmount = betDetail.BetAmount * betDetail.Odds;

                            }
                            else
                            {
                                winAmount = (-betDetail.BetAmount);
                            }
                        }

                        break;

                    case 3:
                        if (int.Parse(betDetail.BetCode) == lastDigit)
                        {
                            winAmount = betDetail.BetAmount * betDetail.Odds;

                        }
                        else
                        {
                            winAmount = (-betDetail.BetAmount);
                        }
                        break;

                    default:

                        break;
                


            }
            return winAmount;
        }
    }
}
