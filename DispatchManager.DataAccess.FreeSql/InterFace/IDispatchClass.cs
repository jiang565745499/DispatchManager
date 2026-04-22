using DispatchManager.DataAccess.FreeSql.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.DataAccess.FreeSql.InterFace
{
    public interface IDispatchClass
    {
        /// <summary>
        /// 获取所有任务系统
        /// </summary>
        /// <returns></returns>
        List<DispatchClass> GetAllDispatchClass();


        /// <summary>
        /// 根据任务系统获取颜色
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        DispatchClass? GetColorByName(string? Name);
    }
}
