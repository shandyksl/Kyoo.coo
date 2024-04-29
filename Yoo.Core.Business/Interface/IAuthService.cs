using System.Collections.Generic;
using System.Threading.Tasks;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.Business.Interface
{
    public interface IAuthService
    {
        Task<AgentConfig> GetAgentConfigByAgentCode(string agentCode);
        Task<List<AgentConfig>> GetAllAgentConfigs();
        Task<string> GenerateBearerToken(string agentCode, string loginName, string currencyCode, string apiKey);
        Task<AuthenticatedUser> ValidateBearerToken(string token);
    }
}
