namespace DispatchManager.Logging
{
    /// <summary>
    /// 日志记录接口 - 统一的日志记录标准
    /// </summary>
    public interface ILogRecorder
    {
        /// <summary>记录调试信息</summary>
        void Debug(string message, string? category = null);

        /// <summary>记录信息</summary>
        void Info(string message, string? category = null);

        /// <summary>记录警告</summary>
        void Warning(string message, string? category = null);

        /// <summary>记录错误</summary>
        void Error(string message, Exception? exception = null, string? category = null);

        /// <summary>记录严重错误</summary>
        void Fatal(string message, Exception? exception = null, string? category = null);

        /// <summary>记录带标签的业务日志</summary>
        void LogBusiness(LogLevel level, string message, long? taskId = null, string? taskName = null, string? result = null);

        /// <summary>异步记录日志</summary>
        Task LogAsync(LogLevel level, string message, Exception? exception = null, string? category = null);
    }
}
