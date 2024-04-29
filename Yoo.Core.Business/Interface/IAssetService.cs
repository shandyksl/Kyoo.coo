using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoo.Core.Model.Common;

namespace Yoo.Core.Business.Interface
{
    public interface IAssetService
    {
        Task<IEnumerable<object>> GetAssetsByAssetType(string assetType);
    }
}
