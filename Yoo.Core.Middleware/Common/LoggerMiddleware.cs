using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Middleware.Common
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger<LoggerMiddleware> _logger;
        public LoggerMiddleware(RequestDelegate next, ILogger<LoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }


        public async Task Invoke(HttpContext context)
        {
            var request = await FormatRequest(context.Request);
            var requestId = Guid.NewGuid().ToString("N");
            _logger.LogInformation($"====================Request: {requestId} form is : {request} . =====================");
            var watch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                var response = await FormatResponse(context.Response);
                watch.Stop();
                _logger.LogInformation($"====================Response: {requestId}  body is : {response} . Using time : {watch.ElapsedMilliseconds} ms =====================");
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            string bodyAsText = "Orginal response content";
            try
            {
                var body = request.Body;
                request.EnableBuffering();
                request.Body.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                request.Body.Seek(0, SeekOrigin.Begin);
                bodyAsText = FormJson(Encoding.UTF8.GetString(buffer));
                //request.Body = body;

                return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                return bodyAsText; 
            }
        }

        private string FormJson(string bodyText)
        {
            if (string.IsNullOrWhiteSpace(bodyText))
            {
                return bodyText;
            }

            try
            {
                var jsonObj = JsonConvert.DeserializeObject(bodyText);

                if (jsonObj == null)
                {
                    return bodyText;
                }

                return JsonConvert.SerializeObject(bodyText);

            }
            catch
            {
                return bodyText;
            }
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            string text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return $"{response.StatusCode}: {text}";
        }
    }
}
