using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Common.Helper
{
    public class ServiceHelper
    {
        private static Dictionary<string, string> LOG_API_URLS = new Dictionary<string, string>()
        {
            { "log_agent_sdk" , "/log/vendoragentsdkrequest" },
            {"log_agent_login", "/log/agentlogin" },
            {"log_vendor_api", "/log/vendorapirequest" },
            {"log_request_api_failed", "/log/apirequestfailedlog" },
            {"notify_bethistory_failed", "/log/notifybethistorylog" },
            {"log_player_ip","/log/playeriplog" },
            {"log_player_uniqueid","/log/playeruqidlog" },
            {"log_mw_failed_transfer" , "/log/mw/transferfaillog" }
        };
        public static string GetLogAPiURL(string funName)
        {
            var loggerURL = Appsettings.app(new string[] { "ServiceURL", "LoggerAPI" });
            return $"{loggerURL}{LOG_API_URLS[funName]}";
        }

        private static Dictionary<string, string> CALC_API_URLS = new Dictionary<string, string>()
        {
            {"getcalvaluedata", "/cal/getcalvaluedata" }
        };

        public static string GetCalcAPiURL(string funName)
        {
            var calcuURL = Appsettings.app(new string[] { "ServiceURL", "NewCalcAPI" });
            if (String.IsNullOrEmpty(calcuURL)) {
                return null;
            }
            return $"{calcuURL}{CALC_API_URLS[funName]}";
        }

        private static Dictionary<string, string> AUTH_API_URLS = new Dictionary<string, string>()
        {
            {"validate_brand_token","/auth/validatebrand" },
            {"validate_player_token", "/auth/validateplayer" },
            {"get_brand_token", "/auth/token" },
            {"get_player_token", "/auth/playertoken" }

        };

        public static string GetAuthAPiURL(string funName)
        {
            var authURL = Appsettings.app(new string[] { "ServiceURL", "AuthAPI" });
            return $"{authURL}{AUTH_API_URLS[funName]}";
        }

        public static string GetServerAgentCode()
        {
            try
            {
                string envinromentAgent = Environment.GetEnvironmentVariable("AgentCode");
                if (!string.IsNullOrEmpty(envinromentAgent))
                {
                    return envinromentAgent;
                }
            }
            catch(Exception e)
            {
                // no need do anything 
            }
            return Appsettings.app(new string[] {"YooConfig","AgentCode" });
        }
    }
}
