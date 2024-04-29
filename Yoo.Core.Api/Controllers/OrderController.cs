using Microsoft.AspNetCore.Mvc;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.Middleware.Filters;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DTO;

namespace Yoo.Core.Api.Controllers
{
    [Route("Api/[Controller]")]
    [ApiController]
    [ServiceFilter(typeof(BearerAuthenticationAttribute))]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;

        public OrderController(ILogger<OrderController> logger, IOrderService orderService, IAuthService authService)
        {
            _logger = logger;
            _orderService = orderService;
            _authService = authService;
        }

        [HttpPost("PlaceBet")]
        public async Task<object> PlaceBet([FromBody] PlaceBetRequest request)
        {
            if (string.IsNullOrEmpty(request.GameType)
                || request.BetInfo.Count == 0)
            {
                _logger.LogInformation("PlaceBet报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }

            var user = HttpContext.Items["AuthenticatedUser"] as AuthenticatedUser;

            if (user == null)
            {
                _logger.LogInformation("PlaceBet报错:" + CommonFunction.GetErrorDesc(106));
                return CommonFunction.GetErrorCode(106); // player_not_found
            }

            return await _orderService.PlaceBet(user.AgentCode, user.LoginName, request.GameType, request.TotalBetAmount, request.BetInfo);

        }
    }
}
