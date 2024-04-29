using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.Model.Common;

namespace Yoo.Core.Middleware.Common
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext, IAuthService _authService)
        {
            var request = httpContext.Request;
            string bearerToken = request.Headers["Authorization"];

            if(!string.IsNullOrEmpty(bearerToken) && bearerToken.StartsWith("Bearer "))
            {
                bearerToken = bearerToken.Substring(7);
            }
            else
            {
                bearerToken = request.Query["access_token"];
            }

            if (!string.IsNullOrEmpty(bearerToken))
            {
                AuthenticatedUser user = await _authService.ValidateBearerToken(bearerToken);

                if (user == null)
                {
                    _logger.LogInformation("WS:BearerToken报错:" + CommonFunction.GetErrorDesc(100));
                    return;
                }

                httpContext.Items.Add("AuthenticatedUser", user);
                httpContext.Items.Add("AuthenticatedUserToken", bearerToken);
                await _next(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Unauthorized");
                return;
            }
        }
    }
}
