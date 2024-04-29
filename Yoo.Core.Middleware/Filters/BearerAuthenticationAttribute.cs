using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.Model.Common;

namespace Yoo.Core.Middleware.Filters
{
    public class BearerAuthenticationAttribute : IAsyncAuthorizationFilter
    {
        private readonly ILogger _logger;
        private readonly IAuthService _authService;

        public BearerAuthenticationAttribute(ILogger<BearerAuthenticationAttribute> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string jwt = GetBearerToken(context);

            if (string.IsNullOrEmpty(jwt))
            {
                _logger.LogInformation("BearerAuthentication报错:" + CommonFunction.GetErrorDesc(301));
                context.Result = new JsonResult(CommonFunction.GetErrorCode(301)); // missing_bearer_header
                return;
            }

            AuthenticatedUser user = await _authService.ValidateBearerToken(jwt);

            if (user == null)
            {
                _logger.LogInformation("BearerAuthentication报错:" + CommonFunction.GetErrorDesc(100));
                context.Result = new JsonResult(CommonFunction.GetErrorCode(100)); // invalid_bearer_token
                return;
            }

            context.HttpContext.Items.Add("AuthenticatedUser", user);
            context.HttpContext.Items.Add("AuthenticatedUserToken", jwt);

            //_logger.LogInformation("BearerAuthentication: Authorized successfully.");
            await Task.CompletedTask;
        }

        public string GetBearerToken(AuthorizationFilterContext context)
        {
            string authorizationHeader = context.HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                return authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }
    }
}

