using BootstrapBlazor.Components;
using DispatchManager.Components.Shared.Tasks;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Models.View;
using DispatchManager.DataAccess.FreeSql.Service;
using DispatchManager.Logging;
using DispatchManager.Schedule.Extensions;
using DispatchManager.Schedule.Utils;
using DispatchManager.Utils;
using Longbow.Tasks;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static FreeSql.Internal.GlobalFilter;

namespace DispatchManager.Components.Pages
{
    public partial class TasksKingDee
    {
        [Inject]
        [NotNull] 
        ToastService? toastService { get; set; }

        private static IEnumerable<int> PageItemsSource => new int[] { 50, 100, 999 };

        private List<DispatchTaskKingDee> SelectedRows { get; set; } = [];

        private static IEnumerable<string> Jobs =>
        [
            "单次任务",
            "周期任务",
            "Cron 任务",
            "超时任务",
            "取消任务",
            "禁用任务",
            "SQL日志",
            "健康检查"
        ];

        [Inject]
        [NotNull]
        private DialogService? DialogService { get; set; }

        private bool IsDemo { get; set; }

        /// <summary>
        ///
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            IsDemo = false;

        }

        private Task<QueryData<DispatchTaskKingDee>> OnQueryAsync(QueryPageOptions options)
        {
            try
            {
                var tasks = TaskServicesManager.ToList().ToTasksModelListKingDee();
                tasks.ForEach(task =>
                {
                    var ts = MainDb.GetDispatchTaskByName(task.Name);

                    if (ts != null)
                    {
                        task.ID = ts.ID;
                        task.ApiUrl = ts.ApiUrl;
                        task.ReturnApiUrl = ts.ReturnApiUrl;
                        task.ClassID = ts.ClassID;
                        task.FNo = ts.FNo;
                        task.IsLog = ts.IsLog;
                        task.KingDeeFormId = ts.KingDeeFormId;
                        task.KingDeeFields = ts.KingDeeFields;
                        task.KingDeeFilterString = ts.KingDeeFilterString;
                    }
                    //task.ID = ts.Where(x => x.Name == task.Name).First()?.ID;
                    //task.ApiUrl = ts.Where(x => x.Name == task.Name).First()?.ApiUrl;
                    //task.ReturnApiUrl = ts.Where(x => x.Name == task.Name).First()?.ReturnApiUrl;
                    //task.ClassID = ts.Where(x => x.Name == task.Name).First()?.ClassID;
                    //task.FNo = ts.Where(x => x.Name == task.Name).First()?.FNo;
                    //task.IsLog = ts.Where(x => x.Name == task.Name).First().IsLog;
                    //task.KingDeeFormId = ts.Where(x => x.Name == task.Name).First().KingDeeFormId;
                    //task.KingDeeFields = ts.Where(x => x.Name == task.Name).First().KingDeeFields;
                    //task.KingDeeFilterString = ts.Where(x => x.Name == task.Name).First().KingDeeFilterString;
                });
                tasks = tasks.Where(x => x.ID != null).ToList();
                
                // 前端模糊匹配搜索
                if (!string.IsNullOrEmpty(options.SearchText))
                {
                    var searchText = options.SearchText.ToLower();
                    tasks = tasks.Where(task =>
                        task.FNo.ToString().Contains(searchText) ||
                        task.Name?.ToLower().Contains(searchText) == true ||
                        task.Trigger?.ToLower().Contains(searchText) == true ||
                        task.ApiUrl?.ToLower().Contains(searchText) == true ||
                        FormatClassName(task.ClassID)?.ToLower().Contains(searchText) == true ||
                        FormatResult(task.LastRunResult)?.ToLower().Contains(searchText) == true ||
                        FormatStatus(task.Status)?.ToLower().Contains(searchText) == true
                    ).ToList();
                }
                
                if (options.SortList != null && options.SortList.Any())
                {
                    tasks = tasks.Sort(options.SortList).ToList();
                }
                var model = tasks.FirstOrDefault(i => i.Name == SelectedRows.FirstOrDefault()?.Name);
                SelectedRows.Clear();
                if (model != null)
                {
                    SelectedRows.Add(model);
                }
                var count = tasks.Count;
                tasks = tasks.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();
                return Task.FromResult(new QueryData<DispatchTaskKingDee>()
                {
                    Items = tasks,
                    TotalCount = count
                });
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0), ex);
                toastService.Show(new ToastOption()
                {
                    PreventDuplicates = true,
                    Category = ToastCategory.Error,
                    Title = "查询异常",
                    Content = "数据加载失败，请稍后重试",
                });
                return Task.FromResult(new QueryData<DispatchTaskKingDee>() { });
            }
        }

        private Task<bool> OnSaveAsync(DispatchTaskKingDee model, ItemChangedType changedType)
        {
            try
            {
                if (!ScheduleUtils.RequiredCronStr(model.Trigger))
                {
                    toastService.Show(new ToastOption()
                    {
                        PreventDuplicates = true,
                        Category = ToastCategory.Error,
                        Title = "数据校验",
                        Content = "Cron表达式解析错误!请检查",
                    });
                    return Task.FromResult(false);
                }
                if (changedType == ItemChangedType.Add)
                {
                    if (MainDb.GetDispatchTaskByName(model.Name) != null ||
                        MainDb.GetDispatchTaskByName2(model.Name) != null)
                    {
                        toastService.Show(new ToastOption()
                        {
                            PreventDuplicates = true,
                            Category = ToastCategory.Error,
                            Title = "数据校验",
                            Content = "任务名已存在,不允许重复添加!",
                        });
                        return Task.FromResult(false);
                    }
                    long id = MainDb.ReturnID(model);
                    model.ID = id;
                    model.Status = SchedulerStatus.Ready;
                    TaskServicesManager.Remove(model.Name);
                }
                if (changedType == ItemChangedType.Update)
                {
                    var YObject = MainDb.GetDispatchTaskByID(model.ID);
                    if(YObject!=null && !string.IsNullOrEmpty(YObject.Name))
                    {
                        TaskServicesManager.Remove(YObject.Name);
                    }
                    MainDb.UpdateSingleAll(model);
                }
                DispatchTaskKingDeeView dispatchTaskKingDeeView = new DispatchTaskKingDeeView(model);
                var TaskClass = MainDb2.GetDispatchClassByID(model.ClassID);
                dispatchTaskKingDeeView.Y9Key = TaskClass.Y9Key;
                dispatchTaskKingDeeView.KingDeeAccountID = TaskClass.KingDeeAccountID;
                dispatchTaskKingDeeView.KingDeeAppID = TaskClass.KingDeeAppID;
                dispatchTaskKingDeeView.KingDeeAppSec = TaskClass.KingDeeAppSec;
                dispatchTaskKingDeeView.KingDeeLCID = TaskClass.KingDeeLCID;
                dispatchTaskKingDeeView.KingDeeServerUrl = TaskClass.KingDeeServerUrl;
                dispatchTaskKingDeeView.KingDeeUserName = TaskClass.KingDeeUserName;
                ScheduleKingDeeUtils.AddTask(dispatchTaskKingDeeView);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0),ex);
                return Task.FromResult(false);
            }
        }

        private Task<bool> OnDeleteAsync(IEnumerable<DispatchTaskKingDee> models)
        {
            try
            {
                // 演示模式下禁止删除内置任务
                if (IsDemo)
                {
                    var m = models.ToList();
                    m.RemoveAll(m => Jobs.Any(i => i == m.Name));
                    models = m;
                }

                // 循环删除任务
                foreach (var model in models)
                {
                    TaskServicesManager.Remove(model.Name);
                    MainDb.DeleteSingle(model);
                }
                return Task.FromResult(true);
            }
            catch (Exception ex) {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0),ex);
                return Task.FromResult(false);
            }
        }

        private bool OnShowButtonCallback(DispatchTaskKingDee model) => !IsDemo && !Jobs.Any(i => i == model.Name);

        private Color GetClassNameColor(int? result) {
            if(MainDb2.GetDispatchClassByID(result) == null)
            {
                return Color.None;
            }
            return (Color)MainDb2.GetDispatchClassByID(result)!.FColor;
        }

        private static Color GetResultColor(TriggerResult result) => result switch
        {
            TriggerResult.Success => Color.Success,
            TriggerResult.Error => Color.Danger,
            TriggerResult.Timeout => Color.Warning,
            TriggerResult.Cancelled => Color.Dark,
            _ => Color.Primary
        };

        private static string FormatResult(TriggerResult result) => result switch
        {
            TriggerResult.Success => "成功",
            TriggerResult.Error => "故障",
            TriggerResult.Timeout => "超时",
            TriggerResult.Cancelled => "取消",
            _ => "未知状态"
        };

        private static Color GetStatusColor(SchedulerStatus status) => status switch
        {
            SchedulerStatus.Running => Color.Success,
            SchedulerStatus.Ready => Color.Danger,
            SchedulerStatus.Disabled => Color.Danger,
            _ => Color.Primary
        };

        private static string FormatStatus(SchedulerStatus status) => status switch
        {
            SchedulerStatus.Running => "运行中",
            SchedulerStatus.Ready => "已停止",
            SchedulerStatus.Disabled => "禁用",
            _ => "未知状态"
        };


        private string FormatClassName(int? classID){
            if (MainDb2.GetDispatchClassByID(classID) == null)
            {
                return Color.None.ToString();
            }
            return MainDb2.GetDispatchClassByID(classID)!.ClassName;
        }

        private static string GetStatusIcon(SchedulerStatus status) => status switch
        {
            SchedulerStatus.Running => "fa-solid fa-play-circle",
            SchedulerStatus.Ready => "fa-solid fa-stop-circle",
            SchedulerStatus.Disabled => "fa-solid fa-times-circle",
            _ => "未知状态"
        };

        private Task OnPause(DispatchTaskKingDee model)
        {
            var task = TaskServicesManager.ToList().FirstOrDefault(i => i.Name == model.Name);
            if (task != null)
            {
                task.Status = SchedulerStatus.Ready;
            }
            SelectedRows.Clear();
            SelectedRows.Add(model);
            return Task.CompletedTask;
        }

        private Task OnDisable(DispatchTaskKingDee model)
        {
            var task = TaskServicesManager.ToList().FirstOrDefault(i => i.Name == model.Name);
            if (task != null)
            {
                task.Status = SchedulerStatus.Disabled;
            }
            MainDb.UpdateSingleDisable(model);
            SelectedRows.Clear();
            SelectedRows.Add(model);
            return Task.CompletedTask;
        }

        private Task OnRun(DispatchTaskKingDee model)
        {
            var task = TaskServicesManager.ToList().FirstOrDefault(i => i.Name == model.Name);
            if (task != null)
            {
                task.Status = SchedulerStatus.Running;
            }
            MainDb.UpdateSingleRun(model);
            SelectedRows.Clear();
            SelectedRows.Add(model);
            return Task.CompletedTask;
        }

        private async Task OnLog(DispatchTaskKingDee model)
        {
            var option = new DialogOption()
            {
                Class = "modal-dialog-task",
                Title = $"{model.Name} - 日志窗口(最新 20 条)",
                Component = BootstrapDynamicComponent.CreateComponent<TaskInfo>(new Dictionary<string, object?>
                {
                    [nameof(TaskInfo.Model)] = model
                })
            };
            await DialogService.Show(option);
        }

        private static bool OnCheckTaskStatus(DispatchTaskKingDee model) => model.Status != SchedulerStatus.Disabled;

        private static bool OnCheckTaskStatus2(DispatchTaskKingDee model) => true;

        private static string GetStatusDotClass(SchedulerStatus status) => status switch
        {
            SchedulerStatus.Running => "dtc-dot-running",
            SchedulerStatus.Ready => "dtc-dot-ready",
            SchedulerStatus.Disabled => "dtc-dot-disabled",
            _ => "dtc-dot-info"
        };

        private static string GetResultDotClass(TriggerResult result) => result switch
        {
            TriggerResult.Success => "dtc-dot-success",
            TriggerResult.Error => "dtc-dot-danger",
            TriggerResult.Timeout => "dtc-dot-warning",
            TriggerResult.Cancelled => "dtc-dot-disabled",
            _ => "dtc-dot-info"
        };

        private readonly HashSet<string> _expandedApiUrls = [];

        private bool IsApiUrlExpanded(string? apiUrl)
        {
            return !string.IsNullOrWhiteSpace(apiUrl) && _expandedApiUrls.Contains(apiUrl);
        }

        private void ToggleApiUrlExpand(string? apiUrl)
        {
            if (string.IsNullOrWhiteSpace(apiUrl))
                return;

            if (!_expandedApiUrls.Add(apiUrl))
            {
                _expandedApiUrls.Remove(apiUrl);
            }
        }


        class DefaultTaskExecutor : ITask
        {
            /// <summary>
            /// 任务执行方法
            /// </summary>
            /// <param name="provider"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public Task Execute(IServiceProvider provider, CancellationToken cancellationToken) => Task.Delay(1000, cancellationToken);
        }
    }
}
