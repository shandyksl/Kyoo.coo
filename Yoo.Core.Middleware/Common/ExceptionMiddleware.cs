
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Yoo.Core.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Microsoft.Extensions.Primitives;
using log4net.Core;

namespace Yoo.Core.Middleware.Common
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private IHostEnvironment environment;
        private ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionMiddleware> logger)
        {
            this.next = next;
            this.environment = env;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next.Invoke(context);
                var features = context.Features;
            }
            //catch (MySqlException e)
            //{
            //    _logger.LogInformation(e.Message.ToString());
            //    _ = Task.Run(() => { SaveRequestWhenSQLException(context); });
            //    await HandleException(context, e);
            //}
            catch (Exception e)
            {
                await HandleException(context, e);
            }
        }

        private async Task HandleException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            _logger.LogError(ex, $"未知报错: {ex.Message}");

            var error = new
            {
                Error = 900,
                ErrorDescription = "system_error"
            };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(error));

        }

        //private async Task SaveRequestWhenSQLException(HttpContext context)
        //{
        //    try {
        //        string url = $"{context.Request.Host}{context.Request.Path}";
        //        var headers = new StringBuilder();
        //        foreach (var value in (IEnumerable<KeyValuePair<string, StringValues>>)context.Request.Headers)
        //        {
        //            headers.AppendLine(value.Key + " = " + value.Value + ",");
        //        }
        //        string contentMethod = context.Request.ContentType;
        //        string requestType = context.Request.Method.ToString();
        //        StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        //        string body = reader.ReadToEnd();
        //        await GameDAO.InsertSdkReqeustWhenSqlException(url, requestType, contentMethod, body, headers.ToString());
        //    }
        //    catch (Exception e) 
        //    {
        //        _logger.LogInformation("Erro, SaveRequestWhenSQLException :" + e.ToString());
        //    }

        //}

    }
}
