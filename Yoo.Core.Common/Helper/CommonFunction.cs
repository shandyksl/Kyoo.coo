using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Snowflake.Core;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yoo.Core.Common.Helper
{
    public class CommonFunction
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const string numbers = "0123456789";
        const string ascii_letters_digits = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";

        static readonly Dictionary<int, string> errorCode = new Dictionary<int, string>() {
            // PLAYER & AGENT - 1
            { 100 , "invalid_bearer_token"              }, // 无效验证
            { 101 , "player_disabled"                   }, // 玩家未开通
            { 102 , "invalid_player"                    }, // 玩家不可用
            { 103 , "invalid_agent"                     }, // 代理不可用
            { 104 , "agent_disabled"                    }, // 代理未开通
            { 106 , "player_not_found"                  }, // 玩家不存在
            { 107 , "agent_under_maintenance"           }, // 代理维护中
            { 108 , "invalid_wallet_type"               }, // 钱包类型不可用
            { 109 , "agent_not_found"                   }, // 代理不存在
            { 110 , "login_name_exist"                  }, // 登录名已存在
            { 111 , "login_failed"                      }, // 登录失败
            { 112 , "wallet_not_found"                  }, // 钱包不存在
            { 113 , "wallet_insufficient_balance"       }, // 钱包余额不足
            { 114 , "gameinfo_not_found"                }, // 游戏信息不存在
            { 115 , "gameconfig_not_found"              }, // 游戏配置不存在

            // TRANSACTION - 2
            { 201 , "invalid_amount"                    }, // 无效金额 
            { 202 , "transaction_not_found"             }, // 交易编号不存在
            { 203 , "transfer_limit_exceeded"           }, // 转账数目已超额
            { 204 , "duplicate_transaction"             }, // 交易编号已存在
            { 205 , "invalid_transaction_type"          }, // 无效交易类型
            { 206 , "failed_transfer"                   }, // 转账失败

            // BET - 4
            { 400 , "duplicate_bet_order"               }, // 订单编号已存在
            { 401 , "betdetail_not_found"               }, // 订单编号不存在

            // GAME - 5
            { 501 , "invalid_game_type"                 }, // 无效游戏类型
            { 502 , "bet_limit_exceeded"                }, // 投注金额已超额
            { 503 , "place_bet_failed"                  }, // 下注失败

            // REQUEST BODY - 3
            { 300 , "invalid_arguments"                 }, // 无效参数
            { 301 , "missing_bearer_header"             }, // 缺少Bearer
            { 302 , "incorrect_tag_format"              }, // 验证格式错误
            { 303 , "invalid_date_format"               }, // 无效日期格式: YYYY-MM-DD HH24:MM:SS
            { 304 , "invalid_date_range"                }, // 无效日期期限: 结束必须大过开始48小时
            { 305 , "invalid_page_number"               }, // 无效页码: 必须大或等于1
            { 306 , "invalid_page_size"                 }, // 无效单页大小: 必须大或等于1
            { 307 , "invalid_currencycode_format"       }, // 无效货币
            { 308 , "invalid_languagecode_format"       }, // 无效语言
            { 309 , "invalid_sorting"                   }, // 无效排序方式: 0 - ASC, 1 - DESC
            { 310 , "invalid_game_type"                 }, // 无效游戏类型

            // Security Hash Verify - 6
            { 600 , "missing_required_headers"          }, // 缺少sign headers
            { 601 , "timestamp_expired"                 }, // timestamp过期
            { 602 , "sign_mismatch"                     }, // sign mismatch

            // GLOBAL & DB - 9
            { 900 , "system_error"                      }, // 系统错误
            { 901 , "failed_load_setting"               }, // 加载设置失败
            { 902 , "db_insert_failed"                  }, // 数据库插入失败
            { 903 , "db_update_failed"                  }, // 数据库更新失败

            // Asset - 8
            { 800 , "Invalid_asset_type"                      }, // 无效资产类型
        };

        public static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            using (System.Security.Cryptography.RNGCryptoServiceProvider rng = new())
            {
                rng.GetBytes(bytes);
            }

            return BitConverter.ToInt32(bytes, 0);
        }


        public static string GenerateRandomString(int length = 8)
        {
            Random random = new Random(GetRandomSeed());
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateRandomInteger(int length = 8)
        {
            Random random = new Random(GetRandomSeed());
            return new string(Enumerable.Repeat(numbers, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateRandomStringAscii(int length = 8)
        {
            Random random = new Random(GetRandomSeed());
            return new string(Enumerable.Repeat(ascii_letters_digits, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GeneraterRandomSnowFlake()
        {
            // By this only generate 19
            long id = IdWorker.Instance.NextId();
            return id.ToString();
        }

        public static bool IsPropertyExist(dynamic settings, string name)
        {
            if (settings is ExpandoObject)
                return ((IDictionary<string, object>)settings).ContainsKey(name);

            return settings.GetType().GetProperty(name) != null;
        }

        async public static Task<T> GetObjectFromCache<T>(IDatabase redis, Func<Task<T>> overloadFunction, string key, double defaultCacheSeconds = 1800)
        {
            string result;
            try
            {
                result = await redis.StringGetAsync(key);
            }
            catch (Exception e)
            {
                result = null;
            }

            if (result == null)
            {
                var agent = await overloadFunction();
                if (agent == null)
                    return default(T);
                try
                {
                    await redis.StringSetAsync(key, JsonConvert.SerializeObject(agent), TimeSpan.FromSeconds(defaultCacheSeconds));
                }
                catch
                {
                    return agent;
                }
                return agent;
            }
            else
                return JsonConvert.DeserializeObject<T>(result);

        }

        public static dynamic GetErrorCode(int code)
        {
            return new
            {
                Error = code,
                ErrorDescription = errorCode.ContainsKey(code) ? errorCode[code] : "Unexpected Error"
            };
        }

        public static string GetErrorDesc(int code)
        {
            return errorCode.ContainsKey(code) ? errorCode[code] : "Unexpected Error";
        }

        public static bool isContainErrorCode(int code)
        {
            return errorCode.ContainsKey(code);
        }

        public static dynamic GetSuccessCode(dynamic data)
        {
            return new
            {
                StatusCode = 200,
                Data = data
            };
        }

        public static long GenerateSecondsTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        public static string ConvertToTimestampSecondsOnly(string dateStr)
        {
            DateTime dates = DateTime.ParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string timestamp = (new DateTimeOffset(dates).ToUnixTimeSeconds()).ToString();
            return timestamp;
        }

        public static long GenerateTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
        public static string ConvertToTimestamp(string dateStr)
        {
            DateTime dates = DateTime.ParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string timestamp = (new DateTimeOffset(dates).ToUnixTimeMilliseconds()).ToString();
            return timestamp;
        }

        public static string ConvertToSecondsTimestamp(string dateStr)
        {
            DateTime dates = DateTime.ParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string timestamp = (new DateTimeOffset(dates).ToUnixTimeSeconds()).ToString();
            return timestamp;
        }

        public static DateTime ConvertTimeStampToDateTime(long timeStamp)
        {
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).LocalDateTime;
            return dateTime;
        }

        public static DateTime ConvertTimeStampMillisecondsToDateTime(long timeStamp)
        {
            DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).LocalDateTime;
            return dateTime;
        }

        public static string ReplaceStringByObject(string originalsrtring, JObject param)
        {
            IList<string> keys = param.Properties().Select(p => p.Name).ToList();
            foreach (var k in keys)
            {
                originalsrtring = Regex.Replace(originalsrtring, $"@{k}", $"'{param[k].ToString()}'", RegexOptions.IgnoreCase);
            }
            return originalsrtring;
        }


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime UnixTimeStampSecondsToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static bool IsTimestampValid(string timestamp, int validityPeriod = 30)
        {
            if (!long.TryParse(timestamp, out long timestampDateTime))
            {
                return false; // Unable to parse timestamp
            }

            DateTimeOffset timestampToCheck = ConvertTimeStampToDateTime(timestampDateTime);
            var timestampValidityEnd = timestampToCheck.UtcDateTime.AddMinutes(validityPeriod);
            var currentTime = DateTime.UtcNow;

            return currentTime <= timestampValidityEnd;
        }

        public static bool isValidJson(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }
    }
}
