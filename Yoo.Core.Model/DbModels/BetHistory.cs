using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Model.DbModels
{
    public class BetHistory
    {
        public int Id { get; set; }
        public string AgentCode { get; set; }       //代理
        public string LoginName { get; set; }       //登录名
        public string GameType { get; set; }        //游戏分类
        public string GameCode { get; set; }        //游戏代号
        public string TransactionId { get; set; }   //订单号
        public string RoundId { get; set; }         //期号
        public decimal BetAmount { get; set; }      //投注金额
        public DateTime BetTime { get; set; }      //投注时间
        public decimal WinAmount { get; set; }      //结算金额
        public int SettleStatus { get; set; }       //1-未结算,2-结算中,3-已结算
        public DateTime? SettleTime { get; set; }   //结算时间
        public DateTime CreatedAt { get; set; }     //创建时间
        public DateTime? UpdatedAt { get; set; }    //更新时间

        // 以下都不是 BetHistory 数据表里的
        public int TotalCount { get; set; } //用于统计有多少数据
        public decimal TotalBetAmount { get; set; } //用于统计总投注
        public decimal TotalWinAmount { get; set; } //用于统计总输赢
        public string GameIntro { get; set; } // 游戏名称
    }
}
