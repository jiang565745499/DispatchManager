using DispatchManager.Logging.Custom;
using System;
using System.ComponentModel;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Service;

namespace DispatchManager.Logging
{
    /// <summary>
    /// 日志工具类 - 提供统一的日志记录接口
    /// 支持log4net原生接口和新的ILogRecorder接口
    /// </summary>
    public class LogHelperUtil
    {
        private LogHelperUtil() { }



        // 新的ILogRecorder接口实例（会由DI容器注入）
        private static ILogRecorder? _logRecorder;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static LogHelperUtil()
        {
            // 延迟初始化，等待Log.Logger被配置后再创建
        }

        /// <summary>
        /// 初始化日志记录器
        /// </summary>
        public static void Initialize(ILogRecorder logRecorder)
        {
            _logRecorder = logRecorder ?? throw new ArgumentNullException(nameof(logRecorder));
        }

        /// <summary>
        /// 获取当前日志记录器
        /// </summary>
        public static ILogRecorder GetLogRecorder()
        {
            if (_logRecorder == null)
            {
                // 如果没有初始化，创建一个新的实例
                _logRecorder = new SerilogLogger();
            }
            return _logRecorder;
        }



        #region 一般日志记录（log4net模式，保留向后兼容）

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public static void WriteInfo(LogContent info)
        {
            if (!IsLogContentEmpty(info))
            {
                // 使用Serilog记录
                GetLogRecorder().Info(info.Message ?? "");
            }
        }

        /// <summary>
        /// 记录错误日志（包含异常信息）
        /// </summary>
        public static void WriteError(LogContent info, Exception se)
        {
            // 若 info 为空但有异常，自动补全 Message 为异常信息
            if (IsLogContentEmpty(info))
            {
                if (se == null) return;
                if (info == null) info = new LogContent(se.Message, null);
                else info.Message = se.Message;
            }

            // 使用Serilog记录
            GetLogRecorder().Error(info.Message ?? "", se);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public static void WriteError(LogContent info)
        {
            if (!IsLogContentEmpty(info))
            {
                // 使用Serilog记录
                GetLogRecorder().Error(info.Message ?? "");
            }
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public static void WriteWarn(LogContent info)
        {
            if (!IsLogContentEmpty(info))
            {
                // 使用Serilog记录
                GetLogRecorder().Warning(info.Message ?? "");
            }
        }

        #endregion

        #region 接口调用日志（log4net模式，保留向后兼容）

        /// <summary>
        /// 记录接口信息日志
        /// </summary>
        public static void WriteInterInfo(LogContent info)
        {
            if (!IsLogContentEmpty(info))
            {
                // 使用Serilog记录
                GetLogRecorder().Info(info.Message ?? "");
            }
        }

        /// <summary>
        /// 记录接口错误日志（包含异常信息）
        /// </summary>
        public static void WriteInterError(LogContent info, Exception se)
        {
            // 若 info 为空但有异常，自动补全 Message 为异常信息
            if (IsLogContentEmpty(info))
            {
                if (se == null) return;
                if (info == null) info = new LogContent(se.Message, null);
                else info.Message = se.Message;
            }

            // 使用Serilog记录
            GetLogRecorder().Error(info.Message ?? "", se);
        }

        /// <summary>
        /// 记录接口错误日志
        /// </summary>
        public static void WriteInterError(LogContent info)
        {
            if (!IsLogContentEmpty(info))
            {
                // 使用Serilog记录
                GetLogRecorder().Error(info.Message ?? "");
            }
        }

        /// <summary>
        /// 记录调度任务HTTP信息日志
        /// </summary>
        public static void WriteDispatchTaskInfo(DispatchTask? task, SqliteLogService? logService, string message)
        {
            if (task?.IsLog == true)
            {
                logService?.WriteDispatchTaskLog(task, message, category: "DispatchTaskHttp");
            }
        }

        /// <summary>
        /// 记录调度任务HTTP错误日志
        /// </summary>
        public static void WriteDispatchTaskError(DispatchTask? task, SqliteLogService? logService, string message, Exception ex)
        {
            if (task?.IsLog == true)
            {
                logService?.WriteDispatchTaskErrorLog(task, message, ex, "DispatchTaskHttp");
            }
        }

        #endregion

        #region 新的ILogRecorder接口方法

        /// <summary>
        /// 记录调试信息
        /// </summary>
        public static void LogDebug(string message, Exception? ex = null)
        {
            GetLogRecorder().Debug(message);
            if (ex != null)
                WriteError(new LogContent(message, null), ex);
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        public static void LogInfo(string message)
        {
            GetLogRecorder().Info(message);
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        public static void LogWarning(string message)
        {
            GetLogRecorder().Warning(message);
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        public static void LogError(string message, Exception? ex = null)
        {
            GetLogRecorder().Error(message);
            if (ex != null)
                WriteError(new LogContent(message, null), ex);
        }

        /// <summary>
        /// 记录致命错误
        /// </summary>
        public static void LogFatal(string message, Exception? ex = null)
        {
            GetLogRecorder().Fatal(message);
            if (ex != null)
                WriteError(new LogContent(message, null), ex);
        }

        /// <summary>
        /// 记录业务日志
        /// </summary>
        public static void LogBusiness(long? taskId, string taskName, int result, string resultMessage = "")
        {
            GetLogRecorder().LogBusiness(LogLevel.Info, resultMessage ?? "任务已完成", taskId, taskName, result.ToString());
        }

        /// <summary>
        /// 异步记录日志
        /// </summary>
        public static Task LogAsync(string message, LogLevel level = LogLevel.Info)
        {
            return GetLogRecorder().LogAsync(level, message);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检查日志内容是否为空
        /// </summary>
        private static bool IsLogContentEmpty(LogContent info)
        {
            if (info == null)
                return true;

            // 检查所有相关属性是否都为空
            return string.IsNullOrWhiteSpace(info.Message) &&
                   string.IsNullOrWhiteSpace(info.ErrorType) &&
                   string.IsNullOrWhiteSpace(info.AdditionalInfo) &&
                   string.IsNullOrWhiteSpace(info.StackTrace);
        }

        #endregion
    }
}
