using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Models.View;
using DispatchManager.Logging;
using DispatchManager.Schedule.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DispatchManager.Core.Libs
{
    public class SyncHSBGYZ
    {
        private static bool TryGetMsgAndDs(DispatchTask task, JsonElement root, string content, out JsonElement msgElement, out JsonElement dsElement)
        {
            msgElement = default;
            dsElement = default;

            if (!root.TryGetProperty("msg", out msgElement) || msgElement.ValueKind != JsonValueKind.Object)
            {
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"Y9返回数据结构异常：msg不是对象，内容={content}", task.ID));
                return false;
            }

            if (!msgElement.TryGetProperty("ds", out dsElement) || dsElement.ValueKind != JsonValueKind.Array)
            {
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"Y9返回数据结构异常：msg.ds不是数组，内容={content}", task.ID));
                return false;
            }

            return true;
        }

        private static bool TryGetDs1FirstItem(DispatchTask task, JsonElement msgElement, out JsonElement firstDs1Item)
        {
            firstDs1Item = default;

            if (!msgElement.TryGetProperty("ds1", out var ds1Array) || ds1Array.ValueKind != JsonValueKind.Array)
            {
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"msg.ds1 类型异常，实际类型: {ds1Array.ValueKind}", task.ID));
                return false;
            }

            firstDs1Item = ds1Array.EnumerateArray().FirstOrDefault();
            if (firstDs1Item.ValueKind != JsonValueKind.Object)
            {
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"msg.ds1首项类型异常，实际类型: {firstDs1Item.ValueKind}", task.ID));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 同步 HSBG 数据
        /// </summary>
        /// <param name="y9Url">Y9 地址</param>
        /// <returns></returns>
        public async Task DisplayYZ1(DispatchTaskView taskView)
        {
            try
            {
                var task = taskView.dispatchTask;
                string y9Url = $"http://192.168.190.68:43068/PCodeClient/api.ashx?cmd=api568140";
                using var client = new HttpClient();
                {
                    string status = "000000";
                    do
                    {
                        status = string.Empty;
                        var values = new Dictionary<string, string>
                            {
                                {"FTaskNo","1"},
                                { "FTaskTotal","3"}
                            };
                        var statusCode = string.Empty;
                        var content = ConnectUtil.PostResponseWithKey(taskView.dispatchTask, y9Url, values, out statusCode, taskView.Y9Key ?? string.Empty);

                        // 解析返回的 JSON 数据
                        var jsonDocument = JsonDocument.Parse(content);
                        var root = jsonDocument.RootElement;

                        if (!TryGetMsgAndDs(task, root, content, out var msgElement, out var dsElement))
                        {
                            break;
                        }

                        // 检查 ds 的 status
                        var dsArray = dsElement.EnumerateArray();
                        foreach (var item in dsArray)
                        {
                            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("status", out var statusElement))
                            {
                                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds项缺少status字段或类型异常", task.ID));
                                continue;
                            }

                            status = statusElement.GetString() ?? string.Empty;
                            if (status == "000000")
                            {
                                // 检查是否存在 ds1
                                if (TryGetDs1FirstItem(task, msgElement, out var firstDs1Item))
                                {
                                    if (!firstDs1Item.TryGetProperty("FRequestUrl", out var requestUrlProp) ||
                                        !firstDs1Item.TryGetProperty("FRequestJson", out var requestJsonProp) ||
                                        !firstDs1Item.TryGetProperty("FSourceDeID", out var fSourceDeIDProp))
                                    {
                                        LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds1首项缺少必要字段(FRequestUrl/FRequestJson/FSourceDeID)", task.ID));
                                        break;
                                    }

                                    // 获取 FRequestUrl 和 FRequestJson
                                    var requestUrl = requestUrlProp.GetString();
                                    var requestJson = requestJsonProp.GetString();
                                    if (string.IsNullOrWhiteSpace(requestUrl) || string.IsNullOrWhiteSpace(requestJson))
                                    {
                                        LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds1首项字段值为空(FRequestUrl/FRequestJson)", task.ID));
                                        break;
                                    }

                                    // 处理 FSourceDeID 的类型转换
                                    string FSourceDeID;
                                    if (fSourceDeIDProp.ValueKind == JsonValueKind.Number)
                                    {
                                        FSourceDeID = fSourceDeIDProp.GetInt64().ToString();
                                    }
                                    else
                                    {
                                        FSourceDeID = fSourceDeIDProp.GetString() ?? string.Empty;
                                    }
                                    // 记录POST请求开始时间
                                    var postStartTime = DateTime.Now;
                                    // Console.WriteLine($"[{postStartTime:yyyy-MM-dd HH:mm:ss.fff}] 开始调用T100业务接口: {requestUrl}");
                                    // 向 FRequestUrl 发起 POST 请求
                                    var postContent = new StringContent(requestJson ?? string.Empty, Encoding.UTF8, "application/json");
                                    var postResponse = await client.PostAsync(requestUrl, postContent);
                                    var postResult = await postResponse.Content.ReadAsStringAsync();

                                    // 计算POST请求耗时
                                    var postElapsed = DateTime.Now - postStartTime;
                                    // Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] T100业务接口返回完成，耗时: {postElapsed.TotalMilliseconds:F2}ms");

                                    int lastIndex = postResult.LastIndexOf('}');
                                    if (lastIndex != -1)
                                    {
                                        postResult = postResult.Remove(lastIndex, 1);
                                    }
                                    postResult = postResult + $" ,\"FSourceDeID\":\"{FSourceDeID}\"";
                                    postResult = postResult + "}";

                                    LogHelperUtil.WriteInfo(new Logging.Custom.LogContent($"POST 请求结果: {postResult}", task.ID));

                                    // 记录回调Y9开始时间
                                    var callbackStartTime = DateTime.Now;

                                    // 再将返回的结果发送回 Y9
                                    await DisplayHD(task, postResult, 2);
                                }
                                break;
                            }
                        }
                    } while (status == "000000");
                }
            }
            catch (Exception ex)
            {
                //// Console.WriteLine($"同步 HSBG 数据时出错: {ex.Message}");
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"同步 DisplayYZ1 数据时出错: {ex.Message}", 0), ex);
            }
        }

        public async Task DisplayYZ2(DispatchTaskView taskView)
        {
            try
            {
                var task = taskView.dispatchTask;
                string y9Url = $"http://192.168.190.68:43068/PCodeClient/api.ashx?cmd=api568140";
                using var client = new HttpClient();
                {
                    string status = "000000";
                    do
                    {
                        status = string.Empty;
                        var values = new Dictionary<string, string>
                            {
                                {"FTaskNo","2"},
                                { "FTaskTotal","3"}
                            };
                        var statusCode = string.Empty;
                        var content = ConnectUtil.PostResponseWithKey(taskView.dispatchTask, y9Url, values, out statusCode, taskView.Y9Key ?? string.Empty);

                        // 解析返回的 JSON 数据
                        var jsonDocument = JsonDocument.Parse(content);
                        var root = jsonDocument.RootElement;

                        if (!TryGetMsgAndDs(task, root, content, out var msgElement, out var dsElement))
                        {
                            break;
                        }

                        // 检查 ds 的 status
                        var dsArray = dsElement.EnumerateArray();
                        foreach (var item in dsArray)
                        {
                            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("status", out var statusElement))
                            {
                                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds项缺少status字段或类型异常", task.ID));
                                continue;
                            }

                            status = statusElement.GetString() ?? string.Empty;
                            if (status == "000000")
                            {
                                // 检查是否存在 ds1
                                if (TryGetDs1FirstItem(task, msgElement, out var firstDs1Item))
                                {
                                    if (!firstDs1Item.TryGetProperty("FRequestUrl", out var requestUrlProp) ||
                                        !firstDs1Item.TryGetProperty("FRequestJson", out var requestJsonProp) ||
                                        !firstDs1Item.TryGetProperty("FSourceDeID", out var fSourceDeIDProp))
                                    {
                                        LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds1首项缺少必要字段(FRequestUrl/FRequestJson/FSourceDeID)", task.ID));
                                        break;
                                    }

                                    // 获取 FRequestUrl 和 FRequestJson
                                    var requestUrl = requestUrlProp.GetString();
                                    var requestJson = requestJsonProp.GetString();
                                    if (string.IsNullOrWhiteSpace(requestUrl) || string.IsNullOrWhiteSpace(requestJson))
                                    {
                                        LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds1首项字段值为空(FRequestUrl/FRequestJson)", task.ID));
                                        break;
                                    }

                                    // 处理 FSourceDeID 的类型转换
                                    string FSourceDeID;
                                    if (fSourceDeIDProp.ValueKind == JsonValueKind.Number)
                                    {
                                        FSourceDeID = fSourceDeIDProp.GetInt64().ToString();
                                    }
                                    else
                                    {
                                        FSourceDeID = fSourceDeIDProp.GetString() ?? string.Empty;
                                    }
                                    // 记录POST请求开始时间
                                    var postStartTime = DateTime.Now;
                                    // Console.WriteLine($"[{postStartTime:yyyy-MM-dd HH:mm:ss.fff}] 开始调用T100业务接口: {requestUrl}");
                                    // 向 FRequestUrl 发起 POST 请求
                                    var postContent = new StringContent(requestJson ?? string.Empty, Encoding.UTF8, "application/json");
                                    var postResponse = await client.PostAsync(requestUrl, postContent);
                                    var postResult = await postResponse.Content.ReadAsStringAsync();

                                    // 计算POST请求耗时
                                    var postElapsed = DateTime.Now - postStartTime;
                                    // Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] T100业务接口返回完成，耗时: {postElapsed.TotalMilliseconds:F2}ms");

                                    int lastIndex = postResult.LastIndexOf('}');
                                    if (lastIndex != -1)
                                    {
                                        postResult = postResult.Remove(lastIndex, 1);
                                    }
                                    postResult = postResult + $" ,\"FSourceDeID\":\"{FSourceDeID}\"";
                                    postResult = postResult + "}";

                                    LogHelperUtil.WriteInfo(new Logging.Custom.LogContent($"POST 请求结果: {postResult}", task.ID));

                                    // 记录回调Y9开始时间
                                    var callbackStartTime = DateTime.Now;

                                    // 再将返回的结果发送回 Y9
                                    await DisplayHD(task, postResult, 2);
                                }
                                break;
                            }
                        }
                    } while (status == "000000");
                }
            }
            catch (Exception ex)
            {
                //// Console.WriteLine($"同步 HSBG 数据时出错: {ex.Message}");
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"同步 DisplayYZ2 数据时出错: {ex.Message}", 0), ex);
            }
        }


        public async Task DisplayYZ3(DispatchTaskView taskView)
        {
            try
            {
                var task = taskView.dispatchTask;
                string y9Url = $"http://192.168.190.68:43068/PCodeClient/api.ashx?cmd=api568140";
                using var client = new HttpClient();
                {
                    string status = "000000";
                    do
                    {
                        status = string.Empty;
                        var values = new Dictionary<string, string>
                            {
                                {"FTaskNo","3"},
                                { "FTaskTotal","3"}
                            };
                        var statusCode = string.Empty;
                        var content = ConnectUtil.PostResponseWithKey(taskView.dispatchTask, y9Url, values, out statusCode, taskView.Y9Key ?? string.Empty);

                        // 解析返回的 JSON 数据
                        var jsonDocument = JsonDocument.Parse(content);
                        var root = jsonDocument.RootElement;

                        if (!TryGetMsgAndDs(task, root, content, out var msgElement, out var dsElement))
                        {
                            break;
                        }

                        // 检查 ds 的 status
                        var dsArray = dsElement.EnumerateArray();
                        foreach (var item in dsArray)
                        {
                            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("status", out var statusElement))
                            {
                                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds项缺少status字段或类型异常", task.ID));
                                continue;
                            }

                            status = statusElement.GetString() ?? string.Empty;
                            if (status == "000000")
                            {
                                // 检查是否存在 ds1
                                if (TryGetDs1FirstItem(task, msgElement, out var firstDs1Item))
                                {
                                    if (!firstDs1Item.TryGetProperty("FRequestUrl", out var requestUrlProp) ||
                                        !firstDs1Item.TryGetProperty("FRequestJson", out var requestJsonProp) ||
                                        !firstDs1Item.TryGetProperty("FSourceDeID", out var fSourceDeIDProp))
                                    {
                                        LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds1首项缺少必要字段(FRequestUrl/FRequestJson/FSourceDeID)", task.ID));
                                        break;
                                    }

                                    // 获取 FRequestUrl 和 FRequestJson
                                    var requestUrl = requestUrlProp.GetString();
                                    var requestJson = requestJsonProp.GetString();
                                    if (string.IsNullOrWhiteSpace(requestUrl) || string.IsNullOrWhiteSpace(requestJson))
                                    {
                                        LogHelperUtil.WriteInterError(new Logging.Custom.LogContent("msg.ds1首项字段值为空(FRequestUrl/FRequestJson)", task.ID));
                                        break;
                                    }

                                    // 处理 FSourceDeID 的类型转换
                                    string FSourceDeID;
                                    if (fSourceDeIDProp.ValueKind == JsonValueKind.Number)
                                    {
                                        FSourceDeID = fSourceDeIDProp.GetInt64().ToString();
                                    }
                                    else
                                    {
                                        FSourceDeID = fSourceDeIDProp.GetString() ?? string.Empty;
                                    }
                                    // 记录POST请求开始时间
                                    var postStartTime = DateTime.Now;
                                    // Console.WriteLine($"[{postStartTime:yyyy-MM-dd HH:mm:ss.fff}] 开始调用T100业务接口: {requestUrl}");
                                    // 向 FRequestUrl 发起 POST 请求
                                    var postContent = new StringContent(requestJson ?? string.Empty, Encoding.UTF8, "application/json");
                                    var postResponse = await client.PostAsync(requestUrl, postContent);
                                    var postResult = await postResponse.Content.ReadAsStringAsync();

                                    // 计算POST请求耗时
                                    var postElapsed = DateTime.Now - postStartTime;
                                    // Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] T100业务接口返回完成，耗时: {postElapsed.TotalMilliseconds:F2}ms");

                                    int lastIndex = postResult.LastIndexOf('}');
                                    if (lastIndex != -1)
                                    {
                                        postResult = postResult.Remove(lastIndex, 1);
                                    }
                                    postResult = postResult + $" ,\"FSourceDeID\":\"{FSourceDeID}\"";
                                    postResult = postResult + "}";

                                    LogHelperUtil.WriteInfo(new Logging.Custom.LogContent($"POST 请求结果: {postResult}", task.ID));

                                    // 记录回调Y9开始时间
                                    var callbackStartTime = DateTime.Now;

                                    // 再将返回的结果发送回 Y9
                                    await DisplayHD(task, postResult, 2);
                                }
                                break;
                            }
                        }
                    } while (status == "000000");
                }
            }
            catch (Exception ex)
            {
                //// Console.WriteLine($"同步 HSBG 数据时出错: {ex.Message}");
                LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"同步 DisplayYZ3 数据时出错: {ex.Message}", 0), ex);
            }
        }




        public static async Task<string> DisplayHD(DispatchTask task, string postResult, int retryCount)
        {
            if (retryCount < 0)
                return "-1";
            retryCount--;
            using var client = new HttpClient();
            {
                TimeSpan Stime = new TimeSpan(DateTime.Now.Ticks);
                var ResultLast = await client.PostAsync("http://192.168.190.68:43068/PCodeClient/api.ashx?cmd=api568143", new StringContent(postResult, Encoding.UTF8, "application/json"));
                TimeSpan ETime = new TimeSpan(DateTime.Now.Ticks);
                // 获取回调结果内容
                var resultContent = await ResultLast.Content.ReadAsStringAsync();
                LogHelperUtil.WriteInterInfo(new Logging.Custom.LogContent($@"接口:http://192.168.190.68:43068/PCodeClient/api.ashx?cmd=api568143返回 耗时{ETime.Subtract(Stime).Duration().TotalMilliseconds}毫秒 内容:{resultContent}", task.ID));
                if (!ResultLast.IsSuccessStatusCode || resultContent.Contains("重复执行"))
                {
                    return await DisplayHD(task, postResult, retryCount);
                }
                else
                {
                    var jsonDocument = JsonDocument.Parse(resultContent);
                    var root = jsonDocument.RootElement;

                    if (!root.TryGetProperty("msg", out var msgElement) || msgElement.ValueKind != JsonValueKind.Object)
                    {
                        LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"回调返回结构异常：msg不是对象，内容={resultContent}", task.ID));
                        return "-1";
                    }

                    if (!msgElement.TryGetProperty("ds", out var dsElement) || dsElement.ValueKind != JsonValueKind.Array)
                    {
                        LogHelperUtil.WriteInterError(new Logging.Custom.LogContent($"回调返回结构异常：msg.ds不是数组，内容={resultContent}", task.ID));
                        return "-1";
                    }

                    // 检查 ds 的 status
                    var dsArray = dsElement.EnumerateArray();
                    foreach (var item in dsArray)
                    {
                        if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("status", out var statusElement))
                        {
                            return statusElement.GetString() ?? "";
                        }
                    }
                }
            }
            return "-1";
        }


    }
}
