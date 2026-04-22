using Microsoft.AspNetCore.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Service;
using DispatchManager.Logging;
using Microsoft.JSInterop;
using BootstrapBlazor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DispatchManager.Components.Pages
{
    public partial class LogDashboard
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        private string _timeRange = "24h";
        private List<Log> AllLogs { get; set; } = new List<Log>();
        private List<Log> SelectedRows { get; set; } = new List<Log>();
        private string? _lastSearchKeyword;
        private bool _lastQueryIsSearch;
        private int _lastQueryTotalCount;

        /// <summary>
        /// 总日志数
        /// </summary>
        private int TotalLogs => AllLogs.Count;

        /// <summary>
        /// 错误日志数
        /// </summary>
        private int ErrorLogs => AllLogs.Count(l => l.Level?.ToLower() == "error");

        /// <summary>
        /// 警告日志数
        /// </summary>
        private int WarnLogs => AllLogs.Count(l => l.Level?.ToLower() == "warn");

        /// <summary>
        /// 信息日志数
        /// </summary>
        private int InfoLogs => AllLogs.Count(l => l.Level?.ToLower() == "info");

        /// <summary>
        /// 最近错误日志
        /// </summary>
        private List<Log> RecentErrorLogs => AllLogs
            .Where(l => l.Level?.ToLower() == "error")
            .OrderByDescending(l => l.Date)
            .Take(10)
            .ToList();

        /// <summary>
        /// 组件初始化
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await RefreshDashboard();
        }

        /// <summary>
        /// 刷新仪表板数据
        /// </summary>
        private async Task RefreshDashboard()
        {
            // 获取所有日志
            AllLogs = LogDb.GetAllLog();

            // 生成图表数据
            var logTrendData = GenerateLogTrendData();
            var logLevelData = GenerateLogLevelData();
            var taskLogData = GenerateTaskLogData();

            // 初始化图表
            await JSRuntime.InvokeVoidAsync("initLogDashboard", logTrendData, logLevelData, taskLogData);

            StateHasChanged();
        }

        /// <summary>
        /// 生成日志时间趋势数据
        /// </summary>
        private object GenerateLogTrendData()
        {
            var now = DateTime.Now;
            DateTime startDate;
            int intervalHours;
            int pointCount;

            switch (_timeRange)
            {
                case "7d":
                    startDate = now.AddDays(-7);
                    intervalHours = 6;
                    pointCount = 28;
                    break;
                case "30d":
                    startDate = now.AddDays(-30);
                    intervalHours = 24;
                    pointCount = 30;
                    break;
                default: // 24h
                    startDate = now.AddHours(-24);
                    intervalHours = 1;
                    pointCount = 24;
                    break;
            }

            var labels = new List<string>();
            var errorData = new List<int>();
            var warnData = new List<int>();
            var infoData = new List<int>();

            for (int i = 0; i < pointCount; i++)
            {
                var currentStart = startDate.AddHours(i * intervalHours);
                var currentEnd = currentStart.AddHours(intervalHours);

                labels.Add(currentStart.ToString(intervalHours == 1 ? "HH:00" : "MM-dd"));

                var periodLogs = AllLogs.Where(l => l.Date >= currentStart && l.Date < currentEnd);
                errorData.Add(periodLogs.Count(l => l.Level?.ToLower() == "error"));
                warnData.Add(periodLogs.Count(l => l.Level?.ToLower() == "warn"));
                infoData.Add(periodLogs.Count(l => l.Level?.ToLower() == "info"));
            }

            return new {
                labels,
                errorData,
                warnData,
                infoData
            };
        }

        /// <summary>
        /// 生成日志级别分布数据
        /// </summary>
        private object GenerateLogLevelData()
        {
            var levelCounts = AllLogs
                .GroupBy(l => l.Level?.ToUpper() ?? "UNKNOWN")
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            var labels = levelCounts.Select(l => l.Level).ToList();
            var data = levelCounts.Select(l => l.Count).ToList();

            return new {
                labels,
                data
            };
        }

        /// <summary>
        /// 生成任务日志分布数据
        /// </summary>
        private object GenerateTaskLogData()
        {
            var taskLogs = AllLogs
                .Where(l => l.TaskID.HasValue && l.TaskID > 0)
                .GroupBy(l => l.TaskID)
                .Select(g => new { TaskID = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToList();

            var labels = taskLogs.Select(t => t.TaskID.ToString()).ToList();
            var data = taskLogs.Select(t => t.Count).ToList();

            return new {
                labels,
                data
            };
        }

        private Task<QueryData<Log>> OnQueryAsync(QueryPageOptions options)
        {
            try
            {
                var searchText = options.SearchText?.Trim();
                var normalizedSearchText = string.IsNullOrWhiteSpace(searchText) ? string.Empty : searchText;

                string? sortName = options.SortOrder != SortOrder.Unset ? options.SortName : null;
                bool sortAsc = options.SortOrder == SortOrder.Asc;

                var ts = LogDb.GetInterLog(
                    options.PageIndex,
                    options.PageItems,
                    out long count,
                    sortName,
                    sortAsc,
                    normalizedSearchText,
                    (HashSet<int>?)null);

                if (count == 0 && string.IsNullOrEmpty(normalizedSearchText))
                {
                    var allLogs = LogDb.GetAllLog();
                    count = allLogs.Count;
                    var safePageIndex = options.PageIndex <= 0 ? 1 : options.PageIndex;
                    ts = allLogs
                        .OrderByDescending(x => x.Date)
                        .Skip((safePageIndex - 1) * options.PageItems)
                        .Take(options.PageItems)
                        .ToList();
                }

                if (count > 0 && (ts == null || ts.Count == 0))
                {
                    var allLogs = LogDb.GetAllLog();
                    count = allLogs.Count;
                    var safePageIndex = options.PageIndex <= 0 ? 1 : options.PageIndex;
                    ts = allLogs
                        .OrderByDescending(x => x.Date)
                        .Skip((safePageIndex - 1) * options.PageItems)
                        .Take(options.PageItems)
                        .ToList();
                }

                var selected = ts.FirstOrDefault(i => i.ID == SelectedRows.FirstOrDefault()?.ID);
                SelectedRows.Clear();
                if (selected != null) SelectedRows.Add(selected);

                _lastSearchKeyword = normalizedSearchText;
                _lastQueryIsSearch = !string.IsNullOrWhiteSpace(normalizedSearchText);
                _lastQueryTotalCount = (int)count;

                return Task.FromResult(new QueryData<Log>
                {
                    Items = ts,
                    TotalCount = (int)count,
                    IsSorted = options.SortOrder != SortOrder.Unset,
                    IsSearch = _lastQueryIsSearch,
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
    }
}