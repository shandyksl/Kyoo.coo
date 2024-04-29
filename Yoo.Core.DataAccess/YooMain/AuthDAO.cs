using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yoo.Core.Common.Storage;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.DataAccess.YooMain
{
    public class AuthDAO
    {
        public async static Task<AgentConfig> GetAgentConfigByAgentCode(string AgentCode)
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string squery = "SELECT * FROM AgentConfig WHERE AgentCode = @AgentCode LIMIT 1";
                var result = await db.QueryFirstOrDefaultAsync<AgentConfig>(squery, new { AgentCode = AgentCode });
                return result;
            }
        }

        public async static Task<List<AgentConfig>> GetAllAgentConfigs()
        {
            using (var db = DBContainer.DB.GetMainConnection())
            {
                string squery = "SELECT * FROM AgentConfig";
                var result = await db.QueryAsync<AgentConfig>(squery);
                return result.ToList();
            }
        }
    }
}
