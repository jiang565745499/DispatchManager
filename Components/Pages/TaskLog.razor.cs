using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Service;
using DispatchManager.Logging;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DispatchManager.Components.Pages
{
    public partial class TaskLog
    {
        private static IEnumerable<int> PageItemsSource => new int[] { 50, 100, 200, 500 };

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        private List<Log> SelectedRows { get; set; } = [];
        private bool _lastQueryIsSearch;
        private int _lastQueryTotalCount;
        private string? _lastSearchKeyword;

        // ─── 导出 ───────────────────────────────────────────────

        private async Task ExportLogsToExcel() => await ExportLogs("日志导出", "text/csv", ".csv");
        private async Task ExportLogsToCsv() => await ExportLogs("日志导出", "text/csv", ".csv");

        private async Task ExportLogs(string baseName, string contentType, string ext)
        {
            try
            {
                var allLogs = LogDb.GetAllLog();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("时间戳,级别,日志消息,异常信息");
                foreach (var log in allLogs)
                {
                    sb.AppendLine(string.Join(",",
                        Csv(log.Date?.ToString("yyyy-MM-dd HH:mm:ss.fff")),
                        Csv(log.Level),
                        Csv(log.Message),
                        Csv(log.Exception)));
                }
                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                var fileName = $"{baseName}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                await JSRuntime.InvokeVoidAsync("downloadFile", fileName, contentType, Convert.ToBase64String(bytes));
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0), ex);
            }
        }

        private static string Csv(string? v)
        {
            if (v == null) return string.Empty;
            if (v.Contains(',') || v.Contains('\n') || v.Contains('"'))
                return $"\"{v.Replace("\"", "\"\"")}\"";
            return v;
        }

        // ─── 工具方法 ───────────────────────────────────────────

        private static Color GetLevelColor(string? level) => level?.ToUpperInvariant() switch
        {
            "ERROR" or "FATAL" => Color.Danger,
            "WARN"             => Color.Warning,
            "INFO" or "INFORMATION" => Color.Success,
            "DEBUG"   or "DBG"         => Color.Secondary,
            _                  => Color.None
        };

        // ─── 数据查询 ────────────────────────────────────────────

        private Task<QueryData<Log>> OnQueryAsync(QueryPageOptions options)
        {
            try
            {
                var searchText = options.SearchText?.Trim();
                var normalizedSearchText = string.IsNullOrWhiteSpace(searchText) ? string.Empty : searchText;

                string? sortName = options.SortOrder != SortOrder.Unset ? options.SortName : null;
                bool sortAsc = options.SortOrder == SortOrder.Asc;

                HashSet<int>? taskNameMatchedIds = null;
                if (!string.IsNullOrWhiteSpace(normalizedSearchText))
                {
                    taskNameMatchedIds = MainDb.GetAllDispatchTask()
                        .Where(t => !string.IsNullOrWhiteSpace(t.Name) && t.Name.Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase))
                        .Where(t => t.ID.HasValue && t.ID.Value <= int.MaxValue)
                        .Select(t => (int)t.ID!.Value)
                        .ToHashSet();

                    if (taskNameMatchedIds.Count == 0)
                        taskNameMatchedIds = null;
                }

                var ts = LogDb.GetInterLog(
                    options.PageIndex,
                    options.PageItems,
                    out long count,
                    sortName,
                    sortAsc,
                    normalizedSearchText,
                    taskNameMatchedIds);

                // 无搜索词且服务返回空时，回退到全量分页，避免被错误搜索条件过滤
                if (count == 0 && string.IsNullOrEmpty(normalizedSearchText))
                {
                    var allLogs = LogDb.GetAllLog();
                    count = allLogs.Count;
                    var safePageIndex = options.PageIndex <= 0 ? 1 : options.PageIndex;
                    ts = allLogs
                        .Skip((safePageIndex - 1) * options.PageItems)
                        .Take(options.PageItems)
                        .ToList();
                }

                // 防御：如果 count>0 但当前页无数据（常见于服务层筛选/分页异常），统一回退到全量分页
                if (count > 0 && (ts == null || ts.Count == 0))
                {
                    var allLogs = LogDb.GetAllLog();
                    count = allLogs.Count;
                    var safePageIndex = options.PageIndex <= 0 ? 1 : options.PageIndex;
                    ts = allLogs
                        .Skip((safePageIndex - 1) * options.PageItems)
                        .Take(options.PageItems)
                        .ToList();
                }

                // 保持已选中行
                var selected = ts.FirstOrDefault(i => i.ID == SelectedRows.FirstOrDefault()?.ID);
                SelectedRows.Clear();
                if (selected != null) SelectedRows.Add(selected);

                _lastSearchKeyword = normalizedSearchText;
                _lastQueryIsSearch = !string.IsNullOrWhiteSpace(normalizedSearchText);
                _lastQueryTotalCount = (int)count;

                return Task.FromResult(new QueryData<Log>
                {
                    Items      = ts,
                    TotalCount = (int)count,
                    IsSorted   = options.SortOrder != SortOrder.Unset,
                    IsSearch   = _lastQueryIsSearch,
                    IsFiltered = _lastQueryIsSearch
                });
            }
            catch (Exception ex)
            {
                LogHelperUtil.WriteError(new Logging.Custom.LogContent(ex.Message, 0), ex);
                _lastSearchKeyword = null;
                _lastQueryIsSearch = false;
                _lastQueryTotalCount = 0;
                return Task.FromResult(new QueryData<Log>());
            }
        }

        private readonly Dictionary<long, string> _taskNameCache = [];

        private string GetTaskName(long? taskId)
        {
            if (!taskId.HasValue)
                return "系统日志";

            if (_taskNameCache.TryGetValue(taskId.Value, out var taskName))
                return taskName;

            taskName = MainDb.GetDispatchTaskByID(taskId.Value)?.Name ?? $"任务ID:{taskId.Value}";
            _taskNameCache[taskId.Value] = taskName;
            return taskName;
        }
    }
}
