using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Service;
using DispatchManager.Logging;
using DispatchManager.Utils;
using Longbow.Tasks;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using static FreeSql.Internal.GlobalFilter;

namespace DispatchManager.Components.Pages
{
    public partial class TaskClass
    {
        //[Inject] 
        //IFreeSql? freeSql { get; set; }
        
        [Inject]
        [NotNull] 
        ToastService? toastService { get; set; }

        private static IEnumerable<int> PageItemsSource => new int[] { 10, 30, 50 };

        private List<DispatchClass> SelectedRows { get; set; } = [];

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

        private Task<QueryData<DispatchClass>> OnQueryAsync(QueryPageOptions options)
        {
            try
            {
                List<DispatchClass> ts = MainDb.GetAllDispatchClass();
                
                // 前端模糊匹配搜索
                if (!string.IsNullOrEmpty(options.SearchText))
                {
                    var searchText = options.SearchText.ToLower();
                    ts = ts.Where(task =>
                        task.ClassName?.ToLower().Contains(searchText) == true
                    ).ToList();
                }
                
                if (options.SortList != null && options.SortList.Any())
                {
                    ts = ts.Sort(options.SortList).ToList();
                }
                var model = ts.FirstOrDefault(i => i.ClassName == SelectedRows.FirstOrDefault()?.ClassName);
                SelectedRows.Clear();
                if (model != null)
                {
                    SelectedRows.Add(model);
                }
                var count = ts.Count;
                ts = ts.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();
                return Task.FromResult(new QueryData<DispatchClass>()
                {
                    Items = ts,
                    TotalCount = count
                });
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0), ex);
                return Task.FromResult(new QueryData<DispatchClass>() { });
            }
        }

        private Task<bool> OnSaveAsync(DispatchClass model, ItemChangedType changedType)
        {
            try
            {
                if (changedType == ItemChangedType.Add)
                {
                    if (MainDb.GetColorByName(model.ClassName) != null)
                    {
                        toastService.Show(new ToastOption()
                        {
                            PreventDuplicates = true,
                            Category = ToastCategory.Error,
                            Title = "数据校验",
                            Content = "任务系统已存在,不允许重复添加!",
                        });
                        return Task.FromResult(false);
                    }
                    long id = MainDb.ReturnID(model);
                    model.ID = id;
                }
                if (changedType == ItemChangedType.Update)
                {
                    var YObject = MainDb.GetDispatchClassByID(model.ID);
                    if (YObject != null && !string.IsNullOrEmpty(YObject.ClassName))
                    {
                        TaskServicesManager.Remove(YObject.ClassName);
                    }
                    // 更新数据
                    MainDb.UpdateSingle(model);
                }
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0),ex);
                return Task.FromResult(false);
            }
        }

        private Task<bool> OnDeleteAsync(IEnumerable<DispatchClass> models)
        {
            try
            {
                // 循环删除任务
                foreach (var model in models)
                {
                    MainDb.DeleteSingle(model);
                }
                return Task.FromResult(true);
            }
            catch (Exception ex) {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0),ex);
                return Task.FromResult(true);
            }
        }

        private bool OnShowButtonCallback(DispatchClass model) => !IsDemo && !Jobs.Any(i => i == model.ClassName);

        private Color GetClassNameColor(DispatchClass result) {
            return (Color)MainDb.GetColorByName(result.ClassName)!.FColor!;
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


        private static string FormatClassName(DispatchClass dispatchClass){
            return dispatchClass.ClassName;
        }

        private static string GetStatusIcon(SchedulerStatus status) => status switch
        {
            SchedulerStatus.Running => "fa-solid fa-play-circle",
            SchedulerStatus.Ready => "fa-solid fa-stop-circle",
            SchedulerStatus.Disabled => "fa-solid fa-times-circle",
            _ => "未知状态"
        };
    }
}
