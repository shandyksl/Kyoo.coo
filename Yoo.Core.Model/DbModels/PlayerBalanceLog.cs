using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class PlayerBalanceLog
    {
        public int Id { get; set; }
        public string AgentCode { get; set; }
        public string LoginName { get; set; }
        public decimal AfterBalance { get; set; }
        public decimal BeforeBalance { get; set; }
        public decimal TransactionAmount { get; set; }
        public int TransactionType { get; set; }
        public string TransactionReference { get; set; }
        public string CurrencyCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}
