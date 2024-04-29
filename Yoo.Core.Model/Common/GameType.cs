using System;

namespace Yoo.Core.Model.Common
{
    public static class GameType
    {
        public const string STOCKCP = "STCP";
        public const string CRYPTOFFC = "CFFC";
        public const string BOXOFFICECP = "BOCP";
        public const string EVENTCP = "EVCP";

        public static bool IsValid(string type)
        {
            return type == STOCKCP
                || type == CRYPTOFFC
                || type == BOXOFFICECP
                || type == EVENTCP;
        }
    }
}