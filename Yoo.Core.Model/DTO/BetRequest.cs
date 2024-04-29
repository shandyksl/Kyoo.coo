using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoo.Core.Model.Common;

namespace Yoo.Core.Model.DTO
{
    public class BetHistoryRequest
    {
        public string AgentCode { get; set; }
        public string GameType { get; set; }
        public string GameCode { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int Sort { get; set; } = Sorting.ASC;
    }

    public class BetDetailRequest
    {
        public string AgentCode { get; set; }
        public string TransactionId { get; set; }
    }
}
