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
    public class DispatchTaskKingDee
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
        public string? ApiUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Y9地址")]
        [NotNull]
        [Required(ErrorMessage = "Y9地址空着,金蝶返回的数据抛给谁?")]
        [PlaceHolder("Y9地址不能为空")]
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

        #region 金蝶对接部分

        /// <summary>
        /// 金蝶表单ID
        /// </summary>
        [Display(Name = "金蝶表单ID")]
        [NotNull]
        [Required(ErrorMessage = "金蝶表单ID不能为空")]
        [PlaceHolder("金蝶表单ID不能为空")]
        public string? KingDeeFormId { get; set; }

        /// <summary>
        /// 要查询的金蝶字段集合
        /// </summary>
        [Display(Name = "金蝶字段集合")]
        [NotNull]
        [Required(ErrorMessage = "金蝶字段集合不能为空")]
        [PlaceHolder("金蝶字段集合")]
        public string? KingDeeFields { get; set; }

        /// <summary>
        /// 要查询的金蝶过滤项
        /// </summary>
        [Display(Name = "金蝶过滤项")]
        [PlaceHolder("金蝶过滤项")]
        public string? KingDeeFilterString { get; set; }


        #endregion
    }
}
