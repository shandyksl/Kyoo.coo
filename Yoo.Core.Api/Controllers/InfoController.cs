using Microsoft.AspNetCore.Mvc;
using Yoo.Core.Business.Interface;
using Yoo.Core.Middleware.Filters;
using Yoo.Core.Common.Helper;
using Yoo.Core.Model.DTO;
using Yoo.Core.Model.Common;
using System.Globalization;

namespace Yoo.Core.Api.Controllers
{
    [Route("Api/[Controller]")]
    [ApiController]
    [ServiceFilter(typeof(BearerAuthenticationAttribute))]
    public class InfoController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IGameService _gameService;

        public InfoController(ILogger<ServiceController> logger, IGameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }

        [HttpPost("GetGameLobby")]
        public async Task<object> GetGameLobby()
        {
            var user = HttpContext.Items["AuthenticatedUser"] as AuthenticatedUser;
            return await _gameService.GetGameLobby(user.AgentCode);
        }

        [HttpPost("GetGameInfo")]
        public async Task<object> GetGameInfo([FromBody] GameInfoRequest request)
        {
            var user = HttpContext.Items["AuthenticatedUser"] as AuthenticatedUser;
            if (string.IsNullOrEmpty(user.AgentCode) || string.IsNullOrEmpty(request.GameType))
            {
                _logger.LogInformation("GetGameInfo报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }
            else if (!GameType.IsValid(request.GameType))
            {
                _logger.LogInformation("GetGameInfo报错:" + CommonFunction.GetErrorDesc(310));
                return CommonFunction.GetErrorCode(310); // invalid_game_type
            }

            string sorting = (request.Sort == Sorting.ASC) ? "ASC" : "DESC";
            return await _gameService.GetGameInfo(user.AgentCode, request.GameType, request.Page, request.PageSize, sorting);
        }

        [HttpPost("GetGameResult")]
        public async Task<object> GetGameResult([FromBody] GameResultRequest request)
        {
            bool isFromDateSuccess = DateTime.TryParseExact(request.FromDate, DateTimeFormat.StandardardDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fromDate);
            bool isToDateSuccess = DateTime.TryParseExact(request.ToDate, DateTimeFormat.StandardardDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime toDate);

            if (!isFromDateSuccess || !isToDateSuccess || fromDate >= toDate)
            {
                _logger.LogInformation("GetGameResult报错:" + CommonFunction.GetErrorDesc(303));
                return CommonFunction.GetErrorCode(303); // invalid_date_format
            }
            else if (!string.IsNullOrEmpty(request.GameType) && !GameType.IsValid(request.GameType))
            {
                _logger.LogInformation("GetGameResult报错:" + CommonFunction.GetErrorDesc(310));
                return CommonFunction.GetErrorCode(310); // invalid_game_type
            }

            string sorting = (request.Sort == Sorting.ASC) ? "ASC" : "DESC";
            return await _gameService.GetGameResult(request.GameType, request.GameCode, fromDate, toDate, request.Page, request.PageSize, sorting);
        }
    }
}
