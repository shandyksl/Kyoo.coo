using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class PlayerAuthLog
    {
        public int Id { get; set; }
        public string AgentCode { get; set; }
        public string LoginName { get; set; }
        public string Token { get; set; }
        public string Platform { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
