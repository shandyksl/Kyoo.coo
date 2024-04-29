using System;

namespace Yoo.Core.Model.Common
{
    public static class TransferType
    {
        public const int TRANSFERIN = 1;
        public const int TRANSFEROUT = 2;

        public static bool IsValid(int type)
        {
            return type == TRANSFERIN 
                || type == TRANSFEROUT;
        }
    }
}