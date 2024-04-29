using System;

namespace Yoo.Core.Model.Common
{
    public static class AssetType
    {
        public const string STOCKMARKETINDEX = "SMI";
        public const string CRYPTO = "Crypto";

        public static bool IsValid(string type)
        {
            return type == STOCKMARKETINDEX
                || type == CRYPTO;
        }
    }
}