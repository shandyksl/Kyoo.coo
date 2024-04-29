using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class AssetPrice
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
