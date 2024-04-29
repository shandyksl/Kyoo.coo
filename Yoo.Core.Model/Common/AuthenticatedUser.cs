using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.Common
{
    public class AuthenticatedUser
    {
        public string LoginName { get; set; }
        public string AgentCode { get; set; }
        public string CurrencyCode { get; set; }
    }
}
