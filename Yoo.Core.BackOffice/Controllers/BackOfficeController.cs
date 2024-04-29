using Microsoft.AspNetCore.Mvc;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.Model.Common;
using Yoo.Core.Model.DTO;

namespace Yoo.Core.BackOffice.Controllers
{
    [Route("Api/[Controller]")]
    [ApiController]
    public class BackOfficeController : ControllerBase
    {
        private readonly ILogger<BackOfficeController> _logger;
 
        public BackOfficeController(ILogger<BackOfficeController> logger)
        {
            _logger = logger;
        }


        [HttpPost("CheckBalance")]
        public async Task<object> CheckBalance()
        {
            return ("Hello World");
        }
    }
}
