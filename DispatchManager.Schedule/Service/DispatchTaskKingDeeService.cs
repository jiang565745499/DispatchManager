using DispatchManager.DataAccess.FreeSql.Models.View;
using DispatchManager.DataAccess.FreeSql.Service;
using DispatchManager.Logging;
using DispatchManager.Schedule.Utils;
using Longbow.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Schedule.Service
{
    /// <summary>
    /// 金蝶Service实现
    /// </summary>
    public class DispatchTaskKingDeeService : BackgroundService
    {
        public readonly IFreeSql freeSql;

        public DispatchTaskKingDeeService([FromKeyedServices("MainDB")] IFreeSql _freeSql)
        {
            freeSql = _freeSql;
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(() =>
        {
            #region 内置任务 参考
            //TaskServicesManager.GetOrAdd("单次任务", (provider, token) => Task.Delay(1000, token));
            //TaskServicesManager.GetOrAdd("周期任务", (provider, token) => Task.Delay(1000, token), TriggerBuilder.Default.WithInterval(10000).Build());
            //TaskServicesManager.GetOrAdd("Cron 任务", (provider, token) => Task.Delay(1000, token), TriggerBuilder.Build(Cron.Secondly(5)));
            //TaskServicesManager.GetOrAdd("超时任务", (provider, token) => Task.Delay(2000, token), TriggerBuilder.Default.WithTimeout(1000).WithInterval(1000).WithRepeatCount(2).Build());

            //// 本机调试时此处会抛出异常，配置文件中默认开启了任务持久化到物理文件，此处异常只有首次加载时会抛出
            //// 此处异常是示例自定义任务内部未进行捕获异常时任务仍然能继续运行，不会导致整个进程崩溃退出
            //// 此处代码可注释掉
            ////TaskServicesManager.GetOrAdd("故障任务", token => throw new Exception("故障任务"));
            //TaskServicesManager.GetOrAdd("取消任务", (provider, token) => Task.Delay(1000, token)).Triggers.First().Enabled = false;

            //// 创建任务并禁用
            //TaskServicesManager.GetOrAdd("禁用任务", (provider, token) => Task.Delay(1000, token)).Status = SchedulerStatus.Disabled;
            #endregion
            try
            {
                ScheduleTaskKingDeeService ScheduleTaskKingDeeService = new ScheduleTaskKingDeeService(freeSql);
                List<DispatchTaskKingDeeView> DispatchTasks = ScheduleTaskKingDeeService.GetAllDispatchTaskKingDeeView();
                foreach (DispatchTaskKingDeeView task in DispatchTasks)
                {
                    ScheduleKingDeeUtils.AddTask(task);
                }
            }
            catch (OperationCanceledException ex)
            {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent($"任务取消:" + ex.Message,0), ex);
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message,0), ex);
            }
        }, stoppingToken);

    }
}
