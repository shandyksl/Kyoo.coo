using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yoo.Core.Common.Helper;
using Yoo.Core.Common.Storage;
using Yoo.Core.Business.Interface;
using System;
using System.Threading.Tasks;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.DbModels;
using System.Collections.Generic;
using Jose;
using Yoo.Core.Model.Common;

namespace Yoo.Core.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IDatabase _redis;

        private const string tokenKeyString = "YooToken:";

        public AuthService(ILogger<AuthService> logger, RedisHelper client)
        {
            _logger = logger;
            _redis = client.GetDatabase();
        }

        public async Task<AgentConfig> GetAgentConfigByAgentCode(string agentCode)
        {
            string redisKey = "AgentConfigData:" + agentCode;
            var result = await _redis.StringGetAsync(redisKey);

            if (result == RedisValue.Null)
            {
                var agent = await AuthDAO.GetAgentConfigByAgentCode(agentCode);
                if (agent == null) return null;
                await _redis.StringSetAsync(redisKey, JsonConvert.SerializeObject(agent), TimeSpan.FromSeconds(1800));
                return agent;
            }
            else
            {
                return JsonConvert.DeserializeObject<AgentConfig>(result);
            }
        }

        public async Task<AuthenticatedUser> ValidateBearerToken(string token)
        {
            // 验证Token
            string redisKey = tokenKeyString + token;
            var existData = await _redis.StringGetAsync(redisKey);

            if (existData != RedisValue.Null)
            {
                return JsonConvert.DeserializeObject<AuthenticatedUser>(existData);
            }
            else
            {
                return null;
            }
        }
        public async Task<string> GenerateBearerToken(string agentCode, string loginName, string currencyCode, string apiKey)
        {
            try
            {
                var header = new Dictionary<string, object>{
                    { "alg", "HS256" },
                    { "typ","JWT" },
                    { "curTime", DateTime.Now.ToString() }
                };

                AuthenticatedUser user = new AuthenticatedUser()
                {
                    AgentCode = agentCode,
                    LoginName = loginName,
                    CurrencyCode = currencyCode
                };

                string jwtToken = CalcHmac(header, user, apiKey);
                string redisKey = tokenKeyString + jwtToken;
                string jsonData = JsonConvert.SerializeObject(user);
                await _redis.StringSetAsync(redisKey, jsonData, TimeSpan.FromMinutes(30));

                return jwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("GenerateBearerToken未知报错:" + ex.Message);
                return CommonFunction.GetErrorCode(902);
            }
        }

        public static string CalcHmac(Dictionary<string, object> header, AuthenticatedUser user, string apiKey)
        {
            byte[] key = Encoding.ASCII.GetBytes(apiKey);
            return JWT.Encode(user, key, JwsAlgorithm.HS256, extraHeaders: header);
        }


        async public Task<string> Sha256Function(string to_encrypt)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(to_encrypt));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<List<AgentConfig>> GetAllAgentConfigs()
        {
            string redisKey = "AgentConfigData";
            var result = await _redis.StringGetAsync(redisKey);

            if (result == RedisValue.Null)
            {
                var agent = await AuthDAO.GetAllAgentConfigs();
                if (agent == null) return null;
                await _redis.StringSetAsync(redisKey, JsonConvert.SerializeObject(agent), TimeSpan.FromSeconds(1800));
                return agent;
            }
            else
            {
                return JsonConvert.DeserializeObject<List<AgentConfig>>(result);
            }
        }
    }
}
