using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Encryptor;
using Yoo.Core.Common.Helper;
using Yoo.Core.Common.Storage;

namespace Yoo.Core.Middleware.Filters
{
    public class SecurityHashVerificationAttribute : ActionFilterAttribute
	{
        private readonly ILogger _logger;
        private readonly IDatabase _redis;
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _environment;

        public SecurityHashVerificationAttribute(ILogger<SecurityHashVerificationAttribute> logger, RedisHelper redis, IAuthService authService, IWebHostEnvironment environment)
		{
            _logger = logger;
            _redis = redis.GetDatabase();
            _authService = authService;
            _environment = environment;
        }

        async public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) 
        {
            if (_environment.IsDevelopment() || _environment.IsEnvironment("Localhost"))
            {
                _logger.LogInformation("Security Hash is disabled in development or localhost environment.");
                await next();
            }
            else
            {
                var timestamp = context.HttpContext.Request.Headers["Timestamp"].FirstOrDefault();
                var sign = context.HttpContext.Request.Headers["Sign"].FirstOrDefault();

                dynamic requestBody;
                using (StreamReader reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8))
                {
                    //这里是重点，读完后的位置必须回归
                    context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                    requestBody = await reader.ReadToEndAsync();
                    context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                }

                JObject body = JObject.Parse(requestBody);

                var agentCode = (string) body["agentCode"]; ;

                if (string.IsNullOrEmpty(agentCode) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(sign))
                {
                    _logger.LogInformation("SecurityHash报错:" + CommonFunction.GetErrorDesc(600));
                    context.Result = new JsonResult(CommonFunction.GetErrorCode(600)); // missing_required_headers
                    return;
                }

                bool IsTimestampValid = CommonFunction.IsTimestampValid(timestamp);

                if (!IsTimestampValid)
                {
                    _logger.LogInformation("SecurityHash报错:" + CommonFunction.GetErrorDesc(601));
                    context.Result = new JsonResult(CommonFunction.GetErrorCode(601)); // timestamp_expired
                    return;
                }

                var agent = await _authService.GetAgentConfigByAgentCode(agentCode);
                if (agent == null)
                {
                    _logger.LogInformation("SecurityHash报错:" + CommonFunction.GetErrorDesc(109));
                    context.Result = new JsonResult(CommonFunction.GetErrorCode(109)); // agent_not_found
                    return;
                }
                if (!agent.IsActive)
                {
                    _logger.LogInformation("SecurityHash报错:" + CommonFunction.GetErrorDesc(104));
                    context.Result = new JsonResult(CommonFunction.GetErrorCode(104)); // agent_disable
                    return;
                }

                var expectedHash = Md5.CreateMD5(agent.AgentCode + agent.ApiKey + timestamp + requestBody).ToLower();

                if (sign != expectedHash)
                {
                    _logger.LogInformation("SecurityHash报错:" + CommonFunction.GetErrorDesc(602));
                    _logger.LogInformation("Generated Sign:" + (string) expectedHash);
                    _logger.LogInformation("Requested Sign:" + sign);
                    context.Result = new JsonResult(CommonFunction.GetErrorCode(602)); // sign_mismatch
                    return;
                }

                //_logger.LogInformation("SecurityHash: Verified successfully.");
                await next();
            }
        }
    }
}

