using System;

namespace Yoo.Core.Model.Common
{
    public static class LanguageCode
    {
        public const string English = "en";
        public const string Chinese = "zh";
        public const string SimplifiedChinese = "zh-cn";
        public const string TraditionalChinese = "zh-tw";

        public static bool IsValid(string code)
        {
            return code == English 
                || code == Chinese
                || code == SimplifiedChinese
                || code == TraditionalChinese;
        }
    }
}