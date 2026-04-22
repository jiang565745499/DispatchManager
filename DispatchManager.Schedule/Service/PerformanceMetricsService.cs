using DispatchManager.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DispatchManager.Schedule.Service
{
    /// <summary>
    /// 性能指标分析服务 - 提供系统性能监控和分析功能
    /// </summary>
    public class PerformanceMetricsService
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, PerformanceMetric> _metrics = new Dictionary<string, PerformanceMetric>();
        private static readonly List<PerformanceRecord> _performanceRecords = new List<PerformanceRecord>();
        private static readonly int MaxRecords = 1000;

        /// <summary>
        /// 记录操作性能
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>操作结果</returns>
        public static T RecordPerformance<T>(string operationName, Func<T> action)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = action();
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed, false, null);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed, true, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 异步记录操作性能
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>操作结果</returns>
        public static async Task<T> RecordPerformanceAsync<T>(string operationName, Func<Task<T>> action)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await action();
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed, false, null);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed, true, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 记录性能指标
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="elapsed">执行时间</param>
        /// <param name="isError">是否出错</param>
        /// <param name="errorMessage">错误信息</param>
        private static void RecordMetric(string operationName, TimeSpan elapsed, bool isError, string? errorMessage)
        {
            lock (_lock)
            {
                // 更新指标统计
                if (!_metrics.ContainsKey(operationName))
                {
                    _metrics[operationName] = new PerformanceMetric
                    {
                        OperationName = operationName,
                        TotalExecutions = 0,
                        TotalExecutionTime = TimeSpan.Zero,
                        ErrorCount = 0,
                        LastExecutionTime = DateTime.Now
                    };
                }

                var metric = _metrics[operationName];
                metric.TotalExecutions++;
                metric.TotalExecutionTime += elapsed;
                if (isError)
                {
                    metric.ErrorCount++;
                }
                metric.LastExecutionTime = DateTime.Now;

                // 添加性能记录
                var record = new PerformanceRecord
                {
                    OperationName = operationName,
                    ExecutionTime = elapsed,
                    IsError = isError,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.Now
                };

                _performanceRecords.Add(record);

                // 限制记录数量
                if (_performanceRecords.Count > MaxRecords)
                {
                    _performanceRecords.RemoveRange(0, _performanceRecords.Count - MaxRecords);
                }

                // 记录性能日志
                LogPerformanceMetric(metric, record);
            }
        }

        /// <summary>
        /// 获取所有性能指标
        /// </summary>
        /// <returns>性能指标列表</returns>
        public static List<PerformanceMetric> GetAllMetrics()
        {
            lock (_lock)
            {
                return _metrics.Values.ToList();
            }
        }

        /// <summary>
        /// 获取性能记录
        /// </summary>
        /// <param name="operationName">操作名称（可选）</param>
        /// <param name="startTime">开始时间（可选）</param>
        /// <param name="endTime">结束时间（可选）</param>
        /// <param name="maxRecords">最大记录数</param>
        /// <returns>性能记录列表</returns>
        public static List<PerformanceRecord> GetPerformanceRecords(string? operationName = null, DateTime? startTime = null, DateTime? endTime = null, int maxRecords = 100)
        {
            lock (_lock)
            {
                var records = _performanceRecords.AsEnumerable();

                if (!string.IsNullOrEmpty(operationName))
                {
                    records = records.Where(r => r.OperationName == operationName);
                }

                if (startTime.HasValue)
                {
                    records = records.Where(r => r.Timestamp >= startTime.Value);
                }

                if (endTime.HasValue)
                {
                    records = records.Where(r => r.Timestamp <= endTime.Value);
                }

                return records.OrderByDescending(r => r.Timestamp).Take(maxRecords).ToList();
            }
        }

        /// <summary>
        /// 重置性能指标
        /// </summary>
        public static void ResetMetrics()
        {
            lock (_lock)
            {
                _metrics.Clear();
                _performanceRecords.Clear();
            }
        }

        /// <summary>
        /// 记录性能指标日志
        /// </summary>
        /// <param name="metric">性能指标</param>
        /// <param name="record">性能记录</param>
        /// <summary>
        /// 慢查询阈值（毫秒）
        /// </summary>
        private static readonly int SlowQueryThreshold = 1000; // 1秒

        /// <summary>
        /// 记录性能指标日志
        /// </summary>
        /// <param name="metric">性能指标</param>
        /// <param name="record">性能记录</param>
        private static void LogPerformanceMetric(PerformanceMetric metric, PerformanceRecord record)
        {
            var averageExecutionTime = metric.TotalExecutions > 0
                ? metric.TotalExecutionTime.TotalMilliseconds / metric.TotalExecutions
                : 0;

            // 检测慢查询
            bool isSlowQuery = record.ExecutionTime.TotalMilliseconds > SlowQueryThreshold;

            var logContent = new Logging.Custom.LogContent(
                $"性能指标记录",
                null,
                "PerformanceMetric"
            )
            {
                AdditionalInfo = $"操作名称: {metric.OperationName}, 总执行次数: {metric.TotalExecutions}, 错误次数: {metric.ErrorCount}, 平均执行时间: {averageExecutionTime:F2}ms, 本次执行时间: {record.ExecutionTime.TotalMilliseconds:F2}ms, 状态: {(record.IsError ? "错误" : "成功" )}, 慢查询: {(isSlowQuery ? "是" : "否" )}"
            };

            if (isSlowQuery)
            {
                // 记录慢查询警告
                LogHelperUtil.WriteWarn(logContent);
            }
            else if (record.IsError && !string.IsNullOrEmpty(record.ErrorMessage))
            {
                LogHelperUtil.WriteError(logContent);
            }
            else
            {
                LogHelperUtil.WriteInfo(logContent);
            }
        }

        /// <summary>
        /// 获取慢查询记录
        /// </summary>
        /// <param name="threshold">慢查询阈值（毫秒）</param>
        /// <param name="maxRecords">最大记录数</param>
        /// <returns>慢查询记录列表</returns>
        public static List<PerformanceRecord> GetSlowQueries(int threshold = 1000, int maxRecords = 50)
        {
            lock (_lock)
            {
                return _performanceRecords
                    .Where(r => r.ExecutionTime.TotalMilliseconds > threshold)
                    .OrderByDescending(r => r.ExecutionTime.TotalMilliseconds)
                    .Take(maxRecords)
                    .ToList();
            }
        }

        /// <summary>
        /// 获取慢查询统计
        /// </summary>
        /// <param name="threshold">慢查询阈值（毫秒）</param>
        /// <returns>慢查询统计信息</returns>
        public static Dictionary<string, int> GetSlowQueryStats(int threshold = 1000)
        {
            lock (_lock)
            {
                return _performanceRecords
                    .Where(r => r.ExecutionTime.TotalMilliseconds > threshold)
                    .GroupBy(r => r.OperationName)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
        }
    }

    /// <summary>
    /// 性能指标
    /// </summary>
    public class PerformanceMetric
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// 总执行次数
        /// </summary>
        public int TotalExecutions { get; set; }

        /// <summary>
        /// 总执行时间
        /// </summary>
        public TimeSpan TotalExecutionTime { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// 最后执行时间
        /// </summary>
        public DateTime LastExecutionTime { get; set; }

        /// <summary>
        /// 平均执行时间
        /// </summary>
        public double AverageExecutionTime => TotalExecutions > 0
            ? TotalExecutionTime.TotalMilliseconds / TotalExecutions
            : 0;

        /// <summary>
        /// 错误率
        /// </summary>
        public double ErrorRate => TotalExecutions > 0
            ? (double)ErrorCount / TotalExecutions * 100
            : 0;
    }

    /// <summary>
    /// 性能记录
    /// </summary>
    public class PerformanceRecord
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// 是否出错
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}