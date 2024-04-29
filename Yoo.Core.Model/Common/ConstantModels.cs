using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.Common
{
    public static class ConstantModels
    {
        public const string PLAYERDB = "playerdb";
        public const string REPORTINGDB = "reportdb";

    }

    public static class DateTimeFormat
    {
        public const string StandardardDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    }

    public static class PlayerStatus
    {
        public const int ACTIVE = 1;
        public const int INACTIVE = 2;
        public const int BANNED = 3;
    }

    public static class TransactionType
    {
        public const int TRANSFERIN = 1;
        public const int TRANSFEROUT = 2;
        public const int BET = 3;
        public const int PAYOUT = 4;
        public const int REFUND = 5;
    }

    public static class SettleStatus
    {
        public const int UNSETTLED = 1; //未结算
        public const int SETTLING = 2; //结算中
        public const int SETTLED = 3; //已结算
    }

    public static class GameState
    {
        public const int OPEN = 1; //开局
        public const int CLOSE = 2; //关闭
        public const int END = 3; //结束
    }

    public static class GameBadge
    {
        public const int NORMAL = 1; //普通
        public const int NEW = 2; //新
        public const int HOT = 3; //热门
    }

    public static class Sorting
    {
        public const int ASC = 1;
        public const int DESC = 2;
    }
}