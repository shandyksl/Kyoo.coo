using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Model.DTO
{
    public class PlaceBetRequest
    {
        public string GameType { get; set; } //CryptoFFC
        public decimal TotalBetAmount { get; set; }
        public List<BetInfo> BetInfo { get; set; }

    }

    public class BetInfo
    {
        public string GameCode { get; set; } //BTC
        public string RoundId { get; set; }
        public decimal BetAmount { get; set; }
        public List<BetOption> BetOptions { get; set; }
    }

    public class BetOption
    {
        public int BetType { get; set; }
        public string BetCode { get; set; }
        public string Odds { get; set; }
        public decimal BetAmount { get; set; }
    }
}
