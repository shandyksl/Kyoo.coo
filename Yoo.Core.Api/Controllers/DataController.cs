using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Globalization;
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
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;
        private readonly IPlayerService _playerService;
        private readonly IBetService _betService;
        private readonly IAuthService _authService;

        public DataController(ILogger<DataController> logger, IPlayerService playerService, IBetService betService, IAuthService authService)
        {
            _logger = logger;
            _playerService = playerService;
            _betService = betService;
            _authService = authService;
        }

        [HttpPost("GetBalance")]
        public async Task<object> GetBalance()
        {
            var user = HttpContext.Items["AuthenticatedUser"] as AuthenticatedUser;

            return await _playerService.GetPlayerBalance(user.AgentCode, user.LoginName);
        }

        [HttpPost("GetPlayerInfo")]
        public async Task<object> GetPlayerInfo()
        {
            var user = HttpContext.Items["AuthenticatedUser"] as AuthenticatedUser;

            return await _playerService.GetPlayerInfo(user.AgentCode, user.LoginName);

        }

        [HttpPost("GetBetHistory")]
        public async Task<object> GetBetHistory([FromBody] BetHistoryRequest betHistoryRequest)
        {
            var user = HttpContext.Items["AuthenticatedUser"] as AuthenticatedUser;
            bool isFromDateSuccess = DateTime.TryParseExact(betHistoryRequest.FromDate, DateTimeFormat.StandardardDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fromDate);
            bool isToDateSuccess = DateTime.TryParseExact(betHistoryRequest.ToDate, DateTimeFormat.StandardardDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime toDate);

            if (user == null
               || string.IsNullOrEmpty(betHistoryRequest.FromDate)
               || string.IsNullOrEmpty(betHistoryRequest.ToDate))
            {
                _logger.LogInformation("GetBetHistory报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }
            else if (!isFromDateSuccess || !isToDateSuccess || fromDate >= toDate)
            {
                _logger.LogInformation("GetBetHistory报错:" + CommonFunction.GetErrorDesc(303));
                return CommonFunction.GetErrorCode(303); // invalid_date_format
            }
            else if (betHistoryRequest.Sort != Sorting.ASC && betHistoryRequest.Sort != Sorting.DESC)
            {
                _logger.LogInformation("GetBetHistory报错:" + CommonFunction.GetErrorDesc(309));
                return CommonFunction.GetErrorCode(309); // invalid_sorting
            }
            else if (!string.IsNullOrEmpty(betHistoryRequest.GameType) && !GameType.IsValid(betHistoryRequest.GameType))
            {
                _logger.LogInformation("GetGameInfo报错:" + CommonFunction.GetErrorDesc(310));
                return CommonFunction.GetErrorCode(310); // invalid_game_type
            }

            string sorting = (betHistoryRequest.Sort == Sorting.ASC) ? "ASC" : "DESC";
            return await _betService.GetBetHistory(user.AgentCode, user.LoginName, betHistoryRequest.GameType, betHistoryRequest.GameCode, fromDate, toDate, betHistoryRequest.Page, betHistoryRequest.PageSize, sorting);
        }

        [HttpPost("GetBetDetail")]
        public async Task<object> GetBetDetail([FromBody] BetDetailRequest betDetailRequest)
        {
            var user = HttpContext.Items["AuthenticatedUser"] as AuthenticatedUser;

            if (user == null || string.IsNullOrEmpty(betDetailRequest.TransactionId))
            {
                _logger.LogInformation("GetBetDetail报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }

            return await _betService.GetBetDetail(user.AgentCode, user.LoginName, betDetailRequest.TransactionId);
        }
    }
}