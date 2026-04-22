using DispatchManager.DataAccess.FreeSql.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.DataAccess.FreeSql.InterFace
{
    public interface IScheduleTask
    {
        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns></returns>
        List<DispatchTask> GetAllDispatchTask();


        /// <summary>
        /// 获取未禁用的任务
        /// </summary>
        /// <returns></returns>
        List<DispatchTask> GetAllEnableDispatchTask();

        /// <summary>
        /// 根据任务名获取任务
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        DispatchTask? GetDispatchTaskByName(string? Name);

        /// <summary>
        /// 根据任务ID获取任务
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        DispatchTask? GetDispatchTaskByID(long? ID);
    }
}
