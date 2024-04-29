using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.DataAccess.YooMain;
using Yoo.Core.Model.Common;

namespace Yoo.Core.Business.Services
{
    public class AssetService : IAssetService
    {
        private readonly ILogger<AssetService> _logger;

        public AssetService(ILogger<AssetService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<object>> GetAssetsByAssetType(string assetType)
        {
            if(!AssetType.IsValid(assetType))
            {
                _logger.LogInformation("GetAssetsByAssetType报错:" + CommonFunction.GetErrorDesc(800));
                return CommonFunction.GetErrorCode(800); // invalid_asset_type
            }

            return await AssetDAO.GetAssetsByAssetType(assetType);
        }
    }
}
