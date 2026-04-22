using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DispatchManager.DataAccess.FreeSql.Models
{
    // Serilog.Sinks.SQLite 默认建表结构:
    // id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp TEXT, Level VARCHAR(10),
    // Exception TEXT, RenderedMessage TEXT, Properties TEXT
    [Table(Name = "Log")]
    [Index("idx_log_date", "Timestamp DESC", false)]
    public class Log
    {
        /// <summary>
        /// 自增主键（对应 Serilog 建的 id 列）
        /// </summary>
        [NotNull]
        [Column(Name = "id", IsIdentity = true, IsPrimary = true)]
        public long? ID { get; set; }

        /// <summary>
        /// 时间戳字符串（Serilog 写入格式 yyyy-MM-dd HH:mm:ss.fff +08:00）
        /// </summary>
        [Display(Name = "时间")]
        [Column(Name = "Timestamp")]
        public string? Timestamp { get; set; }

        /// <summary>
        /// 解析 Timestamp 得到的 DateTime，不映射到数据库列
        /// </summary>
        /// 
        [Display(Name = "日志时间")]
        [Column(IsIgnore = true)]
        public DateTime? Date
        {
            get => string.IsNullOrEmpty(Timestamp)
                ? null
                : DateTimeOffset.TryParse(Timestamp, out var dto) ? dto.LocalDateTime : null;
            set { } // Blazor @bind-Field 需要 setter，忽略写入
        }

        [Display(Name = "日志级别")]
        [Column(Name = "Level")]
        public string? Level { get; set; }

        [Display(Name = "异常信息")]
        [Column(Name = "Exception")]
        public string? Exception { get; set; }

        [Display(Name = "渲染消息")]
        [Column(Name = "RenderedMessage")]
        public string? RenderedMessage { get; set; }

        [Display(Name = "日志消息")]
        [Column(IsIgnore = true)]
        public string? Message
        {
            get => RenderedMessage;
            set { } // Blazor @bind-Field 需要 setter，忽略写入
        }

        [Display(Name = "附加属性")]
        [Column(Name = "Properties")]
        public string? Properties { get; set; }

        /// <summary>
        /// DispatchTask 主键
        /// </summary>
        [Display(Name = "任务ID")]
        [Column(Name = "TaskId")]
        public long? TaskID { get; set; }
    }
}
