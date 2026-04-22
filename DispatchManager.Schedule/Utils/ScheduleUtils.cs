using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Models.View;
using DispatchManager.DataAccess.FreeSql.Service;
using DispatchManager.Logging;
using DispatchManager.Schedule.Entitys;
using DispatchManager.Schedule.Service;
using Longbow.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Schedule.Utils
{
    public class ScheduleUtils
    {
        public static void AddTask(DispatchTaskView taskView)
        {
            var task = taskView.dispatchTask;
            var cronExpression = !string.IsNullOrEmpty(task.Trigger) ? task.Trigger : Cron.Minutely(5).ToString();
            
            TaskServicesManager.GetOrAdd(task.Name, (provider, token) => ExecuteTaskAsync(provider, task, token), 
                TriggerBuilder.Build(cronExpression)).Status = task.Status;
        }

        private static async Task ExecuteTaskAsync(IServiceProvider provider, DispatchTask task, CancellationToken token)
        {
            SqliteLogService? taskLogService = null;

            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            try
            {
                taskLogService = provider.GetRequiredService<SqliteLogService>();

                // 记录任务开始执行
                TaskStatisticsService.RecordTaskStart(task.Name);

                // 获取最新的Y9密钥
                var updatedTaskView = GetUpdatedTaskView(provider, task);
                
                if (task.IsDllTask)
                {
                    // 执行DLL任务
                    await ExecuteDllTaskAsync(updatedTaskView, taskLogService);
                }
                else
                {
                    // 执行API任务
                    var apiResult = await CallApiAsync(task, taskLogService);

                    // FISRetry：当响应 body 中包含 "status": 500 时，最多重试 2 次
                    if (apiResult != null && task.FISRetry && HasFISStatus500(apiResult))
                    {
                        const int maxFISRetries = 2;
                        for (int retryIndex = 1; retryIndex <= maxFISRetries; retryIndex++)
                        {
                            LogTaskInfo(taskLogService, task, $"FIS响应status为500，发起第{retryIndex}次重试");
                            apiResult = await CallApiAsync(task, taskLogService);
                            if (apiResult == null || !HasFISStatus500(apiResult))
                                break;
                        }
                    }

                    if (apiResult != null)
                    {
                        await ProcessApiResponseAsync(task, updatedTaskView, apiResult, taskLogService);
                    }
                    else
                    {
                        LogTaskInfo(taskLogService, task, "接口返回为空");
                    }
                }

                // 记录任务执行成功
                TaskStatisticsService.RecordTaskSuccess(task.Name);
            }
            catch (OperationCanceledException ex)
            {
                LogTaskError(taskLogService, task, $"任务被取消: {ex.Message}", ex, "TaskCancellation");
                TaskStatisticsService.RecordTaskFailure(task.Name, ex.Message);
            }
            catch (HttpRequestException ex)
            {
                LogTaskError(taskLogService, task, $"HTTP请求异常: {ex.Message}", ex, "HttpRequestError");
                TaskStatisticsService.RecordTaskFailure(task.Name, ex.Message);
            }
            catch (JsonException ex)
            {
                LogTaskError(taskLogService, task, $"JSON解析异常: {ex.Message}", ex, "JsonParseError");
                TaskStatisticsService.RecordTaskFailure(task.Name, ex.Message);
            }
            catch (Exception ex)
            {
                LogTaskError(taskLogService, task, $"任务执行异常: {ex.Message}", ex, "GeneralError");
                TaskStatisticsService.RecordTaskFailure(task.Name, ex.Message);
            }
        }

        /// <summary>
        /// 执行DLL任务
        /// </summary>
        /// <param name="task">任务信息</param>
        /// <returns></returns>
        private static async Task ExecuteDllTaskAsync(DispatchTaskView taskView, SqliteLogService? taskLogService)
        {
            var task = taskView.dispatchTask;
            if (string.IsNullOrEmpty(task.DllPath) || string.IsNullOrEmpty(task.MethodName))
            {
                throw new Exception("DLL路径或方法名不能为空");
            }

            var dllPath = ResolveToRunDirectoryPath(task.DllPath);

            if (!File.Exists(dllPath))
            {
                throw new Exception($"DLL文件不存在: {dllPath}");
            }

            try
            {
                // 加载DLL
                var assembly = System.Reflection.Assembly.LoadFrom(dllPath);
                
                // 查找包含指定方法的类型
                var types = assembly.GetTypes();
                object instance = null;
                System.Reflection.MethodInfo method = null;

                foreach (var type in types)
                {
                    method = type.GetMethod(task.MethodName);
                    if (method != null)
                    {
                        // 创建实例
                        instance = Activator.CreateInstance(type);
                        break;
                    }
                }

                if (method == null)
                {
                    throw new Exception($"在DLL中未找到方法: {task.MethodName}");
                }

                // 执行方法，传入taskView参数
                var result = method.Invoke(instance, [taskView]);
                
                // 如果方法返回Task，等待执行完成
                if (result is Task taskResult)
                {
                    await taskResult;
                }

                LogTaskInfo(taskLogService, task, $"DLL任务执行成功，方法: {task.MethodName}");
            }
            catch (Exception ex)
            {
                LogTaskError(taskLogService, task, $"DLL任务执行异常: {ex.Message}", ex, "DllTaskError");
                throw;
            }
        }

        private static string ResolveToRunDirectoryPath(string path)
        {
            return Path.IsPathRooted(path)
                ? path
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        }

        private static async Task<string> CallApiAsync(DispatchTask task, SqliteLogService? taskLogService)
        {
            var values = new Dictionary<string, string>();
            var statsCode = string.Empty;

            string apiResult;
            
            /// HTTPS特殊处理 适用于J9-Cloud
            if (task.ApiUrl.ToUpper().StartsWith("HTTPS://"))
            {
                apiResult = ConnectUtil.PostResponse(task, task.ID, task.ApiUrl, values, out statsCode, "application/json", taskLogService);
            }
            else
            {
                apiResult = ConnectUtil.PostResponse(task, task.ID, task.ApiUrl, values, out statsCode, "application/x-www-form-urlencoded", taskLogService);
            }

            if (apiResult != null && statsCode == HttpStatusCode.OK.ToString())
            {
                return apiResult;
            }

            LogTaskInfo(taskLogService, task, $"接口返回异常，状态码: {statsCode}");
            return null;
        }

        private static async Task ProcessApiResponseAsync(DispatchTask task, DispatchTaskView taskView, string apiResult, SqliteLogService? taskLogService)
        {
            try
            {
                JObject jsonObj = JObject.Parse(apiResult);
                
                // 处理接口错误
                if (!string.IsNullOrEmpty(jsonObj.SelectToken("error")?.ToString()))
                {
                    LogTaskInfo(taskLogService, task, $"接口返回异常,错误信息: {jsonObj.SelectToken("error")?.ToString()}");
                    return;
                }

                // 处理Y9返回内容
                if (jsonObj != null && jsonObj.ToString().Contains("msg") && jsonObj.SelectToken("msg")!.ToString().Contains("ds"))
                {
                    await ProcessY9ResponseAsync(task, taskView, jsonObj, taskLogService);
                }
                else
                {
                    // 处理普通回调
                    await ProcessNormalCallbackAsync(task, taskView, apiResult, taskLogService);
                }
            }
            catch (Exception ex)
            {
                // 处理解析异常
                await ProcessExceptionCallbackAsync(task, taskView, apiResult, ex, taskLogService);
            }
        }

        private static async Task ProcessY9ResponseAsync(DispatchTask task, DispatchTaskView taskView, JObject jsonObj, SqliteLogService? taskLogService)
        {
            Y9DSBase? dSBase = JsonConvert.DeserializeObject<Y9DSBase>(jsonObj.SelectToken("msg")!.SelectToken("ds")![0]!.ToString());
            if (dSBase != null && dSBase.status == "000000" && !string.IsNullOrEmpty(task.ReturnApiUrl))
            {
                await ProcessStandardY9ResponseAsync(task, taskView, jsonObj, taskLogService);
            }
        }

        private static async Task ProcessStandardY9ResponseAsync(DispatchTask task, DispatchTaskView taskView, JObject jsonObj, SqliteLogService? taskLogService)
        {
            string jsonStr = jsonObj.SelectToken("msg")!.SelectToken("ds1")!.ToString();
            ConnectUtil.PostResponse(task, task.ID, task.ReturnApiUrl, jsonStr, "application/json", taskLogService);
        }

        private static async Task ProcessNormalCallbackAsync(DispatchTask task, DispatchTaskView taskView, string apiResult, SqliteLogService? taskLogService)
        {
            if (string.IsNullOrEmpty(task.ReturnApiUrl))
                return;

            if (task.ResponseType == ResponseType.XML)
            {
                ConnectUtil.PostResponse(task, task.ID, task.ReturnApiUrl, apiResult, "application/xml", taskLogService);
            }
        }

        private static async Task ProcessExceptionCallbackAsync(DispatchTask task, DispatchTaskView taskView, string apiResult, Exception ex, SqliteLogService? taskLogService)
        {
            // 记录解析异常
            LogTaskError(taskLogService, task, $"API响应解析异常: {ex.Message}", ex, "ApiParseError");

            // 华朔特殊调用
            if (!string.IsNullOrEmpty(task.ReturnApiUrl))
            {
                if (task.ResponseType == ResponseType.XML)
                {
                    ConnectUtil.PostResponse(task, task.ID, task.ReturnApiUrl, apiResult, "application/xml", taskLogService);
                }
            }
        }

        /// <summary>
        /// 检查FIS响应body中是否包含 "status": 500
        /// </summary>
        private static bool HasFISStatus500(string apiResult)
        {
            try
            {
                var jsonObj = JObject.Parse(apiResult);
                var statusToken = jsonObj.SelectToken("status");
                return statusToken != null && statusToken.Value<int>() == 500;
            }
            catch
            {
                return false;
            }
        }

        private static void LogTaskInfo(SqliteLogService? logService, DispatchTask task, string message)
        {
            LogHelperUtil.WriteDispatchTaskInfo(task, logService, message);
        }

        private static void LogTaskError(SqliteLogService? logService, DispatchTask task, string message, Exception ex, string? category = null)
        {
            LogHelperUtil.WriteDispatchTaskError(task, logService, message, ex);
        }

        public static bool RequiredCronStr(string str)
        {
            try
            {
                Cron.ParseCronExpression(str);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取最新的任务视图，包含最新的Y9密钥
        /// </summary>
        /// <param name="provider">服务提供者</param>
        /// <param name="task">任务信息</param>
        /// <returns>更新后的任务视图</returns>
        private static DispatchTaskView GetUpdatedTaskView(IServiceProvider provider, DispatchTask task)
        {
            var taskView = new DispatchTaskView(task);
            
            if (task.ClassID.HasValue)
            {
                // 从服务提供者中获取DispatchClassService
                var dispatchClassService = provider.GetRequiredService<DispatchClassService>();
                
                // 获取最新的DispatchClass信息
                var taskClass = dispatchClassService.GetDispatchClassByID(task.ClassID.Value);
                if (taskClass != null)
                {
                    taskView.Y9Key = taskClass.Y9Key;
                }
            }
            
            return taskView;
        }
    }
}
