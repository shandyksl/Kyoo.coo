using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class GameInfo
    {
        public int Id { get; set; }
        public string GameType { get; set; }
        public string GameCode { get; set; }
        public string GameIntro { get; set; }
        public string RoundId { get; set; }
        public DateTime IssueStartTime { get; set; }
        public DateTime IssueEndTime { get; set; }
        public DateTime StartBuyTime { get; set; }
        public DateTime EndBuyTime { get; set; }
        public int State { get; set; }
        public decimal ResultPrice { get; set; }
        public string ResultInfo { get; set; }
        public DateTime ResultTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TotalCount { get; set; } //用于统计有多少数据

    }
}
