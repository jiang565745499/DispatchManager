using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Models.View;
using DispatchManager.DataAccess.FreeSql.Service;
using DispatchManager.Logging;
using DispatchManager.Schedule.Entitys;
using Kingdee.CDP.WebApi.SDK;
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
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DispatchManager.Schedule.Utils
{
    public class ScheduleKingDeeUtils
    {
        public static void AddTask(DispatchTaskKingDeeView task)
        {
            TaskServicesManager.GetOrAdd(task.dispatchTaskKingDee.Name, (provider, token) => Task.Run(() =>
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                try
                {
                    // 获取最新的任务视图，包含最新的Y9密钥
                    var updatedTask = GetUpdatedTaskKingDeeView(provider, task.dispatchTaskKingDee);
                    
                    // 金蝶
                    //读取配置，初始化SDK
                    K3CloudApi client = new K3CloudApi();
                    client.InitClient(acctID: updatedTask.KingDeeAccountID, appID: updatedTask.KingDeeAppID,
                        appSec: updatedTask.KingDeeAppSec, userName: updatedTask.KingDeeUserName,
                        lcid: Convert.ToInt32(updatedTask.KingDeeLCID), serverUrl: updatedTask.KingDeeServerUrl
                        );
                    //单据查询的请求参数
                    var param = new QueryParam()
                    {
                        FormId = updatedTask.dispatchTaskKingDee.KingDeeFormId,
                        FieldKeys = updatedTask.dispatchTaskKingDee.KingDeeFields,
                        FilterString = updatedTask.dispatchTaskKingDee.KingDeeFilterString,
                    };
                    //调用单据查询接口
                    var returnInfo = client.ExecuteBillQuery(param.ToJson());
                    if (returnInfo.Count > 0)
                    {
                        //对返回结果进行解析和校验，这里使用的是JsonPatch
                        var resultJObject = JArray.Parse(JsonConvert.SerializeObject(returnInfo));
                        if (updatedTask != null && updatedTask.dispatchTaskKingDee.IsLog == true)
                        {
                            LogHelperUtil.WriteInfo(new Logging.Custom.LogContent(resultJObject.ToString(), updatedTask.dispatchTaskKingDee.ID));
                        }
                        // 返回状态码
                        string statusCode = string.Empty;
                        var queryData = new Dictionary<string, string>
                        {
                            {"BodyData",resultJObject.ToString()}
                        };
                        var Str = ConnectUtil.PostResponseWithKey(updatedTask!, updatedTask!.dispatchTaskKingDee.ReturnApiUrl!, queryData, out statusCode, updatedTask.Y9Key!);
                    }
                    else
                    {
                        LogHelperUtil.WriteInfo(new Logging.Custom.LogContent(@$"金蝶接口返回异常,{updatedTask.dispatchTaskKingDee.Name}同步失败!", updatedTask.dispatchTaskKingDee.ID));
                    }
                }
                catch (Exception ex)
                {
                    LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0), ex);
                }

            }, token), TriggerBuilder.Build(Cron.ParseCronExpression(!string.IsNullOrEmpty(task.dispatchTaskKingDee.Trigger) ? task.dispatchTaskKingDee.Trigger : Cron.Minutely(5)).ToString())).Status = task.dispatchTaskKingDee.Status;
        }

        /// <summary>
        /// 获取最新的金蝶任务视图，包含最新的Y9密钥
        /// </summary>
        /// <param name="provider">服务提供者</param>
        /// <param name="task">任务信息</param>
        /// <returns>更新后的任务视图</returns>
        private static DispatchTaskKingDeeView GetUpdatedTaskKingDeeView(IServiceProvider provider, DispatchTaskKingDee task)
        {
            var taskView = new DispatchTaskKingDeeView(task);
            
            if (task.ClassID.HasValue)
            {
                // 从服务提供者中获取DispatchClassService
                var dispatchClassService = provider.GetRequiredService<DispatchClassService>();
                
                // 获取最新的DispatchClass信息
                var taskClass = dispatchClassService.GetDispatchClassByID(task.ClassID.Value);
                if (taskClass != null)
                {
                    taskView.Y9Key = taskClass.Y9Key;
                    taskView.KingDeeAccountID = taskClass.KingDeeAccountID;
                    taskView.KingDeeAppID = taskClass.KingDeeAppID;
                    taskView.KingDeeAppSec = taskClass.KingDeeAppSec;
                    taskView.KingDeeLCID = taskClass.KingDeeLCID;
                    taskView.KingDeeServerUrl = taskClass.KingDeeServerUrl;
                    taskView.KingDeeUserName = taskClass.KingDeeUserName;
                }
            }
            
            return taskView;
        }

        public static bool RequiredCronStr(string str)
        {
            bool flag = false;
            try
            {
                var Resultstr = Cron.ParseCronExpression(str);
                flag = true;
            }
            catch (Exception)
            {
                return false;
            }
            return flag;
        }
    }
}
