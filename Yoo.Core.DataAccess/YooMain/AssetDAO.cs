using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yoo.Core.Common.Storage;
using Yoo.Core.Model.DbModels;

namespace Yoo.Core.DataAccess.YooMain
{
    public class AssetDAO
    {
        public async static Task<IEnumerable<object>> GetAssetsByAssetType(string assetType)
        {
            using (var db = DBContainer.DB.GetAssetDbConnection())
            {
                List<object> assets = new List<object>();

                string squery = "SELECT a.Symbol, a.CurrencyCode, " +
                    "ap.Price AS Price FROM Asset a " +
                    "INNER JOIN (SELECT Symbol, MAX(CreatedAt) AS MaxCreatedAt FROM AssetPrice GROUP BY Symbol) apMax ON a.Symbol = apMax.Symbol " +
                    "INNER JOIN AssetPrice ap ON a.Symbol = ap.Symbol AND ap.CreatedAt = apMax.MaxCreatedAt WHERE a.Type = @Type";
                var results = await db.QueryAsync<dynamic>(squery, new { Type = assetType });

                return results;
            }
        }
    }
}
