using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class GameOption
    {
        public int Id { get; set; }
        public string AgentCode { get; set; }
        public string RoundId { get; set; }
        public int OptionType { get; set; }
        public string OptionCode { get; set; }
        public string OptionName { get; set; }
        public decimal Odds { get; set; }
        public int Status { get; set; }
        public int Rank { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
