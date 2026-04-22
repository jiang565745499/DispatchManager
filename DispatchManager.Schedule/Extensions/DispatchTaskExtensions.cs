using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Service;
using Longbow.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Schedule.Extensions
{
    public static class DispatchTaskExtensions
    {
        public static List<DispatchTask> ToTasksModelList(this IEnumerable<IScheduler> schedulers) => schedulers.Select(i => new DispatchTask
        {
            Name = i.Name,
            CreateTime = i.CreatedTime,
            LastRuntime = i.LastRuntime,
            NextRuntime = i.NextRuntime,
            LastRunResult = i.LastRunResult,
            Status = i.Status,
            Trigger = i.Triggers.First().ToString()
        }).ToList();

        public static List<DispatchTaskKingDee> ToTasksModelListKingDee(this IEnumerable<IScheduler> schedulers) => schedulers.Select(i => new DispatchTaskKingDee
        {
            Name = i.Name,
            CreateTime = i.CreatedTime,
            LastRuntime = i.LastRuntime,
            NextRuntime = i.NextRuntime,
            LastRunResult = i.LastRunResult,
            Status = i.Status,
            Trigger = i.Triggers.First().ToString()
        }).ToList();
    }
}
