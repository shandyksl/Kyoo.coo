using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yoo.Core.Common.Helper
{
    public class HttpClientService
    {
        private static ILogger logger = ApplicationLogging.CreateLogger<HttpClientService>();

        //funct 参数应放空，除非需要使用CNM 重新发过
        public static async Task<dynamic> SendRequestToAgent(string url, Dictionary<string,string> parameter, string type, string vendor = "",string agentCode="", bool isRecord = false, bool retry = false, string funct = "") 
        {
            string result;
            dynamic returnResult = new { };
            HttpResponseMessage response =null;
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };
               
                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30); //30s timeout to avoid performance issue ;
                    
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
                        returnResult = new
                        {
                            error = "network_error",
                            error_description = $"Status Code : {response.StatusCode}"
                        };

                        logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                        result = JsonConvert.SerializeObject(returnResult);

                        if (retry && !String.IsNullOrEmpty(funct))
                        {
                            var loggerURL = ServiceHelper.GetLogAPiURL("log_request_api_failed");
                            Dictionary<string, string> data = new Dictionary<string, string>()
                            {
                                {"functNme", funct},
                                {"responseContent" , result},
                                {"requestUrl", url},
                                {"requestContent" ,JsonConvert.SerializeObject(parameter, Formatting.Indented)},
                                {"productShortName", vendor},
                                {"agentcode", agentCode }
                            };
                            _ = await SendRequestToAgent(loggerURL, data, "POST");
                        }

                        
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
                            {"agentcode", agentCode }
                        };
                        _ = SendRequestToAgent(loggerURL, data, vendor, "POST", agentCode);

                    }

                    return returnResult;
                }
            }
            catch(Exception e){
                logger.LogInformation($"Sending API to {url} is failed.");
                logger.LogInformation($"Seding parameters is {string.Join(Environment.NewLine,parameter)}");
                logger.LogError(e,e.Message);

                //TODO:: Need call Python SDK for retry.. 
                if (retry && !string.IsNullOrEmpty(funct))  {
                    var loggerURL = ServiceHelper.GetLogAPiURL("log_request_api_failed");
                    Dictionary<string, string> data = new Dictionary<string, string>()
                            {
                                {"functNme", funct},
                                {"responseContent" , ""},
                                {"requestUrl", url},
                                {"requestContent" ,JsonConvert.SerializeObject(parameter, Formatting.Indented)},
                                {"productShortName", vendor},
                                {"agentcode", agentCode }
                            };
                    _ = await SendRequestToAgent(loggerURL, data, "POST");
                }

            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestJsonToGameAPI(string url, string parameter, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestJsonToAsync(url, parameter, type);
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
                        
                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestJsonToAsync(string url, string parameter, string type, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30); //30s timeout to avoid performance issue ;

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
                               // logger.LogInformation("SendRequestJsonToAsync, Can not convert the response to json, result:" + result);
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
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestJsonToAsyncWithHeader(string url, string parameter, string type, bool isRecord = false, Dictionary<string, string> header = null)
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30); //5s timeout to avoid performance issue ;

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
                        foreach (KeyValuePair<string, string> entry in header)
                        {
                            request.Headers.Add(entry.Key, entry.Value);
                        }
                        request.RequestUri = new Uri(url);
                        request.Content = new StringContent(parameter, Encoding.UTF8,"application/json");
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

                //TODO:: Need call Python SDK for retry.. 
            }

            return returnResult;
        }

        public static async Task<dynamic> SendRequestXmlToGameAPI(string url, string parameter, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestXmlToAsync(url, parameter, type);
            if (isRecord )
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , parameter},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestXmlToAsync(string url, string parameter, string type, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression =  DecompressionMethods.GZip | DecompressionMethods.Deflate

                };
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30); //5s timeout to avoid performance issue ;

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
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestToGameAPI(string url, Dictionary<string, string> parameter, string vendor, string type, bool isRecord= true)
        {
            var result = await SendRequestToAsync(url, parameter, type);
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

                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestToGameAPIWithHeaderYoo(string url, Dictionary<string, string> parameter, Dictionary<string, string> header, string vendor, string type, bool isRecord = true)
        {
            var result = await SendRequestToGameAPIWithHeader(url, header, parameter, type);
            if (false && isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , JsonConvert.SerializeObject(parameter)},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestWithGetOriginalResponeToGameAPIWithContext(string url, Dictionary<string, dynamic> parameter, string vendor, string type, bool isRecord = true, Dictionary<string, string> headers = null, bool isJson = false)
        {
            dynamic result = null;

            if (isJson == true)
            {
                string jsonData = JsonConvert.SerializeObject(parameter);
                result = await SendRequestJsonToAsyncWithHeader(url, jsonData, type, isRecord, headers);
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

                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }


        public static async Task<string> SendRequestWithGetOriginalResponeToGameAPI(string url, Dictionary<string, string> parameter, string vendor, string type, bool isRecord = true, Dictionary<string, string> headers = null, bool isJson = false)
        {
            var result = "";

            if(isJson == true)
            {
                string jsonData = JsonConvert.SerializeObject(parameter);
                result = await SendRequestJsonToAsyncWithHeader(url, jsonData, type, isRecord, headers);
            }
            else
            {
                result = await SendRequestToAsyncWithGetOriginalData(url, parameter, type, headers);
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

                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendRequestToAsync(string url, Dictionary<string, string> parameter, string type, bool isRecord = false) 
        {
            string result;
            dynamic returnResult = new { }; // todo: need change and add timeout return information
            try {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(60); //30s timeout to avoid performance issue ;

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
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestToAsyncWithhoutStatusCode(string url, Dictionary<string, string> parameter, string type, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new
            {
                error = 999,
                error_description = "HTTP Error"
            }; // todo: need change and add timeout return information
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30); //30s timeout to avoid performance issue ;

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
            }
            catch (Exception e)
            {
                logger.LogInformation("This fucking Url : " + url + " return a fucking Error Message: " + e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }

        public static async Task<dynamic> SendRequestToReadTextAsync(string url, Dictionary<string, string> parameter, string type, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { }; // todo: need change and add timeout return information
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30); //5s timeout to avoid performance issue ;

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
                    return returnResult;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return returnResult;
        }


        public static async Task<string> SendRequestToAsyncWithGetOriginalData(string url, Dictionary<string, string> parameter, string type, Dictionary<string, string> headers, bool isRecord = false)
        {
            string result = "";
            if (headers == null)
                headers = new Dictionary<string, string>();
            if(parameter == null)
                parameter = new Dictionary<string, string>();

            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30); //5s timeout to avoid performance issue ;

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
                        request.RequestUri = new Uri(url);
                        request.Content = formContent;
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
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);

                //TODO:: Need call Python SDK for retry.. 
            }
            return result;
        }

        public static async Task<dynamic> SendPureTextToVendor(string url, string data, Dictionary<string,string> headers = null,  string vendor = "", string agentCode = "", bool isRecord = false)
        {
            string result = "";
 
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    //AddDefaultHeaders(client);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                    client.Timeout = TimeSpan.FromSeconds(30); //30s timeout to avoid performance issue ;

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
                            dynamic errResponse = new
                            {
                                error = "network_error",
                                error_description = $"Status Code : {response.StatusCode}"
                            };

                            logger.LogInformation($"Response failed. This fucking Url : {url}  return a fucking Status Code : {response.StatusCode}");
                            result = JsonConvert.SerializeObject(errResponse);
                        }

                    }

                    bool is_getBalance = url.Contains("GetBalance");
                    if (isRecord && !is_getBalance) //we don't need record get balance api 
                    {
                        var loggerURL = ServiceHelper.GetLogAPiURL("log_agent_sdk");
                        Dictionary<string, string> record_data = new Dictionary<string, string>()
                        {
                            {"parameters", JsonConvert.SerializeObject(new { PostData = data}, Formatting.Indented) },
                            {"result" , result},
                            {"api_url", url },
                            {"vendor", vendor },
                            {"agentcode", agentCode }
                        };
                        _ = Task.Run(() =>
                        {
                            try
                            {
                                  SendRequestToAgent(loggerURL, record_data, vendor, "POST", agentCode);
                            }
                            catch(Exception e)
                            {
                                // do nothing first
                            }
                        });
                    }

                    return result;
                }
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

        public static async Task<dynamic> SendRequestToGameAPIWithHeader(string url, Dictionary<string, string> header, Dictionary<string, string> parameter, string type = "POST")
        {
            dynamic returnResult = new { };
            string result; 
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };
                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                     

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

                        foreach (KeyValuePair<string,string> entry in header)
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


        public static async Task<dynamic> SendTextRequestToGameAPI(string url, string parameter, string vendor,  bool isRecord = true , string requestContent = "application/x-www-form-urlencoded")
        {
            var result = await SendTextRequestToAsync(url, parameter, requestContent);
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

                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendTextRequestToAsync(string url, string parameter, string requestContent = "application/x-www-form-urlencoded")
        {
            string result = "";          
            
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30); 

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
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            return result;
        }

        public static async Task<dynamic> SendJsonRequestWithHeaderToGameAPI(string url, Dictionary<string, string> parameter, string vendor, string type, Dictionary<string, string> headers = null, bool isRecord = true)
        {
            string jsonData = JsonConvert.SerializeObject(parameter);
            dynamic result = await SendRequestJsonToAsyncWithHeader(url, jsonData, type, isRecord, headers);
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

                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }

        public static async Task<dynamic> SendJsonRequestAllbetToGameAPI(string url, string parameter, string vendor, Dictionary<string, string> header, bool isRecord = true)
        {
            var result = await SendJsonRequestAllbet(url, parameter,header, isRecord);
            if ( isRecord)
            {
                var logUrl = ServiceHelper.GetLogAPiURL("log_vendor_api");
                var logData = new Dictionary<string, string>()
                {
                    {"parameters" , parameter},
                    {"result" , JsonConvert.SerializeObject(result)},
                    {"api_url", url },
                    {"vendor", vendor }
                };

                _ = SendRequestToAsync(logUrl, logData, "POST");
            }
            return result;
        }
        public static async Task<dynamic> SendJsonRequestAllbet(string url, string parameter, Dictionary<string, string> header, bool isRecord = false)
        {
            string result;
            dynamic returnResult = new { };
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };

                using (var client = HttpClientFactory.Create(handler))
                {
                    AddDefaultHeaders(client);
                    client.Timeout = TimeSpan.FromSeconds(30);

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
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);               
            }

            return returnResult;
        }

        private static void AddDefaultHeaders(HttpClient httpClient)
        {
            //httpClient.DefaultRequestHeaders.Add("content-type", "application/x-www-form-urlencoded");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
                
        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

            // Copy the request's content (via a MemoryStream) into the cloned object
            var ms = new MemoryStream();
            if (req.Content != null)
            {
                await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);
            }


            clone.Version = req.Version;
            foreach (KeyValuePair<string, object> option in req.Options) 
                clone.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);  
            
           // foreach (KeyValuePair<string, object> prop in req.Properties)
               // clone.Properties.Add(prop);

            foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;
        }
    }
}
