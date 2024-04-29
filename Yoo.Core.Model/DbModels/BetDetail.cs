using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class BetDetail
    {
        public int Id { get; set; }                
        public int BetHistoryId { get; set; }   //投注记录ID
        public int BetType { get; set; }        //下注选项类型
        public string BetCode { get; set; }        //下注选择
        public decimal Odds { get; set; }          //赔率
        public decimal BetAmount { get; set; }     //投注金额
        public decimal WinAmount { get; set; }     //结算金额
        public string BetResult { get; set; }      //投注结果
        public int Status { get; set; }            //1-未结算,2-已结算
        public DateTime CreatedAt { get; set; }    //创建时间
        public DateTime? UpdatedAt { get; set; }   //更新时间
    }
}