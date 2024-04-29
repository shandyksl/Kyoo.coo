using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class Asset
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
