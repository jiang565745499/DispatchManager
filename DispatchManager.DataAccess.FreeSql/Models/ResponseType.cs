namespace DispatchManager.DataAccess.FreeSql.Models
{
    /// <summary>
    /// 任务回调响应类型（对应原 task.Name.ToUpper().Contains(关键词) 的判断逻辑）
    /// </summary>
    public enum ResponseType
    {
        /// <summary>
        /// 默认 JSON 回调 —— 标准 PostResponse
        /// </summary>
        Default = 0,

        /// <summary>
        /// 普通 XML 接口 —— Content-Type: application/xml 回调
        /// </summary>
        XML = 1,
    }
}
