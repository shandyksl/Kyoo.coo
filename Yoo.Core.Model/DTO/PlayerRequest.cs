using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DTO
{
    public class PlayerRequest
    {
        public string AgentCode { get; set; }
        public string LoginName { get; set; }
    }

    public class LoginRequest
    {
        public string AgentCode { get; set; }
        public string LoginName { get; set; }
        public string LanguageCode { get; set; }
        public string CurrencyCode { get; set; }
        public string NickName { get; set; }
        public string Platform { get; set; }
    }

    public class CreatePlayerRequest
    {
        public string AgentCode { get; set; }
        public string CurrencyCode { get; set; }
        public string LoginName { get; set; }
        public string IpAddress { get; set; }
    }
}
