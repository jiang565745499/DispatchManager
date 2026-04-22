using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Logging.Custom
{
    public class LogContent
    {
        public LogContent(string message, long? taskID)
        {
            this.TaskID = taskID;
            this.Message = message;
            this.Timestamp = DateTime.Now;
        }

        public LogContent(string message, long? taskID, string errorType, string? stackTrace = null)
        {
            this.TaskID = taskID;
            this.Message = message;
            this.Timestamp = DateTime.Now;
            this.ErrorType = errorType;
            this.StackTrace = stackTrace;
        }

        public long? TaskID { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string ErrorType { get; set; }
        public string? StackTrace { get; set; }
        public string? AdditionalInfo { get; set; }
    }
}
