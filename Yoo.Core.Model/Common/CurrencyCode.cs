using System;

namespace Yoo.Core.Model.Common
{
    public static class CurrencyCode
    {
        public const string CNY = "CNY"; //china
        public const string USD = "USD"; //usa
        public const string MYR = "MYR"; //malaysia

        public static bool IsValid(string code)
        {
            return code == CNY 
                || code == USD
                || code == MYR;
        }
    }
}