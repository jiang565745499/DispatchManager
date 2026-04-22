using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using System.Collections.Generic;

namespace DispatchManager.DataAccess.FreeSql.InterFace
{
    public interface ISqliteLog
    {
        /// <summary>
        /// 获取所有Log
        /// </summary>
        /// <returns></returns>
        List<Log> GetAllLog();

        /// <summary>
        /// 获取接口调用的Log
        /// </summary>
        /// <returns></returns>
        List<Log> GetInterLog();

        /// <summary>
        /// 获取符合条件的接口Log（分页 + 服务端排序/搜索）
        /// </summary>
        List<Log> GetInterLog(int pageIndex, int pageSize, out long total, string? sortName = null, bool sortAsc = false, string? searchText = null, HashSet<int>? taskNameMatchedIds = null);

        /// <summary>
        /// 清理指定天数之前的历史日志
        /// </summary>
        /// <param name="retainDays">保留最近多少天，默认 90 天</param>
        /// <returns>删除的行数</returns>
        int CleanupOldLogs(int retainDays = 90);

        /// <summary>
        /// 按照日志级别获取日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <returns>日志集合</returns>
        List<Log> GetLogsByLevel(string level);

        /// <summary>
        /// 获取日志统计数据
        /// </summary>
        /// <param name="days">统计的天数范围</param>
        /// <returns>日志统计字典</returns>
        Dictionary<string, int> GetLogStatistics(int days = 7);

        /// <summary>
        /// 获取日志总数
        /// </summary>
        /// <returns>日志条数</returns>
        long GetLogCount();
    }
}
