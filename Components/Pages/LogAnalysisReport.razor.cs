using Microsoft.AspNetCore.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Service;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatchManager.Components.Pages
{
    public partial class LogAnalysisReport
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        private string _timeRange = "24h";
        private DateTime? _startDate = DateTime.Now.AddDays(-1);
        private DateTime? _endDate = DateTime.Now;
        private string _logLevel = "";
        private string _taskId = "";
        private string _reportFormat = "html";

        private ReportData _reportData { get; set; } = null;

        /// <summary>
        /// 生成报告
        /// </summary>
        private async Task GenerateReport()
        {
            // 确定时间范围
            DateTime startDate, endDate;
            if (_timeRange == "custom" && _startDate.HasValue && _endDate.HasValue)
            {
                startDate = _startDate.Value;
                endDate = _endDate.Value;
            }
            else
            {
                endDate = DateTime.Now;
                switch (_timeRange)
                {
                    case "7d":
                        startDate = endDate.AddDays(-7);
                        break;
                    case "30d":
                        startDate = endDate.AddDays(-30);
                        break;
                    default: // 24h
                        startDate = endDate.AddHours(-24);
                        break;
                }
            }

            // 获取日志数据
            var allLogs = LogDb.GetAllLog();
            var filteredLogs = allLogs.Where(l => l.Date >= startDate && l.Date <= endDate);

            // 应用过滤条件
            if (!string.IsNullOrEmpty(_logLevel))
            {
                filteredLogs = filteredLogs.Where(l => l.Level?.ToUpper() == _logLevel);
            }

            if (!string.IsNullOrEmpty(_taskId) && int.TryParse(_taskId, out int taskId))
            {
                filteredLogs = filteredLogs.Where(l => l.TaskID == taskId);
            }

            var logs = filteredLogs.ToList();

            // 生成报告数据
            _reportData = GenerateReportData(logs, startDate, endDate);

            // 生成时间趋势数据并初始化图表
            var trendData = GenerateTrendData(logs, startDate, endDate);
            await JSRuntime.InvokeVoidAsync("initReportChart", trendData);

            StateHasChanged();
        }

        /// <summary>
        /// 生成报告数据
        /// </summary>
        private ReportData GenerateReportData(List<Log> logs, DateTime startDate, DateTime endDate)
        {
            var totalLogs = logs.Count;
            var errorLogs = logs.Count(l => l.Level?.ToLower() == "error");
            var errorRate = totalLogs > 0 ? (double)errorLogs / totalLogs * 100 : 0;

            // 计算时间范围的小时数
            var hours = (endDate - startDate).TotalHours;
            var averageLogFrequency = hours > 0 ? totalLogs / hours : 0;

            // 分析日志级别分布
            var logLevelDistribution = logs
                .GroupBy(l => l.Level?.ToUpper() ?? "UNKNOWN")
                .Select(g => new LogLevelDistribution
                {
                    Level = g.Key,
                    Count = g.Count(),
                    Percentage = totalLogs > 0 ? (double)g.Count() / totalLogs * 100 : 0
                })
                .OrderByDescending(d => d.Count)
                .ToList();

            // 分析任务错误统计
            var taskErrorStats = logs
                .Where(l => l.TaskID.HasValue && l.TaskID > 0)
                .GroupBy(l => l.TaskID)
                .Select(g => new TaskErrorStat
                {
                    TaskId = (int)g.Key.Value,
                    TotalCount = g.Count(),
                    ErrorCount = g.Count(l => l.Level?.ToLower() == "error"),
                    ErrorRate = g.Count() > 0 ? (double)g.Count(l => l.Level?.ToLower() == "error") / g.Count() * 100 : 0
                })
                .OrderByDescending(s => s.ErrorRate)
                .Take(10)
                .ToList();

            // 计算问题任务数（错误率 > 5%）
            var problematicTasks = taskErrorStats.Count(s => s.ErrorRate > 5);

            return new ReportData
            {
                TotalLogs = totalLogs,
                ErrorRate = errorRate,
                AverageLogFrequency = averageLogFrequency,
                ProblematicTasks = problematicTasks,
                LogLevelDistribution = logLevelDistribution,
                TaskErrorStats = taskErrorStats
            };
        }

        /// <summary>
        /// 生成时间趋势数据
        /// </summary>
        private object GenerateTrendData(List<Log> logs, DateTime startDate, DateTime endDate)
        {
            var hours = (endDate - startDate).TotalHours;
            int intervalHours = hours <= 24 ? 1 : hours <= 168 ? 6 : 24;
            int pointCount = (int)Math.Ceiling(hours / intervalHours);

            var labels = new List<string>();
            var errorData = new List<int>();
            var warnData = new List<int>();
            var infoData = new List<int>();

            for (int i = 0; i < pointCount; i++)
            {
                var currentStart = startDate.AddHours(i * intervalHours);
                var currentEnd = currentStart.AddHours(intervalHours);

                labels.Add(currentStart.ToString(intervalHours == 1 ? "HH:00" : "MM-dd"));

                var periodLogs = logs.Where(l => l.Date >= currentStart && l.Date < currentEnd);
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
        /// 导出报告为HTML格式
        /// </summary>
        private async Task ExportReportAsHtml()
        {
            if (_reportData == null)
                return;

            var htmlContent = GenerateHtmlReport();
            var bytes = Encoding.UTF8.GetBytes(htmlContent);
            var fileName = $"日志分析报告_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.html";

            await DownloadFile(bytes, fileName, "text/html");
        }

        /// <summary>
        /// 导出报告为CSV格式
        /// </summary>
        private async Task ExportReportAsCsv()
        {
            if (_reportData == null)
                return;

            var csvContent = GenerateCsvReport();
            var bytes = Encoding.UTF8.GetBytes(csvContent);
            var fileName = $"日志分析报告_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv";

            await DownloadFile(bytes, fileName, "text/csv");
        }

        /// <summary>
        /// 生成HTML报告
        /// </summary>
        private string GenerateHtmlReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"zh-CN\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("<title>日志分析报告</title>");
            sb.AppendLine("<link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css\" rel=\"stylesheet\">");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container mt-5\">");
            sb.AppendLine($"<h1 class=\"text-center mb-5\">日志分析报告 - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}</h1>");
            
            // 报告摘要
            sb.AppendLine("<div class=\"card mb-5\">");
            sb.AppendLine("<div class=\"card-header bg-primary text-white\">");
            sb.AppendLine("<h2>报告摘要</h2>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"card-body\">");
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine($"<div class=\"col-md-3 text-center\"><h3>{_reportData.TotalLogs}</h3><p>总日志数</p></div>");
            sb.AppendLine($"<div class=\"col-md-3 text-center\"><h3 class=\"text-danger\">{_reportData.ErrorRate.ToString("F2")}%</h3><p>错误率</p></div>");
            sb.AppendLine($"<div class=\"col-md-3 text-center\"><h3>{_reportData.AverageLogFrequency.ToString("F2")}</h3><p>平均日志频率/小时</p></div>");
            sb.AppendLine($"<div class=\"col-md-3 text-center\"><h3 class=\"text-warning\">{_reportData.ProblematicTasks}</h3><p>问题任务数</p></div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // 日志级别分布
            sb.AppendLine("<div class=\"card mb-5\">");
            sb.AppendLine("<div class=\"card-header bg-primary text-white\">");
            sb.AppendLine("<h2>日志级别分布</h2>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"card-body\">");
            sb.AppendLine("<table class=\"table table-striped\">");
            sb.AppendLine("<thead><tr><th>级别</th><th>数量</th><th>占比</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var item in _reportData.LogLevelDistribution)
            {
                sb.AppendLine($"<tr><td>{item.Level}</td><td>{item.Count}</td><td>{item.Percentage.ToString("F2")}%</td></tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // 任务错误统计
            sb.AppendLine("<div class=\"card mb-5\">");
            sb.AppendLine("<div class=\"card-header bg-primary text-white\">");
            sb.AppendLine("<h2>任务错误统计</h2>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"card-body\">");
            sb.AppendLine("<table class=\"table table-striped\">");
            sb.AppendLine("<thead><tr><th>任务ID</th><th>错误次数</th><th>总日志数</th><th>错误率</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var item in _reportData.TaskErrorStats)
            {
                sb.AppendLine($"<tr><td>{item.TaskId}</td><td>{item.ErrorCount}</td><td>{item.TotalCount}</td><td>{item.ErrorRate.ToString("F2")}%</td></tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
            sb.AppendLine("<script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js\"></script>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /// <summary>
        /// 生成CSV报告
        /// </summary>
        private string GenerateCsvReport()
        {
            var sb = new StringBuilder();
            
            // 报告摘要
            sb.AppendLine("报告摘要");
            sb.AppendLine($"总日志数,{_reportData.TotalLogs}");
            sb.AppendLine($"错误率,{_reportData.ErrorRate.ToString("F2")}%");
            sb.AppendLine($"平均日志频率/小时,{_reportData.AverageLogFrequency.ToString("F2")}");
            sb.AppendLine($"问题任务数,{_reportData.ProblematicTasks}");
            sb.AppendLine();

            // 日志级别分布
            sb.AppendLine("日志级别分布");
            sb.AppendLine("级别,数量,占比");
            foreach (var item in _reportData.LogLevelDistribution)
            {
                sb.AppendLine($"{item.Level},{item.Count},{item.Percentage.ToString("F2")}%");
            }
            sb.AppendLine();

            // 任务错误统计
            sb.AppendLine("任务错误统计");
            sb.AppendLine("任务ID,错误次数,总日志数,错误率");
            foreach (var item in _reportData.TaskErrorStats)
            {
                sb.AppendLine($"{item.TaskId},{item.ErrorCount},{item.TotalCount},{item.ErrorRate.ToString("F2")}%");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        private async Task DownloadFile(byte[] bytes, string fileName, string contentType)
        {
            await JSRuntime.InvokeVoidAsync("downloadFile", fileName, contentType, Convert.ToBase64String(bytes));
        }
    }

    /// <summary>
    /// 报告数据
    /// </summary>
    public class ReportData
    {
        public int TotalLogs { get; set; }
        public double ErrorRate { get; set; }
        public double AverageLogFrequency { get; set; }
        public int ProblematicTasks { get; set; }
        public List<LogLevelDistribution> LogLevelDistribution { get; set; } = new List<LogLevelDistribution>();
        public List<TaskErrorStat> TaskErrorStats { get; set; } = new List<TaskErrorStat>();
    }

    /// <summary>
    /// 日志级别分布
    /// </summary>
    public class LogLevelDistribution
    {
        public string Level { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 任务错误统计
    /// </summary>
    public class TaskErrorStat
    {
        public int TaskId { get; set; }
        public int TotalCount { get; set; }
        public int ErrorCount { get; set; }
        public double ErrorRate { get; set; }
    }
}