using BootstrapBlazor.Components;
using BootstrapBlazor.DataAccess.FreeSql;
using DispatchManager.DataAccess.FreeSql.Extensions;
using DispatchManager.DataAccess.FreeSql.InterFace;
using DispatchManager.DataAccess.FreeSql.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DispatchManager.DataAccess.FreeSql.Service
{
    /// <summary>
    /// SQLite日志服务 - 提供高效的日志查询和管理
    /// </summary>
    public class SqliteLogService : ISqliteLog
    {
        private readonly IFreeSql _freeSql;
        private const int MaxRetainDays = 365;  // 最多保留365天
        private const int DefaultRetainDays = 90;

        public SqliteLogService([FromKeyedServices("LogDB")] IFreeSql sql)
        {
            _freeSql = sql ?? throw new ArgumentNullException(nameof(sql));
        }

        /// <summary>获取所有日志</summary>
        public List<Log> GetAllLog()
        {
            try
            {
                return _freeSql.Select<Log>()
                    .OrderByDescending(x => x.Timestamp)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("获取所有日志时发生错误", ex);
            }
        }

        /// <summary>获取所有日志（GetInterLog 无参版，返回全量）</summary>
        public List<Log> GetInterLog()
        {
            try
            {
                return _freeSql.Select<Log>()
                    .OrderByDescending(x => x.Timestamp)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("获取日志时发生错误", ex);
            }
        }

        /// <summary>分页获取日志（支持搜索、排序）</summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="total">总记录数（输出参数）</param>
        /// <param name="sortName">排序字段</param>
        /// <param name="sortAsc">升序标志</param>
        /// <param name="searchText">搜索关键词</param>
        /// <param name="taskNameMatchedIds">任务ID集合</param>
        /// <returns>日志列表</returns>
        public List<Log> GetInterLog(int pageIndex, int pageSize, out long total,
            string? sortName = null, bool sortAsc = false,
            string? searchText = null,
            HashSet<int>? taskNameMatchedIds = null)
        {
            try
            {
                pageIndex = pageIndex <= 0 ? 1 : pageIndex;
                pageSize = pageSize <= 0 ? 50 : pageSize;
                searchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim();

                var query = _freeSql.Select<Log>();
                var matchedTaskIds = taskNameMatchedIds?
                    .Select(i => (long)i)
                    .ToArray();

                if (!string.IsNullOrEmpty(searchText))
                {
                    if (matchedTaskIds != null && matchedTaskIds.Length > 0)
                    {
                        query = query.Where(x =>
                            (x.Level != null && x.Level.Contains(searchText)) ||
                            (x.RenderedMessage != null && x.RenderedMessage.Contains(searchText)) ||
                            (x.Exception != null && x.Exception.Contains(searchText)) ||
                            (x.Properties != null && x.Properties.Contains(searchText)) ||
                            (x.TaskID.HasValue && matchedTaskIds.Contains(x.TaskID.Value))
                        );
                    }
                    else
                    {
                        query = query.Where(x =>
                            (x.Level != null && x.Level.Contains(searchText)) ||
                            (x.RenderedMessage != null && x.RenderedMessage.Contains(searchText)) ||
                            (x.Exception != null && x.Exception.Contains(searchText)) ||
                            (x.Properties != null && x.Properties.Contains(searchText))
                        );
                    }
                }
                else if (matchedTaskIds != null && matchedTaskIds.Length > 0)
                {
                    query = query.Where(x => x.TaskID.HasValue && matchedTaskIds.Contains(x.TaskID.Value));
                }

                total = query.Count();

                var mappedSortName = sortName?.Trim();
                if (string.Equals(mappedSortName, "Message", StringComparison.OrdinalIgnoreCase))
                    mappedSortName = "RenderedMessage";
                if (string.Equals(mappedSortName, "Date", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(mappedSortName, "Timestamp", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(mappedSortName, "TimeStamp", StringComparison.OrdinalIgnoreCase))
                    mappedSortName = "Timestamp";

                if (!string.IsNullOrEmpty(mappedSortName))
                {
                    query = sortAsc
                        ? query.OrderByPropertyName(mappedSortName)
                        : query.OrderByPropertyName($"{mappedSortName} desc");
                }
                else
                {
                    query = query.OrderByDescending(x => x.Timestamp);
                }

                return query.Page(pageIndex, pageSize).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("分页查询日志时发生错误", ex);
            }
        }

        /// <summary>根据查询条件获取日志（通用条件查询）</summary>
        public QueryData<Log> GetInterLogByCon(QueryPageOptions option)
        {
            try
            {
                var query = _freeSql.Select<Log>();

                // 应用动态过滤
                query = query.WhereDynamicFilter(option.ToDynamicFilter());

                // 计算总数
                var total = query.Count();

                // 应用排序
                if (option.SortOrder != SortOrder.Unset)
                {
                    query = query.OrderByPropertyName(option.SortName, option.SortOrder == SortOrder.Asc);
                }
                else
                {
                    query = query.OrderByDescending(x => x.Timestamp);
                }

                // 应用分页
                var items = new List<Log>();
                if (option.IsPage)
                {
                    items = query
                        .Skip((option.PageIndex - 1) * option.PageItems)
                        .Take(option.PageItems)
                        .ToList();
                }
                else
                {
                    items = query.ToList();
                }

                return new QueryData<Log>
                {
                    IsSorted = option.SortOrder != SortOrder.Unset,
                    IsFiltered = option.Filters.Any(),
                    IsAdvanceSearch = option.AdvanceSearches.Any(),
                    IsSearch = option.Searches.Any() || option.CustomerSearches.Any(),
                    Items = items,
                    TotalCount = Convert.ToInt32(total)
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("条件查询日志时发生错误", ex);
            }
        }

        /// <summary>
        /// 清理指定天数之前的历史日志，控制数据库体积
        /// </summary>
        /// <param name="retainDays">保留最近多少天，默认 90 天，范围 1-365 天</param>
        /// <returns>删除的行数</returns>
        public int CleanupOldLogs(int retainDays = DefaultRetainDays)
        {
            try
            {
                retainDays = Math.Clamp(retainDays, 1, MaxRetainDays);
                var cutoff = DateTime.Now.AddDays(-retainDays).ToString("yyyy-MM-dd HH:mm:ss");

                var deletedCount = _freeSql.Delete<Log>()
                    .Where(x => x.Timestamp!.CompareTo(cutoff) < 0)
                    .ExecuteAffrows();

                return deletedCount;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"清理{retainDays}天前的日志时发生错误", ex);
            }
        }

        /// <summary>获取指定级别的日志</summary>
        public List<Log> GetLogsByLevel(string level)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(level))
                    return new List<Log>();

                return _freeSql.Select<Log>()
                    .Where(x => x.Level == level)
                    .OrderByDescending(x => x.Timestamp)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"获取级别为 {level} 的日志时发生错误", ex);
            }
        }

        /// <summary>获取最近N天的日志统计</summary>
        public Dictionary<string, int> GetLogStatistics(int days = 7)
        {
            try
            {
                days = Math.Max(1, Math.Min(days, 365));
                var startDate = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd HH:mm:ss");

                var stats = _freeSql.Select<Log>()
                    .Where(x => x.Timestamp!.CompareTo(startDate) >= 0)
                    .GroupBy(x => x.Level!)
                    .Select(x => new { Level = x.Key, Count = x.Count() })
                    .ToList()
                    .ToDictionary(x => x.Level ?? "Unknown", x => x.Count);

                return stats;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("获取日志统计时发生错误", ex);
            }
        }

        /// <summary>获取日志总数</summary>
        public long GetLogCount()
        {
            try
            {
                return _freeSql.Select<Log>().Count();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("获取日志总数时发生错误", ex);
            }
        }

        /// <summary>
        /// 写入调度任务日志，直接持久化 TaskId
        /// </summary>
        /// <param name="task">调度任务</param>
        /// <param name="message">日志消息</param>
        /// <param name="level">日志级别，默认为 Information</param>
        /// <param name="exception">异常信息（可选）</param>
        /// <param name="category">类别（可选）</param>
        public void WriteDispatchTaskLog(DispatchTask task, string message, string level = "Information", Exception? exception = null, string? category = null)
        {
            if (task?.IsLog != true || string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                var properties = new Dictionary<string, object?>();
                if (!string.IsNullOrWhiteSpace(task.Name)) properties["TaskName"] = task.Name;
                if (!string.IsNullOrWhiteSpace(category)) properties["Category"] = category;

                _freeSql.Insert(new Log
                {
                    Timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"),
                    Level = level,
                    Exception = exception?.ToString(),
                    RenderedMessage = message,
                    Properties = properties.Count > 0 ? JsonSerializer.Serialize(properties) : null,
                    TaskID = task.ID
                }).ExecuteAffrows();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入 DispatchTask 日志失败: {ex}");
            }
        }

        /// <summary>
        /// 写入调度任务错误日志
        /// </summary>
        /// <param name="task">调度任务</param>
        /// <param name="message">日志消息</param>
        /// <param name="ex">异常信息</param>
        /// <param name="category">类别（可选）</param>
        public void WriteDispatchTaskErrorLog(DispatchTask task, string message, Exception ex, string? category = null)
        {
            WriteDispatchTaskLog(task, message, "Error", ex, category);
        }
    }
}
