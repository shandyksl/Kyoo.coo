using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class Transaction
    {
        public int Id { get; set; }
        public string AgentCode { get; set; }
        public string LoginName { get; set; }
        public int TransferType { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
