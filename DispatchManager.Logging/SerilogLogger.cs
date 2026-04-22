using Serilog;
using Serilog.Events;
using Serilog.Sinks.SQLite;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DispatchManager.Logging
{
    /// <summary>
    /// Serilog实现的日志记录器
    /// </summary>
    public class SerilogLogger : ILogRecorder
    {
        private readonly ILogger _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SerilogLogger()
        {
            // 使用已配置的Serilog实例
            _logger = Log.Logger;
        }

        /// <summary>
        /// 记录调试信息
        /// </summary>
        public void Debug(string message, string? category = null)
        {
            _logger.Debug(message);
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        public void Info(string message, string? category = null)
        {
            _logger.Information(message);
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        public void Warning(string message, string? category = null)
        {
            _logger.Warning(message);
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        public void Error(string message, Exception? exception = null, string? category = null)
        {
            if (exception != null)
                _logger.Error(exception, message);
            else
                _logger.Error(message);
        }

        /// <summary>
        /// 记录严重错误
        /// </summary>
        public void Fatal(string message, Exception? exception = null, string? category = null)
        {
            if (exception != null)
                _logger.Fatal(exception, message);
            else
                _logger.Fatal(message);
        }

        /// <summary>
        /// 记录带标签的业务日志
        /// </summary>
        public void LogBusiness(LogLevel level, string message, long? taskId = null, string? taskName = null, string? result = null)
        {
            var logEventLevel = ConvertToSerilogLevel(level);
            _logger.Write(logEventLevel, message);
        }

        /// <summary>
        /// 异步记录日志
        /// </summary>
        public Task LogAsync(LogLevel level, string message, Exception? exception = null, string? category = null)
        {
            var logEventLevel = ConvertToSerilogLevel(level);
            return Task.Run(() =>
            {
                if (exception != null)
                    _logger.Write(logEventLevel, exception, message);
                else
                    _logger.Write(logEventLevel, message);
            });
        }

        /// <summary>
        /// 将自定义LogLevel转换为Serilog的LogEventLevel
        /// </summary>
        private LogEventLevel ConvertToSerilogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Info:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Fatal:
                    return LogEventLevel.Fatal;
                default:
                    return LogEventLevel.Information;
            }
        }
    }
}