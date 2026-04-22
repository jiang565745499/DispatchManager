/**
 * @Auth JiangYQ
 * 
 **/

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using DispatchManager.Logging;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Net.Mime;
using BootstrapBlazor.Components;
using System.Net;
using DispatchManager.DataAccess.FreeSql.Models;
using System.Threading.Tasks;
using DispatchManager.DataAccess.FreeSql.Models.View;
using System.Threading;
using DispatchManager.DataAccess.FreeSql.Service;

namespace DispatchManager.Schedule.Utils
{
    /// <summary>
    /// 调用的工具类
    /// </summary>
    public class ConnectUtil
    {
        private static readonly HttpClient httpClient;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        static ConnectUtil()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                MaxConnectionsPerServer = 100
            };

            httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            httpClient.DefaultRequestHeaders.Add("User-Agent", "DispatchManager/1.0");
        }

        /// <summary>
        /// POST方式调用接口-默认urlencoded
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">请求body</param>
        /// <param name="statusCode">返回的状态码</param>
        /// <param name="Key">AES加密密钥</param>
        /// <param name="ContentType">body类型</param>
        /// <returns></returns>
        public static string PostResponseWithKey(DispatchTask dispatchTask, string url, Dictionary<string, string> postData, out string statusCode, string Key = "", string ContentType = "application/x-www-form-urlencoded")
        {
            return PostResponseWithKey(dispatchTask, url, postData, out statusCode, Key, ContentType, null);
        }

        public static string PostResponseWithKey(DispatchTask dispatchTask, string url, Dictionary<string, string> postData, out string statusCode, string Key, string ContentType, SqliteLogService? logService)
        {
            try
            {
                TimeSpan Stime = new TimeSpan(DateTime.Now.Ticks);
                LogHelperUtil.WriteDispatchTaskInfo(dispatchTask, logService, $@"POST请求接口:{url} 请求内容:{string.Join(", ", postData.Select(kvp => string.Join("-", kvp.Key, kvp.Value)))}");
                if (!string.IsNullOrEmpty(Key))
                {
                    var postDataEnum = postData.GetEnumerator();
                    while (postDataEnum.MoveNext())
                    {
                        postData[postDataEnum.Current.Key] = AesEncrypt(postDataEnum.Current.Value, Key);
                    }
                }
                string result = string.Empty;

                HttpContent httpContent = CreateHttpContent(postData, ContentType);

                var response = SendWithRetry(() => httpClient.PostAsync(url, httpContent)).Result;
                statusCode = response.StatusCode.ToString();
                if (response.IsSuccessStatusCode)
                {
                    TimeSpan ETime = new TimeSpan(DateTime.Now.Ticks);
                    result = response.Content.ReadAsStringAsync().Result;
                    LogHelperUtil.WriteDispatchTaskInfo(dispatchTask, logService, $@"接口:{url}返回 耗时{ETime.Subtract(Stime).Duration().TotalMilliseconds}毫秒 内容:{result}");
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteDispatchTaskError(dispatchTask, logService, ex.Message, ex);
                statusCode = HttpStatusCode.InternalServerError.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// 金蝶数据返回Y9
        /// </summary>
        /// <param name="dispatchTask"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="statusCode"></param>
        /// <param name="Key"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static string PostResponseWithKey(DispatchTaskKingDeeView dispatchTask, string url, Dictionary<string, string> postData, out string statusCode, string Key = "", string ContentType = "application/x-www-form-urlencoded")
        {
            try
            {
                TimeSpan Stime = new TimeSpan(DateTime.Now.Ticks);
                if (dispatchTask != null && dispatchTask.dispatchTaskKingDee.IsLog == true)
                    LogHelperUtil.WriteInterInfo(new Logging.Custom.LogContent($@"POST请求接口:{url} 请求内容:{string.Join(", ", postData.Select(kvp => string.Join("-", kvp.Key, kvp.Value)))}", dispatchTask.dispatchTaskKingDee.ID));
                if (!string.IsNullOrEmpty(Key))
                {
                    // 将postdata中的value加密
                    var postDataEnum = postData.GetEnumerator();
                    while (postDataEnum.MoveNext())
                    {
                        postData[postDataEnum.Current.Key] = AesEncrypt(postDataEnum.Current.Value, Key);
                    }
                }
                string result = string.Empty;

                HttpContent httpContent = CreateHttpContent(postData, ContentType);
                
                var response = SendWithRetry(() => httpClient.PostAsync(url, httpContent)).Result;
                // 输出Http响应状态码
                statusCode = response.StatusCode.ToString();
                // 确保Http响应成功
                if (response.IsSuccessStatusCode)
                {
                    TimeSpan ETime = new TimeSpan(DateTime.Now.Ticks);
                    // 异步读取json
                    result = response.Content.ReadAsStringAsync().Result;
                    if (dispatchTask != null && dispatchTask.dispatchTaskKingDee.IsLog == true)
                        LogHelperUtil.WriteInterInfo(new Logging.Custom.LogContent($@"接口:{url}返回 耗时{ETime.Subtract(Stime).Duration().TotalMilliseconds}毫秒 内容:{result}", dispatchTask.dispatchTaskKingDee.ID));
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent(ex.Message, 0), ex);
                statusCode = HttpStatusCode.InternalServerError.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// GET方式调用接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static string GetResponse(DispatchTask task, string url, out string statusCode, SqliteLogService? logService = null)
        {
            try
            {
                LogHelperUtil.WriteDispatchTaskInfo(task, logService, $@"Get请求接口:{url}");
                TimeSpan Stime = new TimeSpan(DateTime.Now.Ticks);
                // 将调用记录写入Log:
                string result = string.Empty;

                var response = SendWithRetry(() => httpClient.GetAsync(url)).Result;
                // 输出Http响应状态码
                statusCode = response.StatusCode.ToString();
                // 确保Http响应成功
                if (response.IsSuccessStatusCode)
                {
                    TimeSpan ETime = new TimeSpan(DateTime.Now.Ticks);
                    // 异步读取json
                    result = response.Content.ReadAsStringAsync().Result;
                    LogHelperUtil.WriteDispatchTaskInfo(task, logService, $@"接口:{url}返回 耗时{ETime.Subtract(Stime).Duration().TotalMilliseconds}毫秒 内容:{result}");
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteDispatchTaskError(task, logService, ex.Message, ex);
                statusCode = HttpStatusCode.InternalServerError.ToString();
                return string.Empty;
            }

        }

        /// <summary>
        /// POST方式异步调用接口-非加密-默认urlencoded
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static string PostResponse(DispatchTask dispatchTask, long? taskid, string url, Dictionary<string, string> postData, out string statusCode, string ContentType = "application/x-www-form-urlencoded", SqliteLogService? logService = null)
        {
            try
            {
                TimeSpan Stime = new TimeSpan(DateTime.Now.Ticks);
                LogHelperUtil.WriteDispatchTaskInfo(dispatchTask, logService, @$"POST请求接口:{url} 请求内容:{postData.ToStrings()}");
                string result = string.Empty;
                // 设置Http的正文
                HttpContent httpContent = CreateHttpContent(postData, ContentType);

                var response = SendWithRetry(() => httpClient.PostAsync(url, httpContent)).Result;
                // 输出Http响应状态码
                statusCode = response.StatusCode.ToString();
                // 确保Http响应成功
                if (response.IsSuccessStatusCode)
                {
                    TimeSpan ETime = new TimeSpan(DateTime.Now.Ticks);
                    // 异步读取json
                    result = response.Content.ReadAsStringAsync().Result;
                    LogHelperUtil.WriteDispatchTaskInfo(dispatchTask, logService, $@"接口:{url}返回 耗时{ETime.Subtract(Stime).Duration().TotalMilliseconds}毫秒 内容:{result}");
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteDispatchTaskError(dispatchTask, logService, ex.Message, ex);
                statusCode = HttpStatusCode.InternalServerError.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// POST方式异步调用接口-非加密-json方式
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static string PostResponse(DispatchTask dispatchTask, string url, Dictionary<string, string> postData, out string statusCode, SqliteLogService? logService = null)
        {
            try
            {
                TimeSpan Stime = new TimeSpan(DateTime.Now.Ticks);
                LogHelperUtil.WriteDispatchTaskInfo(dispatchTask, logService, $@"POST请求接口:{url} 请求内容:{postData.ToStrings()}");
                string result = string.Empty;
                var _ContentType = "application/json";

                HttpContent httpContent = CreateHttpContent(postData, _ContentType);

                var response = SendWithRetry(() => httpClient.PostAsync(url, httpContent)).Result;
                // 输出Http响应状态码
                statusCode = response.StatusCode.ToString();
                // 确保Http响应成功
                if (response.IsSuccessStatusCode)
                {
                    TimeSpan ETime = new TimeSpan(DateTime.Now.Ticks);
                    // 异步读取json
                    result = response.Content.ReadAsStringAsync().Result;
                    LogHelperUtil.WriteDispatchTaskInfo(dispatchTask, logService, $@"接口:{url}返回 耗时{ETime.Subtract(Stime).Duration().TotalMilliseconds}毫秒 内容:{result}");
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteDispatchTaskError(dispatchTask, logService, ex.Message, ex);
                statusCode = HttpStatusCode.InternalServerError.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// POST方式调用接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static string PostResponse(DispatchTask dispatchTask, long? taskid, string url, string postData, string _ContentType = "application/json", SqliteLogService? logService = null)
        {
            try
            {
                TimeSpan Stime = new TimeSpan(DateTime.Now.Ticks);
                LogHelperUtil.WriteDispatchTaskInfo(dispatchTask, logService, $@"Post请求接口:{url} 请求内容:{postData}");
                string result = string.Empty;
                // 设置Http的正文
                HttpContent httpContent = new StringContent(postData, Encoding.UTF8, _ContentType);
                // 设置Http的内容标头
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_ContentType);
                // 设置Http的内容标头的字符
                httpContent.Headers.ContentType.CharSet = "utf-8";

                using (var requestClient = CreateHttpClientWithHeaders(_ContentType))
                {
                    var response = SendWithRetry(() => requestClient.PostAsync(url, httpContent)).Result;
                    // 确保请求成功
                    response.EnsureSuccessStatusCode();
                    // 确保Http响应成功
                    if (response.IsSuccessStatusCode)
                    {
                        TimeSpan ETime = new TimeSpan(DateTime.Now.Ticks);
                        // 异步读取json
                        result = response.Content.ReadAsStringAsync().Result;
                        LogHelperUtil.WriteDispatchTaskInfo(dispatchTask, logService, $@"接口:{url}返回 耗时{ETime.Subtract(Stime).Duration().TotalMilliseconds}毫秒 内容:{result}");
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteDispatchTaskError(dispatchTask, logService, ex.Message, ex);
                return string.Empty;
            }
        }

        public static string PostResponseWithKey(string url, Dictionary<string, string> postData, out string statusCode, string Key = "", string ContentType = "application/x-www-form-urlencoded")
        {
            try
            {

                LogHelperUtil.WriteInterInfo(new Logging.Custom.LogContent($@"Post请求接口:{url} 请求内容:{string.Join(", ", postData.Select(kvp => string.Join("-", kvp.Key, kvp.Value)))}", 0));
                TimeSpan Stime = new TimeSpan(DateTime.Now.Ticks);
                if (!string.IsNullOrEmpty(Key))
                {
                    // 先获取所有键的副本（避免枚举时修改原集合）
                    var keys = postData.Keys.ToArray();

                    // 遍历键的副本，修改原集合
                    foreach (var key in keys)
                    {
                        postData[key] = AesEncrypt(postData[key], Key);
                    }
                }
                // 将调用记录写入Log:
                string result = string.Empty;

                HttpContent httpContent;
                if (ContentType.Equals("application/json"))
                {
                    string postDataConv = System.Text.Json.JsonSerializer.Serialize(postData);
                    httpContent = new StringContent(postDataConv, Encoding.UTF8, ContentType);
                }
                else
                {
                    // 设置Http的正文
                    httpContent = new FormUrlEncodedContent(postData);
                }
                // 设置Http的内容标头
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ContentType);
                // 设置Http的内容标头的字符
                httpContent.Headers.ContentType.CharSet = "utf-8";
                using (HttpClient httpClient = new HttpClient())
                {
                    // 异步Post
                    HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
                    // 输出Http响应状态码
                    statusCode = response.StatusCode.ToString();
                    // 确保Http响应成功
                    if (response.IsSuccessStatusCode)
                    {
                        TimeSpan ETime = new TimeSpan(DateTime.Now.Ticks);
                        // 异步读取json
                        result = response.Content.ReadAsStringAsync().Result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"加密调用接口:{url}时出错: {ex.Message}", 0), ex);
                statusCode = HttpStatusCode.InternalServerError.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="aeseninstr">要加密的字符串</param>
        /// <param name="secretkey">密钥</param>
        /// <returns></returns>
        public static string AesEncrypt(string str, string aesKey)
        {
            // 1. 校验输入
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (string.IsNullOrEmpty(aesKey)) return string.Empty;

            // 2. 获取密钥字节数组
            byte[] keyBytes = Encoding.UTF8.GetBytes(aesKey);

            // 3. 校验密钥字节长度（AES 必须 16/24/32 字节）
            int[] validKeySizes = { 16, 24, 32 };
            if (Array.IndexOf(validKeySizes, keyBytes.Length) < 0)
                return string.Empty;

            // 4. 要加密的是明文 str！
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;      // ECB 模式
            aes.Padding = PaddingMode.PKCS7;
            aes.BlockSize = 128;            // 固定

            // 5. 加密
            ICryptoTransform cTransform = aes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray);
        }

        /// <summary>
        /// 调用Y9方法
        /// </summary>
        /// <param name="error"></param>
        /// <param name="url"></param>
        /// <param name="postString"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static string UploadY9(ref string error, string url, string postString, string ContentType = "application/x-www-form-urlencoded")
        {
            string srcString = string.Empty;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(postString, Encoding.UTF8, ContentType)
                };

                using var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                srcString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                error = "【Y9HTTP】请求异常：" + ex.Message;
            }

            return srcString;
        }

        #region 私有方法

        /// <summary>
        /// 创建HTTP内容
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private static HttpContent CreateHttpContent(Dictionary<string, string> postData, string contentType)
        {
            HttpContent httpContent;
            if (contentType.Equals("application/json"))
            {
                var postDataConv = JsonSerializer.Serialize(postData);
                httpContent = new StringContent(postDataConv, Encoding.UTF8, contentType);
            }
            else
            {
                // 设置Http的正文
                httpContent = new FormUrlEncodedContent(postData);
            }
            // 设置Http的内容标头
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            // 设置Http的内容标头的字符
            httpContent.Headers.ContentType.CharSet = "utf-8";
            return httpContent;
        }

        /// <summary>
        /// 创建带头部的HttpClient
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private static HttpClient CreateHttpClientWithHeaders(string contentType)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(contentType));
            client.DefaultRequestHeaders.Add("User-Agent", "DispatchManager/1.0");

            return client;
        }

        /// <summary>
        /// 带重试机制的HTTP请求
        /// </summary>
        /// <param name="requestFunc"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> SendWithRetry(Func<Task<HttpResponseMessage>> requestFunc)
        {
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int retry = 0; retry < MaxRetries; retry++)
            {
                try
                {
                    response = await requestFunc();

                    // 对于5xx错误，进行重试
                    if ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
                    {
                        lastException = new Exception($"服务器错误: {response.StatusCode}");
                        await Task.Delay(RetryDelayMs * (retry + 1));
                        continue;
                    }

                    return response;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    await Task.Delay(RetryDelayMs * (retry + 1));
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                    await Task.Delay(RetryDelayMs * (retry + 1));
                }
            }

            if (lastException != null)
            {
                throw lastException;
            }

            throw new InvalidOperationException("HTTP请求未返回响应。");
        }

        #endregion
    }
}