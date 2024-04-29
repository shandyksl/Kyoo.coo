using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Yoo.Core.Common.Helper
{
    public class HttpRequestHelper
    {
        private static ILogger logger = ApplicationLogging.CreateLogger<HttpRequestHelper>();

        public static async Task<dynamic> SendRequestToAsync(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string type, bool isRecord = false , int timeexpiretype = 0)
        {
            string result;
            dynamic returnResult = new { }; // todo: need change and add timeout return information
            try
            {
              
                var client = factory.CreateClient("YooClient");

                if (timeexpiretype != 0)
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                }
                else
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                }

                using (var request = new HttpRequestMessage())
                {
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;

                    }
                    var value_list = new List<KeyValuePair<string, string>>();
                    if (parameter != null)
                    {
                        foreach (KeyValuePair<string, string> entry in parameter)
                        {
                            value_list.Add(entry);
                        }
                    }
                    else
                    {
                        value_list = new();
                    }

                    var formContent = new FormUrlEncodedContent(value_list);
                    request.RequestUri = new Uri(url);
                    request.Content = formContent;
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);

                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestToAsync, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                    }
                    else
                    {
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }

                }
                return returnResult;

            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                
                dynamic errResponse = new
                {
                    error = "network_error",
                    error_description = $"Something is wrong in the network layer"
                };

                returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestToAsyncGzip(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string type, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { }; // todo: need change and add timeout return information
            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate

                };
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    using (var request = new HttpRequestMessage())
                    {
                        if (type.ToLower() == "get")
                        {
                            request.Method = HttpMethod.Get;
                        }
                        else
                        {
                            request.Method = HttpMethod.Post;

                        }
                        var value_list = new List<KeyValuePair<string, string>>();
                        if (parameter != null)
                        {
                            foreach (KeyValuePair<string, string> entry in parameter)
                            {
                                value_list.Add(entry);
                            }
                        }
                        else
                        {
                            value_list = new();
                        }

                        var formContent = new FormUrlEncodedContent(value_list);
                        request.RequestUri = new Uri(url);
                        request.Content = formContent;
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsStringAsync();
                            try
                            {
                                returnResult = JObject.Parse(result);

                            }
                            catch (Exception e)
                            {
                                logger.LogInformation("SendRequestToAsync, Can not convert the response to json, result:" + result);
                                returnResult = result;
                            }
                        }
                        else
                        {
                            dynamic errResponse = new
                            {
                                error = "network_error",
                                error_description = $"Status Code : {response.StatusCode}"
                            };

                            logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                            returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                        }

                    }
                }
                return returnResult;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestToLogAPI(IHttpClientFactory factory,string funName, Dictionary<string,string> parameter)
        {
            var loggerURL = ServiceHelper.GetLogAPiURL(funName);
            return  await SendRequestToAsync(factory, loggerURL, parameter, "POST");
        }

        public static async Task<dynamic> SendRequestToAgent(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string type, string vendor = "", string agentCode = "", bool isRecord = false, bool retry = false, string funct = "")
        {
            string result;
            dynamic returnResult = new { };
            HttpResponseMessage response = null;
            try
            {

                var client = factory.CreateClient("YooClient");
                var request = new HttpRequestMessage();

                if (type.ToLower() == "get")
                {
                    request.Method = HttpMethod.Get;
                }
                else
                {
                    request.Method = HttpMethod.Post;

                }
                var value_list = new List<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> entry in parameter)
                {
                    value_list.Add(entry);
                }
                var formContent = new FormUrlEncodedContent(value_list);
                request.RequestUri = new Uri(url);
                request.Content = formContent;
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                    try
                    {
                        returnResult = JObject.Parse(result);
                    }
                    catch (Exception e)
                    {
                        logger.LogInformation("SendRequestToAgent, Can not convert the response to json, result:" + result);
                        returnResult = result;
                    }
                }
                else
                {

                    if (retry)
                    {
                        try
                        {
                            var loggerURL = ServiceHelper.GetLogAPiURL("log_request_api_failed");
                            Dictionary<string, string> data = new Dictionary<string, string>()
                            {
                                {"functNme", funct },
                                {"result" , JsonConvert.SerializeObject(returnResult)},
                                {"requestUrl", url},
                                {"requestContent" ,JsonConvert.SerializeObject(parameter, Formatting.Indented)},
                                {"productShortName", vendor},
                                {"agentcode", agentCode }
                            };
                            _ = await SendRequestToAgent(factory, loggerURL, data, "POST", "", agentCode);
                        }
                        catch(Exception ex3)
                        {
                            logger.LogInformation("CAO!Resend Fail!!");
                            logger.LogError(ex3, ex3.Message);
                        }
                    }

                    logger.LogInformation($"Response failed. This fucking Url : {url} return a Status code {response.StatusCode}");
                    logger.LogInformation("Will resend the request to agent " + (retry ? " YES!" : "No"));
                    result = "";
                }


                if (isRecord)
                {
                    var loggerURL = ServiceHelper.GetLogAPiURL("log_agent_sdk");
                    Dictionary<string, string> data = new Dictionary<string, string>()
                        {
                            {"parameters", JsonConvert.SerializeObject(parameter, Formatting.Indented) },
                            {"result" , result},
                            {"api_url", url },
                            {"vendor", vendor },
                            {"agent_code", agentCode }
                        };
                    _ = SendRequestToAgent(factory, loggerURL, data, vendor, "POST", agentCode);

                }

                return returnResult;

            }
            catch (Exception e)
            {
                logger.LogInformation($"Sending API to {url} is failed.");
                logger.LogInformation($"Seding parameters is {string.Join(Environment.NewLine, parameter)}");
                logger.LogError(e, e.Message);
                logger.LogInformation("Will resend the request to agent " + (retry ? " YES!" : "No"));

                //TODO:: Need call Python SDK for retry.. 
                if (retry)
                {
                    try
                    {
                        var loggerURL = ServiceHelper.GetLogAPiURL("log_request_api_failed");
                        Dictionary<string, string> data = new Dictionary<string, string>()
                            {
                                {"functNme", funct},
                                {"result" , JsonConvert.SerializeObject(returnResult)},
                                {"requestUrl", url},
                                {"requestContent" ,JsonConvert.SerializeObject(parameter, Formatting.Indented)},
                                {"productShortName", vendor},
                                {"agentcode", agentCode }
                            };
                        _ = await SendRequestToAgent(factory, loggerURL, data, "POST", "", agentCode);
                    }
                    catch(Exception ex)
                    {
                        logger.LogInformation("CAO!Resend Fail!");
                        logger.LogError(ex,ex.Message);
                    }
                }

            }
            return returnResult;

        }

        public static async Task<dynamic> SendJsonRequestWithHeaderToGameAPI(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string vendor, string type, Dictionary<string, string> headers = null, bool isRecord = true, string function = null)
        {
            string jsonData = JsonConvert.SerializeObject(parameter);
            dynamic result = await SendRequestJsonToAsyncWithHeader(factory, url, jsonData, type, isRecord, headers, function);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendJsonRequestWithHeaderToGameAPI(IHttpClientFactory factory, string url, Dictionary<string, dynamic> parameter, string vendor, string type, Dictionary<string, string> headers = null, bool isRecord = true, string function = null)
        {
            string jsonData = JsonConvert.SerializeObject(parameter);
            dynamic result = await SendRequestJsonToAsyncWithHeader(factory, url, jsonData, type, isRecord, headers, function);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestJsonToAsyncWithHeader(IHttpClientFactory factory, string url, string parameter, string type, bool isRecord = false, Dictionary<string, string> header = null, string function = null)
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                var client = factory.CreateClient("YooClient");
                if (function == "balance")
                    client.Timeout = TimeSpan.FromSeconds(5);
                else
                    client.Timeout = TimeSpan.FromSeconds(30);

                using (var request = new HttpRequestMessage())
                {
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;
                    }
                    if (header != null)
                    {
                        foreach (KeyValuePair<string, string> entry in header)
                        {
                            request.Headers.Add(entry.Key, entry.Value);
                        }
                    }
                    request.RequestUri = new Uri(url);
                    request.Content = new StringContent(parameter, Encoding.UTF8, "application/json");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestJsonToAsync, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestJsonToAsyncWithHeader, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }

                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}",
                            vendor_return = returnResult
                        };
                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }

                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }

            return returnResult;
        }

        public static async Task<dynamic> SendRequestJsonWithHeaderAndGetOriginalResponse(IHttpClientFactory factory, string url, string parameter, string type, bool isRecord = false, Dictionary<string, string> header = null, string function = null)
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                var client = factory.CreateClient("YooClient");
                if (function == "balance")
                    client.Timeout = TimeSpan.FromSeconds(5);
                else
                    client.Timeout = TimeSpan.FromSeconds(30);

                using (var request = new HttpRequestMessage())
                {
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else if (type.ToLower() == "put") 
                    {
						request.Method = HttpMethod.Put;
					}
                    else
                    {
                        request.Method = HttpMethod.Post;
                    }
                    if (header != null)
                    {
                        foreach (KeyValuePair<string, string> entry in header)
                        {
                            request.Headers.Add(entry.Key, entry.Value);
                        }
                    }
                    request.RequestUri = new Uri(url);
                    request.Content = new StringContent(parameter, Encoding.UTF8, "application/json");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestJsonWithHeaderAndGetOriginalResponse 1 , Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestJsonWithHeaderAndGetOriginalResponse 2 , Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }

                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}",
                            vendor_return = returnResult
                        };
                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }

                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }

            return returnResult;
        }

        public static async Task<string> SendGetRequestWithHeader(IHttpClientFactory factor,  string url, Dictionary<string,string> parameter, Dictionary<string,string> headers)
        {
            var result = "";
            try
            {
                var client = factor.CreateClient();
                foreach (var kv in headers)
                {
                    client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }

                var builder = new UriBuilder(url);
                var query = HttpUtility.ParseQueryString(builder.Query);
                foreach (var item in parameter)
                {
                    query[item.Key] = item.Value;
                }
                builder.Query = query.ToString();
                url = builder.ToString();

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    dynamic errResponse = new
                    {
                        error = "network_error",
                        error_description = $"Status Code : {response.StatusCode}"
                    };

                    logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                    result = JsonConvert.SerializeObject(errResponse);
                }

            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            return result; 
        }

        public static async Task<string> SendRequestWithGetOriginalResponeToGameAPI(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string vendor, string type, bool isRecord = true, Dictionary<string, string> headers = null, bool isJson = false, string function = null)
        {
            var result = "";

            if (isJson == true)
            {
                string jsonData = JsonConvert.SerializeObject(parameter);
                result = await SendRequestJsonToAsyncWithHeader(factory, url, jsonData, type, isRecord, headers, function);
            }
            else
            {
               result = await SendRequestToAsyncWithGetOriginalData(factory, url, parameter, type, headers);
            }

            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<string> SendRequestToAsyncWithGetOriginalData(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string type, Dictionary<string, string> headers, bool isRecord = false)
        {
            string result = "";
            if (headers == null)
                headers = new Dictionary<string, string>();
            if (parameter == null)
                parameter = new Dictionary<string, string>();

            try
            {
                var client = factory.CreateClient("YooClient");
                using (var request = new HttpRequestMessage())
                {
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;

                    }
                    var value_list = new List<KeyValuePair<string, string>>();

                    foreach (KeyValuePair<string, string> entry in parameter)
                    {
                        value_list.Add(entry);
                    }

                    foreach (KeyValuePair<string, string> entry in headers)
                    {
                        request.Headers.Add(entry.Key, entry.Value);
                    }

                    var formContent = new FormUrlEncodedContent(value_list);
                  
                    request.Content = formContent;
                    request.RequestUri = new Uri(url);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                    else
                    {
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        result = JsonConvert.SerializeObject(errResponse);
                    }

                }
                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return result;
        }

        public static async Task<dynamic> SendRequestToAsyncWithhoutStatusCode(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string type, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new
            {
                error = 999,
                error_description = "HTTP Error"
            }; // todo: need change and add timeout return information
            try
            { 
                var client = factory.CreateClient("YooClient");
                using (var request = new HttpRequestMessage())
                {
                    var response = await client.GetAsync(url);
                    result = await response.Content.ReadAsStringAsync();
                    try
                    {
                        returnResult = JObject.Parse(result);
                    }
                    catch (Exception e)
                    {
                        logger.LogInformation("SendRequestToAsync, Can not convert the response to json, result:" + result);
                        returnResult = result;
                    }

                }
                return returnResult;
            }
            catch (Exception e)
            {
                logger.LogInformation("This fucking Url : " + url + " return a fucking Error Message: " + e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }

        public static async Task<dynamic> SendJsonRequestAllbetToGameAPI(IHttpClientFactory factory, string url, string parameter, string vendor, Dictionary<string, string> header, bool isRecord = true)
        {
            var result = await SendJsonRequestAllbet(factory, url, parameter, header, isRecord);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , parameter},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendJsonRequestAllbet(IHttpClientFactory factory, string url, string parameter, Dictionary<string, string> header, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                var client = factory.CreateClient("YooClient");
                using (var request = new HttpRequestMessage())
                {
                    request.Content = new StringContent(parameter);
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(url);

                    request.Headers.TryAddWithoutValidation("Authorization", header["Authorization"]);
                    request.Headers.TryAddWithoutValidation("Date", header["Date"]);
                    request.Content.Headers.TryAddWithoutValidation("Content-MD5", header["contentMD5"]);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            returnResult = result;
                        }
                    }
                    else
                    {
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };
                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                dynamic errResponse = new
                {
                    error = "network_error",
                    error_description = $"Network error : {e.Message}"
                }; 
                returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                return returnResult; 
            }

            return returnResult;
        }

        public static async Task<dynamic> SendRequestToGameAPI(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestToAsync(factory, url, parameter, type);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestToGameAPIGzip(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestToAsyncGzip(factory, url, parameter, type);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestJsonToGameAPI(IHttpClientFactory factory, string url, string parameter, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestJsonToAsync(factory, url, parameter, type);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , parameter},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestJsonToAsync(IHttpClientFactory factory, string url, string parameter, string type, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { };
            try
            { 
                var client = factory.CreateClient("YooClient");
                using (var request = new HttpRequestMessage())
                {
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else if (type.ToLower() == "put")
                    {
                        request.Method = HttpMethod.Put;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;

                    }

                    request.RequestUri = new Uri(url);
                    request.Content = new StringContent(parameter,
                                Encoding.UTF8,
                                "application/json");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestJsonToAsync, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestJsonWithHeaderAndGetOriginalResponse 2 , Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }

                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}",
                            vendor_return = returnResult
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }

                }
                return returnResult;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestJsonToAgent(IHttpClientFactory factory, string url, string parameter, string type, string vendor, string agentCode, bool retry = false, string funct = "")
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                var client = factory.CreateClient("YooClient");
                using (var request = new HttpRequestMessage())
                {
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else if (type.ToLower() == "put")
                    {
                        request.Method = HttpMethod.Put;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;

                    }

                    request.RequestUri = new Uri(url);
                    request.Content = new StringContent(parameter, Encoding.UTF8, "application/json");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                            if (DynamicHelper.HasProperty(returnResult, "Msg") && returnResult["Msg"] == "Failed" && retry)
                            {
                                try
                                {
                                    var loggerURL = ServiceHelper.GetLogAPiURL("notify_bethistory_failed");
                                    Dictionary<string, string> data = new Dictionary<string, string>()
                                    {
                                        {"functNme", funct },
                                        {"result" , JsonConvert.SerializeObject(returnResult)},
                                        {"requestUrl", url},
                                        {"requestContent" , parameter},
                                        {"productShortName", vendor},
                                        {"agentcode", agentCode }
                                    };
                                    _ = await SendRequestToAgent(factory, loggerURL, data, "POST", "", agentCode);
                                }
                                catch (Exception ex3)
                                {
                                    logger.LogInformation("Resend NOtify Fail!!!!");
                                    logger.LogError(ex3, ex3.Message);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (retry)
                            {
                                try
                                {
                                    var loggerURL = ServiceHelper.GetLogAPiURL("notify_bethistory_failed");
                                    Dictionary<string, string> data = new Dictionary<string, string>()
                                    {
                                        {"functNme", funct },
                                        {"result" , JsonConvert.SerializeObject(returnResult)},
                                        {"requestUrl", url},
                                        {"requestContent" , parameter},
                                        {"productShortName", vendor},
                                        {"agentcode", agentCode }
                                    };
                                    _ = await SendRequestToAgent(factory, loggerURL, data, "POST", "", agentCode);
                                }
                                catch (Exception ex3)
                                {
                                    logger.LogInformation("Resend NOtify Fail!!!");
                                    logger.LogError(ex3, ex3.Message);
                                }
                            }
                            logger.LogInformation("SendRequestJsonToAsync, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                    }
                    else
                    {
                        if (retry)
                        {
                            try
                            {
                                var loggerURL = ServiceHelper.GetLogAPiURL("notify_bethistory_failed");
                                Dictionary<string, string> data = new Dictionary<string, string>()
                                {
                                {"functNme", funct },
                                {"result" , JsonConvert.SerializeObject(returnResult)},
                                {"requestUrl", url},
                                {"requestContent" , parameter},
                                {"productShortName", vendor},
                                {"agentcode", agentCode }
                            };
                                _ = await SendRequestToAgent(factory, loggerURL, data, "POST", "", agentCode);
                            }
                            catch (Exception ex3)
                            {
                                logger.LogInformation("Resend NOtify Fail!!");
                                logger.LogError(ex3, ex3.Message);
                            }
                        }
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }

                }
                return returnResult;
            }
            catch (Exception e)
            {
                if (retry)
                {
                    try
                    {
                        var loggerURL = ServiceHelper.GetLogAPiURL("notify_bethistory_failed");
                        Dictionary<string, string> data = new Dictionary<string, string>()
                            {
                                {"functNme", funct},
                                {"result" , JsonConvert.SerializeObject(returnResult)},
                                {"requestUrl", url},
                                {"requestContent" , parameter},
                                {"productShortName", vendor},
                                {"agentcode", agentCode }
                            };
                        _ = await SendRequestToAgent(factory, loggerURL, data, "POST", "", agentCode);
                    }
                    catch (Exception ex)
                    {
                        logger.LogInformation("CAO! Resend NOtify Fail!");
                        logger.LogError(ex, ex.Message);
                    }
                }
                logger.LogError(e, e.Message);
            }
            return returnResult;
        }

        

        public static async Task<dynamic> SendRequestXmlToGameAPI(IHttpClientFactory factory, string url, string parameter, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestXmlToAsync(factory, url, parameter, type);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , parameter},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestXmlToAsync(IHttpClientFactory factory, string url, string parameter, string type, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                var client = factory.CreateClient("YooClient");
               
                using (var request = new HttpRequestMessage())
                {
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;

                    }

                    request.RequestUri = new Uri(url);
                    request.Content = new StringContent(parameter, Encoding.UTF8, "application/xml");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestJsonToAsync, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                    }
                    else
                    {
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }

                }
                return returnResult;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestToGameAPIWithHeaderYoo(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, Dictionary<string, string> header, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestToGameAPIWithHeader(factory, url, header, parameter, type);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestToGameAPIWithHeader(IHttpClientFactory factory, string url, Dictionary<string, string> header, Dictionary<string, string> parameter, string type = "POST")
        {
            dynamic returnResult = new { };
            string result;
            try
            {
                var client = factory.CreateClient("YooClient");
                using (var request = new HttpRequestMessage())
                {
                    var value_list = new List<KeyValuePair<string, string>>();
                    if (parameter != null)
                    {
                        foreach (KeyValuePair<string, string> entry in parameter)
                        {
                            value_list.Add(entry);
                        }
                    }
                    else
                    {
                        value_list = new();
                    }

                    foreach (KeyValuePair<string, string> entry in header)
                    {
                        request.Headers.Add(entry.Key, entry.Value);
                    }
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;
                    }

                    var formContent = new FormUrlEncodedContent(value_list);
                    request.RequestUri = new Uri(url);
                    request.Content = formContent;
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation($"Can not convert the response  | {result} | to json");
                            returnResult = result;
                        }
                    }
                    else
                    {
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestToReadTextAsync(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string type, string vendor, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { }; // todo: need change and add timeout return information
            try
            {
                var client = factory.CreateClient("YooClient");
                using (var request = new HttpRequestMessage())
                {

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        //StreamReader stream = new StreamReader(await response.Content.ReadAsStreamAsync());
                        using (StreamReader reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                        {
                            result = await reader.ReadToEndAsync();
                            logger.LogInformation(result.ToString());
                        }
                        //result = new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEnd();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestToAsync, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                    }
                    else
                    {
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }

                }

                if (isRecord)
                {
                    var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                    var logData = new Dictionary<string, string>()
                    {
                        {"parameters" , JsonConvert.SerializeObject(parameter)},
                        {"result" , returnResult},
                        {"api_url", url },
                        {"vendor", vendor }
                    };
                    _ = SendRequestToAsync(factory, logUrl, logData, "POST");
                }
                return returnResult;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }

        public static async Task<dynamic> SendTextRequestToGameAPI(IHttpClientFactory factory, string url, string parameter, string vendor, bool isRecord = true, string requestContent = "application/x-www-form-urlencoded")
        {
            var result = await SendTextRequestToAsync(factory, url, parameter, requestContent);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , parameter},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendTextRequestToAsync(IHttpClientFactory factory, string url, string parameter, string requestContent = "application/x-www-form-urlencoded")
        {
            string result = "";

            try
            {
                var client = factory.CreateClient("YooClient");
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(url);
                    request.Content = new StringContent(parameter, Encoding.UTF8, requestContent);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(requestContent);

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                    else
                    {
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        result = JsonConvert.SerializeObject(errResponse);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            return result;
        }

		public static async Task<dynamic> SendPureTextToVendor(IHttpClientFactory factory, string url, string data, Dictionary<string, string> headers = null, string vendor = "", string agentCode = "", bool isRecord = false)
		{
			string result = "";

			try
			{
				var client = factory.CreateClient("YooClient");
				using (var request = new HttpRequestMessage())
				{

					request.Method = HttpMethod.Post;
					request.RequestUri = new Uri(url);
					request.Content = new StringContent(data);
					request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

					if (headers != null)
					{
						foreach (KeyValuePair<string, string> entry in headers)
						{
							//value_list.Add(entry);
							request.Content.Headers.Add(entry.Key, entry.Value);
						}
					}
					var response = await client.SendAsync(request);

					if (response.IsSuccessStatusCode)
					{
						result = await response.Content.ReadAsStringAsync();
					}
					else
					{
						result = await response.Content.ReadAsStringAsync();
						dynamic errResponse = new
						{
							error = "network_error",
							error_description = $"Status Code : {response.StatusCode}",
							vendor_return = result
						};

						logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
						result = JsonConvert.SerializeObject(errResponse);
					}

				}

				bool is_getBalance = url.Contains("GetBalance");
				if (isRecord && !is_getBalance) //we don't need record get balance api 
				{
					var loggerURL = ServiceHelper.GetLogAPiURL("log_vendor_api");
					Dictionary<string, string> record_data = new Dictionary<string, string>()
					{
						{"parameters", JsonConvert.SerializeObject(new { PostData = data}, Formatting.Indented) },
						{"result" , result},
						{"api_url", url },
						{"vendor", vendor },
						{"agent_code", agentCode }
					};
					_ = Task.Run(() =>
					{
						try
						{
							SendRequestToAgent(factory, loggerURL, record_data, "POST", vendor, agentCode);
						}
						catch (Exception e)
						{
							// do nothing first
						}
					});
				}

				return result;
			}
			catch (Exception e)
			{
				logger.LogInformation($"Sending API to {url} is failed.");
				logger.LogInformation($"Seding parameters is {string.Join(Environment.NewLine, data)}");
				logger.LogError(e, e.Message);

				//TODO:: Need call Python SDK for retry.. 
			}
			return result;
		}


		public static async Task<dynamic> SendRequestJsonAndGetOriginalResponeToGameAPI(IHttpClientFactory factory, string url, string parameter, string vendor, string type, bool isRecord = true, Dictionary<string, string> headers = null, string function = null)
        {
            var result = await SendRequestJsonWithHeaderAndGetOriginalResponse(factory, url, parameter, type, isRecord, headers, function);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendJsonRequestWithEmptyParamsHeaderToGameAPI(IHttpClientFactory factory, string url, string parameter, string vendor, string type, Dictionary<string, string> headers = null, bool isRecord = true, string function = null)
        {
            dynamic result = await SendRequestJsonToAsyncWithHeader(factory, url, parameter, type, isRecord, headers, function);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestToGameAPIOriginalResponseWithHeaderYoo(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, Dictionary<string, string> header, string vendor, string type, bool isRecord = true, string function = null, string productShortname = "" )
        {
            var result = await SendRequestToGameAPIOriginalResponseWithHeader(factory, url, header, parameter, type, function, productShortname);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestToGameAPIOriginalResponseWithHeader(IHttpClientFactory factory, string url, Dictionary<string, string> header, Dictionary<string, string> parameter, string type = "POST", string function = null, string productShortname = "")
        {
            dynamic returnResult = new { };
            string result;
            try
            {
                var client = factory.CreateClient("YooClient");
                if (productShortname == "FG" && function == "get_bet_history")
                    client.Timeout = TimeSpan.FromSeconds(60);

                using (var request = new HttpRequestMessage())
                {
                    var value_list = new List<KeyValuePair<string, string>>();
                    if (parameter != null)
                    {
                        foreach (KeyValuePair<string, string> entry in parameter)
                        {
                            value_list.Add(entry);
                        }
                    }
                    else
                    {
                        value_list = new();
                    }

                    foreach (KeyValuePair<string, string> entry in header)
                    {
                        request.Headers.Add(entry.Key, entry.Value);
                    }
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;
                    }

                    var formContent = new FormUrlEncodedContent(value_list);
                    request.RequestUri = new Uri(url);
                    request.Content = formContent;
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation($"Can not convert the response  | {result} | to json");
                            returnResult = result;
                        }
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendRequestToGameAPIWithHeader, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}",
                            vendor_return = returnResult
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestToGameAPIWithHeaderGZIP(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, Dictionary<string, string> header, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestToGameAPIWithHeaderGZIP(factory, url, header, parameter, type);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestToGameAPIWithHeaderGZIP(IHttpClientFactory factory, string url, Dictionary<string, string> header, Dictionary<string, string> parameter, string type = "POST")
        {
            dynamic returnResult = new { };
            string result;
            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate

                };
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {

                    using (var request = new HttpRequestMessage())
                    {
                        var value_list = new List<KeyValuePair<string, string>>();
                        if (parameter != null)
                        {
                            foreach (KeyValuePair<string, string> entry in parameter)
                            {
                                value_list.Add(entry);
                            }
                        }
                        else
                        {
                            value_list = new();
                        }

                        foreach (KeyValuePair<string, string> entry in header)
                        {
                            request.Headers.Add(entry.Key, entry.Value);
                        }
                        if (type.ToLower() == "get")
                        {
                            request.Method = HttpMethod.Get;
                        }
                        else
                        {
                            request.Method = HttpMethod.Post;
                        }

                        var formContent = new FormUrlEncodedContent(value_list);
                        request.RequestUri = new Uri(url);
                        request.Content = formContent;
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsStringAsync();
                            try
                            {
                                returnResult = JObject.Parse(result);
                            }
                            catch (Exception e)
                            {
                                logger.LogInformation($"Can not convert the response  | {result} | to json");
                                returnResult = result;
                            }
                        }
                        else
                        {
                            dynamic errResponse = new
                            {
                                error = "network_error",
                                error_description = $"Status Code : {response.StatusCode}"
                            };

                            logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                            returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            return returnResult;
        }

		public static async Task<dynamic> SendTextRequestToAsync(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string type, bool isRecord = false, int timeexpiretype = 0)
		{
			string result;
			dynamic returnResult = new { }; // todo: need change and add timeout return information
			try
			{

				var client = factory.CreateClient("YooClient");

				if (timeexpiretype != 0)
				{
					client.Timeout = TimeSpan.FromSeconds(3);
				}
				else
				{
					client.Timeout = TimeSpan.FromSeconds(30);
				}

				using (var request = new HttpRequestMessage())
				{
					if (type.ToLower() == "get")
					{
						request.Method = HttpMethod.Get;
					}
					else
					{
						request.Method = HttpMethod.Post;

					}
					var value_list = new List<KeyValuePair<string, string>>();
					if (parameter != null)
					{
						foreach (KeyValuePair<string, string> entry in parameter)
						{
							value_list.Add(entry);
						}
					}
					else
					{
						value_list = new();
					}

					var formContent = new FormUrlEncodedContent(value_list);
					request.RequestUri = new Uri(url);
					request.Content = formContent;
					request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
					request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

					var response = await client.SendAsync(request);
					if (response.IsSuccessStatusCode)
					{
						result = await response.Content.ReadAsStringAsync();
						try
						{
							returnResult = JObject.Parse(result);

						}
						catch (Exception e)
						{
							logger.LogInformation("SendRequestToAsync, Can not convert the response to json, result:" + result);
							returnResult = result;
						}
					}
					else
					{
						dynamic errResponse = new
						{
							error = "network_error",
							error_description = $"Status Code : {response.StatusCode}"
						};

						logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
						returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
					}

				}
				return returnResult;

			}
			catch (Exception e)
			{
				logger.LogError(e, e.Message);
			}
			return returnResult;
		}

		public static async Task<dynamic> SendTextRequestToGameAPI(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string vendor, string type, bool isRecord = true)
		{
			var result = await SendTextRequestToAsync(factory, url, parameter, type);
			if (isRecord)
			{
				var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
				var logData = new Dictionary<string, string>()
				{
					{"parameters" , JsonConvert.SerializeObject(parameter)},
					{"result" , JsonConvert.SerializeObject(result)},
					{"api_url", url },
					{"vendor", vendor }
				};

				_ = SendTextRequestToAsync(factory, logUrl, logData, "POST");
			}
			return result;
		}

		public static async Task<dynamic> SendRequestToGameAPIWithHeaderMultipart(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, Dictionary<string, string> header, string vendor, string type, bool isRecord = true)
		{
			var result = await SendRequestToGameAPIWithMultipart(factory, url, header, parameter, type);
			if (isRecord)
			{
				var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
				var logData = new Dictionary<string, string>()
				{
					{"parameters" , JsonConvert.SerializeObject(parameter)},
					{"result" , JsonConvert.SerializeObject(result)},
					{"api_url", url },
					{"vendor", vendor }
				};

				_ = SendRequestToAsync(factory, logUrl, logData, "POST");
			}
			return result;
		}
		public static async Task<dynamic> SendRequestToGameAPIWithMultipart(IHttpClientFactory factory, string url, Dictionary<string, string> header, Dictionary<string, string> parameter, string type = "POST")
		{
			dynamic returnResult = new { };
			string result;
			try
			{
				var client = factory.CreateClient("YooClient");
				using (var request = new HttpRequestMessage())
				{
					foreach (KeyValuePair<string, string> entry in header)
					{
						request.Headers.Add(entry.Key, entry.Value);
					}

					MultipartFormDataContent formDataContent = new MultipartFormDataContent();

					if (parameter != null)
					{
						foreach (KeyValuePair<string, string> entry in parameter)
						{
							formDataContent.Add(new StringContent(entry.Value), entry.Key);
						}
					}

					if (type.ToLower() == "get")
					{
						request.Method = HttpMethod.Get;
					}
					else
					{
						request.Method = HttpMethod.Post;
					}

					request.RequestUri = new Uri(url);
					request.Content = formDataContent;

					var response = await client.SendAsync(request);

					if (response.IsSuccessStatusCode)
					{
						result = await response.Content.ReadAsStringAsync();
						try
						{
							returnResult = JObject.Parse(result);
						}
						catch (Exception e)
						{
							logger.LogInformation($"Can not convert the response | {result} | to json");
							returnResult = result;
						}
					}
					else
					{
						dynamic errResponse = new
						{
							error = "network_error",
							error_description = $"Status Code : {response.StatusCode}"
						};

						logger.LogInformation($"Response failed. This fucking Url : {url} return a fucking Status Code : {response.StatusCode}");
						returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
					}
				}
			}
			catch (Exception e)
			{
				logger.LogError(e, e.Message);
			}
			return returnResult;
		}

        public static async Task<dynamic> SendPostTextRequestToGameAPI(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string vendor, string type, bool isRecord = true)
        {
            var result = await SendPostTextRequestToAsync(factory, url, parameter, type);
            if (isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendPostTextRequestToAsync(factory, logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendPostTextRequestToAsync(IHttpClientFactory factory, string url, Dictionary<string, string> parameter, string type, bool isRecord = false, int timeexpiretype = 0)
        {
            string result;
            dynamic returnResult = new { }; // todo: need change and add timeout return information
            try
            {

                var client = factory.CreateClient("YooClient");

                if (timeexpiretype != 0)
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                }
                else
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                }

                using (var request = new HttpRequestMessage())
                {
                    if (type.ToLower() == "get")
                    {
                        request.Method = HttpMethod.Get;
                    }
                    else
                    {
                        request.Method = HttpMethod.Post;

                    }
                    var value_list = new List<KeyValuePair<string, string>>();
                    if (parameter != null)
                    {
                        foreach (KeyValuePair<string, string> entry in parameter)
                        {
                            value_list.Add(entry);
                        }
                    }
                    else
                    {
                        value_list = new();
                    }

                    var formContent = new FormUrlEncodedContent(value_list);
                    request.RequestUri = new Uri(url);
                    request.Content = formContent;
                    //request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);

                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendPostTextRequestToAsync, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync();
                        try
                        {
                            returnResult = JObject.Parse(result);
                        }
                        catch (Exception e)
                        {
                            logger.LogInformation("SendPostTextRequestToAsync, Can not convert the response to json, result:" + result);
                            returnResult = result;
                        }

                        dynamic errResponse = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}",
                            vendor_return = returnResult
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        returnResult = JObject.Parse(JsonConvert.SerializeObject(errResponse));
                    }

                }
                return returnResult;

            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            return returnResult;
        }
    }
}

