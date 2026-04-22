using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using Longbow.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.DataAccess.FreeSql.Models
{
    public class DispatchTask
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [NotNull]
        [Column(IsIdentity = true, IsPrimary = true)]
        public long? ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "任务系统")]
        [NotNull]
        [Required(ErrorMessage = "任务系统不能为空哦")]
        public int? ClassID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "任务序号")]
        public int? FNo { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "任务名称")]
        [NotNull]
        [Required(ErrorMessage = "任务名称都空着,你想干嘛?")]
        [PlaceHolder("任务名称不能为空,最大50个字")]
        [MaxLength(50)]
        public string? Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "请求地址")]
        [NotNull]
        [Required(ErrorMessage = "请求地址空着,定时定个寂寞?")]
        [PlaceHolder("请求地址不能为空")]
        public string? ApiUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "回调地址")]
        [PlaceHolder("回调地址填写请求地址后调用的接口")]
        public string? ReturnApiUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "创建时间")]
        public DateTimeOffset CreateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "上次运行时间")]
        public DateTimeOffset? LastRuntime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "下次运行时间")]
        public DateTimeOffset? NextRuntime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Cron表达式")]
        [NotNull]
        [PlaceHolder("Cron表达式不能为空")]
        [Required(ErrorMessage = "Cron表达式空着,等着我来帮你填?")]
        public string? Trigger { get; set; } = "0";

        [Display(Name = "时间间隔")]
        [PlaceHolder("")]
        public int TimeNumber { get; set; } = 1;

        [Display(Name = "时间类型")]
        [PlaceHolder("请选择")]
        public int? TimeType { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "执行结果")]
        [NotNull]
        public TriggerResult LastRunResult { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "任务状态")]
        [NotNull]
        public SchedulerStatus Status { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "是否开启日志")]
        public bool IsLog { get; set; } = false;

        /// <summary>
        /// 回调响应类型（用于区分任务回调的处理逻辑，替代原来通过任务名称关键词判断的方式）
        /// </summary>
        [Display(Name = "响应类型")]
        public ResponseType ResponseType { get; set; } = ResponseType.Default;

        /// <summary>
        /// 是否开启FIS重试（当响应内容中status为500时重试）
        /// </summary>
        [Display(Name = "开启重试")]
        public bool FISRetry { get; set; } = false;

        /// <summary>
        /// DLL文件路径
        /// </summary>
        [Display(Name = "DLL文件路径")]
        public string? DllPath { get; set; }

        /// <summary>
        /// 要执行的方法名
        /// </summary>
        [Display(Name = "执行方法名")]
        public string? MethodName { get; set; }

        /// <summary>
        /// 是否为DLL任务
        /// </summary>
        [Display(Name = "是否为DLL任务")]
        public bool IsDllTask { get; set; } = false;
    }
}
