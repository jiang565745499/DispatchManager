using Microsoft.AspNetCore.Components;
using DispatchManager.Schedule.Service;
using System.Collections.Generic;
using System.Linq;

namespace DispatchManager.Components.Pages
{
    public partial class PerformanceMetrics
    {
        private List<PerformanceMetric> Metrics { get; set; } = new List<PerformanceMetric>();
        private List<PerformanceRecord> PerformanceRecords { get; set; } = new List<PerformanceRecord>();
        private List<PerformanceRecord> FilteredRecords { get; set; } = new List<PerformanceRecord>();
        private List<PerformanceRecord> SlowQueries { get; set; } = new List<PerformanceRecord>();
        private string _operationFilter { get; set; } = string.Empty;
        private int _slowQueryThreshold { get; set; } = 1000;

        /// <summary>
        /// 总操作类型数
        /// </summary>
        private int TotalOperations => Metrics.Count;

        /// <summary>
        /// 总执行次数
        /// </summary>
        private int TotalExecutions => Metrics.Sum(m => m.TotalExecutions);

        /// <summary>
        /// 总错误次数
        /// </summary>
        private int TotalErrors => Metrics.Sum(m => m.ErrorCount);

        /// <summary>
        /// 平均执行时间
        /// </summary>
        private string AverageExecutionTime
        {
            get
            {
                if (Metrics.Count == 0)
                    return "0.00";

                var totalTime = Metrics.Sum(m => m.AverageExecutionTime * m.TotalExecutions);
                var totalExecutions = TotalExecutions;
                return totalExecutions > 0 ? (totalTime / totalExecutions).ToString("F2") : "0.00";
            }
        }

        /// <summary>
        /// 组件初始化
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            RefreshMetrics();
        }

        /// <summary>
        /// 刷新性能指标
        /// </summary>
        private void RefreshMetrics()
        {
            Metrics = PerformanceMetricsService.GetAllMetrics();
            PerformanceRecords = PerformanceMetricsService.GetPerformanceRecords(maxRecords: 100);
            FilteredRecords = PerformanceRecords;
            RefreshSlowQueries();
            StateHasChanged();
        }

        /// <summary>
        /// 刷新慢查询
        /// </summary>
        private void RefreshSlowQueries()
        {
            SlowQueries = PerformanceMetricsService.GetSlowQueries(_slowQueryThreshold, 50);
            StateHasChanged();
        }

        /// <summary>
        /// 重置性能指标
        /// </summary>
        private void ResetMetrics()
        {
            PerformanceMetricsService.ResetMetrics();
            RefreshMetrics();
        }

        /// <summary>
        /// 过滤性能记录
        /// </summary>
        private void FilterRecords()
        {
            if (string.IsNullOrEmpty(_operationFilter))
            {
                FilteredRecords = PerformanceRecords;
            }
            else
            {
                FilteredRecords = PerformanceRecords.Where(r => 
                    r.OperationName.Contains(_operationFilter, System.StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            StateHasChanged();
        }
    }
}