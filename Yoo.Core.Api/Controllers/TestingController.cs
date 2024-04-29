using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Yoo.Core.Common.Storage;
using System.Text;
using Newtonsoft.Json;
using Yoo.Core.Common.Helper;
using Yoo.Core.Business.Interface;

namespace Yoo.Core.Api.Controllers
{

    [Route("Api/[controller]")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private IPlayerService _playerService;
        private IAuthService _authService;
        private readonly ILogger<TestingController> _logger;
        private readonly IDatabase _redis;
        //private readonly MongoHelper _mongo;
        private readonly IHttpClientFactory _http;

        public TestingController(IPlayerService service, IAuthService authService, ILogger<TestingController> logger, RedisHelper client, IHttpClientFactory httpClient)
        {
            _playerService = service;
            _authService = authService;
            _logger = logger;
            _redis = client.GetDatabase();
            _http = httpClient;
        }

        [Produces("application/json")]
        [HttpPost]
        [Route("GetRedis")]
        public async Task<object> GetRedisData()
        {
            var value = "YooApi testing key";
            await _redis.StringSetAsync("YooTestingKey", value);

            string result = await _redis.StringGetAsync("YooTestingKey");
            if (result == value)
            {
                return new { error = 0, desc = "Successful!" };
            }
            else
            {
                return new { error = -999, desc = result };
            }
        }

        [HttpGet]
        [Route("CheckTimeZone")]
        public object GetTimeZoneInfo()
        {
            return Content(
                 @$"<!DOCTYPE html>
                <html><head><meta charset=""utf-8""></head>
                <body>
                <div>{TimeZoneInfo.Local.StandardName}</div>
                <div>{DateTime.Now.ToLongTimeString()}</div>
                <div id=time></div>
                <script>document.getElementById('time').innerText=new Date().toLocaleTimeString()</script>
                </body></html>", "text/html");
        }

        [HttpPost]
        [Route("GetContent")]
        public async Task<object> GetRawContent()
        {
            //throw new Exception("test raw content");
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var content = await reader.ReadToEndAsync();
                dynamic result = JsonConvert.DeserializeObject(content);     //JObject.Parse(content);
                string sb = result.Name;
                return result;
            }
        }

        [HttpPost]
        [Route("TestRabbitMQ")]
        public IActionResult TestRabbitMQ(string message)
        {
            try
            {
                RabbitMQHelper _rabbitMQHelper = new RabbitMQHelper(
                    "10.80.5.236",
                    5672,
                    "rabbit",
                    "dididada_",
                    "yoo_dev");

                var exchange = "yoo_settlement";
                var queue = "yoo_crypto_queue";
                var routingKey = "yoo_crypto";

                _rabbitMQHelper.PublishMessage(exchange, queue, routingKey, message);

                return Ok("消息发送成功！");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"发送消息时出错：{ex.Message}");
            }
        }

        //[HttpPost]
        //[Route("GetMongo")]
        //public async Task<object> WriteToMongo()
        //{
        //    var json = new
        //    {
        //        Name = "Wei ning",
        //        Property = "SB"
        //    };

        //    string result = JsonConvert.SerializeObject(json);
        //    await _mongo.InsertAsync("TestMongoInsert", result);
        //    await _mongo.UpdateAsync("TestMongoInsert", "Property", "SB", "Name", "Ning Ye");
        //    dynamic serchResult = await _mongo.FindAsync("TestMongoInsert", "Property", "SB");
        //    var returnMsgs = new
        //    {
        //        Key = "SearchResult",
        //        Name = serchResult["Name"]
        //    };
        //    return returnMsgs; 
        //}

        //[HttpPost]
        //[Route("GetDB")]
        //public async Task<object> ReadFromMySQL()
        //{
        //    var json = new
        //    {
        //        Name = "Wei ning",
        //        Property = "2B"
        //    };

        //    //string db = await SDKCommon.GetAgentConfig("sg98","playerdb");           
        //    var player = await SDKCommon.GetPlayerByLoginName("sg98", "stevenli02");
        //    return new
        //    {
        //        agentCode = "SG98",
        //        ID = player.externalid 
        //    };
        //}

        [HttpPost]
        [Route("TestAPI")]
        public async Task<object> GetDataFromAPI()
        {
            _logger.LogInformation("calling TestAPI");
            string url = "https://nimbus-games-sdk.staging.awesometech.io/fakesdk/GetBalance";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("username", "stevenli02");
            param.Add("product", "IMSB");
            param.Add("webcode", "da");

            var result = await HttpRequestHelper.SendRequestToAgent(_http, url, param, "IMSB", "POST", "da", true);

            return new
            {
                Type = "POST",
                Result = result.balance
            };
        }

        //[HttpPost]
        //[Route("lazyUpdate")]
        //public async Task<object> GetCalulation()
        //{
        //    dynamic result;
        //    using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        //    {
        //        var content = await reader.ReadToEndAsync();
        //        result = JsonConvert.DeserializeObject(content);     //JObject.Parse(content);
        //    }
        //    int totalbet = 0, faildata = 0;
        //    string roundid = "";
        //    var agent = await _gameSvc.GetAgentByWebcode("m");

        //    foreach (var i in result)
        //    {
        //        foreach (var indata in i)
        //        {
        //            try
        //            {
        //                if (Decimal.Parse((string)indata["current_win_amount"]) != Decimal.Parse((string)indata["expected_win_amount"]))
        //                {
        //                    var existsRecord = await _gameSvc.GetBetHistoryByRoundId(agent.agentcode, (string)indata["round_id"], "MG");
        //                    if (existsRecord != null)
        //                    {
        //                        var upresult = await _gameSvc.Direct_Update_BetHistory(agent, (string)indata["round_id"], Decimal.Parse((string)indata["expected_win_amount"]));
        //                        if (upresult == null)
        //                        {
        //                            faildata++;
        //                        }
        //                        else
        //                        {
        //                            totalbet++;
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                _logger.LogInformation(e.ToString());
        //                faildata++;
        //            }
        //        }
        //    }

        //    return new { successdata = totalbet, faildata = faildata };
        //}

        //[HttpPost]
        //[Route("TestDB")]
        //public async Task<object> TestDB()
        //{
        //    List<dynamic> agentinfo = new List<dynamic>() {
        //        new { agentcode = "mango", roundid = "10000", ProductShortName="sgqp", player = "wojiushiwo"},
        //        new { agentcode = "cqtest", roundid = "10000", ProductShortName="sgqp", player = "wojiushiwo"},
        //        new { agentcode = "sm", roundid = "10000", ProductShortName="sgqp", player = "wojiushiwo"}
        //    };

        //    foreach (var d in agentinfo) 
        //    {
        //        try
        //        {
        //            var agent = await _gameSvc.GetBetHistoryByRoundId((string)d.agentcode, (string)d.roundid, (string)d.ProductShortName);
        //            var player = await _gameSvc.GetPlayerData((string)d.agentcode, (string)d.player);
        //        }
        //        catch (Exception e) 
        //        {
        //            _logger.LogInformation(e.ToString());
        //        }
        //    }


        //    return new
        //    {
        //        result = 1
        //    };
        //}

    }
}
