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
    [ServiceFilter(typeof(SecurityHashVerificationAttribute))]
    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IAuthService _authService;
        private readonly IPlayerService _playerService;
        private readonly IBetService _betService;

        public ServiceController(ILogger<ServiceController> logger, IAuthService authService, IPlayerService playerService, IBetService betService)
        {
            _logger = logger;
            _authService = authService;
            _playerService = playerService;
            _betService = betService;
        }

        [HttpPost("CreatePlayer")]
        public async Task<object> CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            if (string.IsNullOrEmpty(request.AgentCode)
                || string.IsNullOrEmpty(request.LoginName)
                || string.IsNullOrEmpty(request.CurrencyCode)
                || string.IsNullOrEmpty(request.IpAddress))
            {
                _logger.LogInformation("CreatePlayer未知报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }
            else if (!CurrencyCode.IsValid(request.CurrencyCode))
            {
                _logger.LogInformation("CreatePlayer未知报错:" + CommonFunction.GetErrorDesc(307));
                return CommonFunction.GetErrorCode(307); // invalid_currencycode_format
            }

            return await _playerService.CreatePlayer(request.AgentCode, request.LoginName, request.CurrencyCode, request.IpAddress);
        }

        [HttpPost("Login")]
        public async Task<object> Login([FromBody] LoginRequest loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.AgentCode)
                || string.IsNullOrEmpty(loginRequest.LoginName)
                || string.IsNullOrEmpty(loginRequest.LanguageCode)
                || string.IsNullOrEmpty(loginRequest.CurrencyCode)
                || string.IsNullOrEmpty(loginRequest.Platform))
            {
                _logger.LogInformation("LoginPlayer未知报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }
            else if (!CurrencyCode.IsValid(loginRequest.CurrencyCode))
            {
                _logger.LogInformation("LoginPlayer未知报错:" + CommonFunction.GetErrorDesc(307));
                return CommonFunction.GetErrorCode(307); // invalid_currencycode_format
            }
            else if (!LanguageCode.IsValid(loginRequest.LanguageCode))
            {
                _logger.LogInformation("LoginPlayer未知报错:" + CommonFunction.GetErrorDesc(308));
                return CommonFunction.GetErrorCode(308); // invalid_languagecode_format
            }
            return await _playerService.LoginPlayer(loginRequest.AgentCode, loginRequest.LoginName, loginRequest.LanguageCode, loginRequest.CurrencyCode, loginRequest.NickName, loginRequest.Platform);
        }

        [HttpPost("CheckBalance")]
        public async Task<object> CheckBalance([FromBody] PlayerRequest playerRequest)
        {
            if (string.IsNullOrEmpty(playerRequest.AgentCode) || string.IsNullOrEmpty(playerRequest.LoginName))
            {
                _logger.LogInformation("CheckBalance未知报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }

            return await _playerService.GetPlayerBalance(playerRequest.AgentCode, playerRequest.LoginName);
        }

        [HttpPost("Transfer")]
        public async Task<object> Transfer([FromBody] TransferRequest transferRequest)
        {
            if (string.IsNullOrEmpty(transferRequest.AgentCode)
                || string.IsNullOrEmpty(transferRequest.LoginName)
                || transferRequest.Amount == 0
                || string.IsNullOrEmpty(transferRequest.TransactionId))
            {
                _logger.LogInformation("Transfer未知报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }

            return await _playerService.Transfer(transferRequest.AgentCode, transferRequest.LoginName, transferRequest.TransactionId, transferRequest.Amount);
        }

        [HttpPost("CheckTransferStatus")]
        public async Task<object> CheckTransferStatus([FromBody] TransactionRequest transactionRequest)
        {
            if (string.IsNullOrEmpty(transactionRequest.AgentCode)
                || string.IsNullOrEmpty(transactionRequest.LoginName)
                || string.IsNullOrEmpty(transactionRequest.TransactionId))
            {
                _logger.LogInformation("CheckTransferStatus未知报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }

            return await _playerService.CheckTransferStatus(transactionRequest.AgentCode, transactionRequest.LoginName, transactionRequest.TransactionId);
        }

        [HttpPost("GetBetHistory")]
        public async Task<object> GetBetHistory([FromBody] BetHistoryRequest betHistoryRequest)
        {
            bool isFromDateSuccess = DateTime.TryParseExact(betHistoryRequest.FromDate, DateTimeFormat.StandardardDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fromDate);
            bool isToDateSuccess = DateTime.TryParseExact(betHistoryRequest.ToDate, DateTimeFormat.StandardardDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime toDate);

            if (string.IsNullOrEmpty(betHistoryRequest.AgentCode) || string.IsNullOrEmpty(betHistoryRequest.FromDate) || string.IsNullOrEmpty(betHistoryRequest.ToDate))
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

            string sorting = (betHistoryRequest.Sort == Sorting.ASC) ? "ASC" : "DESC";
            return await _betService.GetBetHistory(betHistoryRequest.AgentCode, string.Empty, betHistoryRequest.GameType, betHistoryRequest.GameCode, fromDate, toDate, betHistoryRequest.Page, betHistoryRequest.PageSize, sorting);
        }

        [HttpPost("GetBetDetail")]
        public async Task<object> GetBetDetail([FromBody] BetDetailRequest betDetailRequest)
        {
            if (string.IsNullOrEmpty(betDetailRequest.AgentCode)
                || string.IsNullOrEmpty(betDetailRequest.TransactionId))
            {
                _logger.LogInformation("GetBetDetail未知报错:" + CommonFunction.GetErrorDesc(300));
                return CommonFunction.GetErrorCode(300); // invalid_arguments
            }

            return await _betService.GetBetDetail(betDetailRequest.AgentCode, string.Empty, betDetailRequest.TransactionId);
        }
    }
}
