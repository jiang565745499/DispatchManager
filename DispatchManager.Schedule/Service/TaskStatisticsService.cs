using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Schedule.Service
{
    /// <summary>
    /// 任务执行统计服务
    /// </summary>
    public class TaskStatisticsService
    {
        private static readonly Dictionary<string, TaskStatistics> _statistics = new Dictionary<string, TaskStatistics>();
        private static readonly object _lock = new object();

        /// <summary>
        /// 记录任务开始执行
        /// </summary>
        /// <param name="taskName"></param>
        public static void RecordTaskStart(string taskName)
        {
            lock (_lock)
            {
                if (!_statistics.ContainsKey(taskName))
                {
                    _statistics[taskName] = new TaskStatistics { TaskName = taskName };
                }

                _statistics[taskName].TotalExecutions++;
                _statistics[taskName].LastExecutionStart = DateTime.Now;
            }
        }

        /// <summary>
        /// 记录任务执行成功
        /// </summary>
        /// <param name="taskName"></param>
        public static void RecordTaskSuccess(string taskName)
        {
            lock (_lock)
            {
                if (_statistics.ContainsKey(taskName))
                {
                    var stats = _statistics[taskName];
                    stats.SuccessExecutions++;
                    stats.LastExecutionEnd = DateTime.Now;
                    stats.LastSuccessExecution = DateTime.Now;
                    stats.AverageExecutionTime = CalculateAverageExecutionTime(stats);

                    LogTaskSuccess(stats);
                }
            }
        }

        /// <summary>
        /// 记录任务执行失败
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="errorMessage"></param>
        public static void RecordTaskFailure(string taskName, string errorMessage)
        {
            lock (_lock)
            {
                if (_statistics.ContainsKey(taskName))
                {
                    var stats = _statistics[taskName];
                    stats.FailureExecutions++;
                    stats.LastExecutionEnd = DateTime.Now;
                    stats.LastFailureExecution = DateTime.Now;
                    stats.LastErrorMessage = errorMessage;
                    stats.AverageExecutionTime = CalculateAverageExecutionTime(stats);

                    LogTaskFailure(stats);
                }
            }
        }

        /// <summary>
        /// 获取任务统计信息
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public static TaskStatistics? GetTaskStatistics(string taskName)
        {
            lock (_lock)
            {
                return _statistics.ContainsKey(taskName) ? _statistics[taskName] : null;
            }
        }

        /// <summary>
        /// 获取所有任务统计信息
        /// </summary>
        /// <returns></returns>
        public static List<TaskStatistics> GetAllTaskStatistics()
        {
            lock (_lock)
            {
                return _statistics.Values.ToList();
            }
        }

        /// <summary>
        /// 重置任务统计信息
        /// </summary>
        /// <param name="taskName"></param>
        public static void ResetTaskStatistics(string taskName)
        {
            lock (_lock)
            {
                if (_statistics.ContainsKey(taskName))
                {
                    _statistics[taskName] = new TaskStatistics { TaskName = taskName };
                }
            }
        }

        /// <summary>
        /// 重置所有任务统计信息
        /// </summary>
        public static void ResetAllTaskStatistics()
        {
            lock (_lock)
            {
                _statistics.Clear();
            }
        }

        /// <summary>
        /// 计算平均执行时间
        /// </summary>
        /// <param name="stats"></param>
        /// <returns></returns>
        private static TimeSpan CalculateAverageExecutionTime(TaskStatistics stats)
        {
            if (stats.TotalExecutions == 0 || stats.LastExecutionStart == null || stats.LastExecutionEnd == null)
            {
                return TimeSpan.Zero;
            }

            var totalExecutionTime = (stats.LastExecutionEnd.Value - stats.LastExecutionStart.Value).TotalMilliseconds;
            return TimeSpan.FromMilliseconds(totalExecutionTime / stats.TotalExecutions);
        }

        /// <summary>
        /// 记录任务成功日志
        /// </summary>
        /// <param name="stats"></param>
        private static void LogTaskSuccess(TaskStatistics stats)
        {
            // 不再记录任务执行成功统计日志
            // var logContent = new Logging.Custom.LogContent(
            //     $"任务执行成功统计", 
            //     null, 
            //     "TaskSuccessStatistics"
            // )
            // {
            //     AdditionalInfo = $"任务名称: {stats.TaskName}, 总执行次数: {stats.TotalExecutions}, 成功次数: {stats.SuccessExecutions}, 失败次数: {stats.FailureExecutions}, 平均执行时间: {stats.AverageExecutionTime.TotalMilliseconds:F2}ms"
            // };

            // LogHelperUtil.WriteInfo(logContent);
        }

        /// <summary>
        /// 记录任务失败日志
        /// </summary>
        /// <param name="stats"></param>
        private static void LogTaskFailure(TaskStatistics stats)
        {
            // var logContent = new Logging.Custom.LogContent(
            //     $"任务执行失败统计", 
            //     null, 
            //     "TaskFailureStatistics"
            // )
            // {
            //     AdditionalInfo = $"任务名称: {stats.TaskName}, 总执行次数: {stats.TotalExecutions}, 成功次数: {stats.SuccessExecutions}, 失败次数: {stats.FailureExecutions}, 上次错误: {stats.LastErrorMessage}"
            // };

            // LogHelperUtil.WriteError(logContent);
        }
    }

    /// <summary>
    /// 任务统计信息
    /// </summary>
    public class TaskStatistics
    {
        public string TaskName { get; set; }
        public int TotalExecutions { get; set; } = 0;
        public int SuccessExecutions { get; set; } = 0;
        public int FailureExecutions { get; set; } = 0;
        public DateTime? LastExecutionStart { get; set; }
        public DateTime? LastExecutionEnd { get; set; }
        public DateTime? LastSuccessExecution { get; set; }
        public DateTime? LastFailureExecution { get; set; }
        public string LastErrorMessage { get; set; }
        public TimeSpan AverageExecutionTime { get; set; } = TimeSpan.Zero;

        public double SuccessRate => TotalExecutions > 0 ? (double)SuccessExecutions / TotalExecutions * 100 : 0;
    }
}
