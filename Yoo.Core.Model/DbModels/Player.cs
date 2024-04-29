using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoo.Core.Model.Common;

namespace Yoo.Core.Model.DbModels
{
    public class Player
    {
        public int Id { get; set; }
        public string AgentCode { get; set; }
        public string LoginName { get; set; }
        public string? NickName { get; set; }
        public decimal Balance { get; set; }
        public string IpAddress { get; set; }
        public string LanguageCode { get; set; }
        public string CurrencyCode { get; set; }
        public int Status { get; set; }
        public decimal MaxBetAmount { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastActiveTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
