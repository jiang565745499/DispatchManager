using DispatchManager.DataAccess.FreeSql.InterFace;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Models.View;
using Longbow.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DispatchManager.DataAccess.FreeSql.Service
{
    /// <summary>
    /// 金蝶服务类
    /// </summary>
    public class ScheduleTaskKingDeeService : IScheduleTaskKingDee
    {
        private readonly IFreeSql freeSql;

        public ScheduleTaskKingDeeService([FromKeyedServices("MainDB")] IFreeSql freeSql)
        {
            this.freeSql = freeSql;
        }


        public List<DispatchTaskKingDee> GetAllEnableDispatchTask()
        {
            return freeSql.Select<DispatchTaskKingDee>().Where(x => x.Status != Longbow.Tasks.SchedulerStatus.Disabled).ToList();
        }

        public List<DispatchTaskKingDee> GetAllDispatchTask()
        {
            return freeSql.Select<DispatchTaskKingDee>().ToList();

        }

        public List<DispatchTaskKingDeeView> GetAllDispatchTaskKingDeeView()
        {
            List <DispatchTaskKingDee> KDDate = freeSql.Select<DispatchTaskKingDee>().ToList();
            List<DispatchTaskKingDeeView> KDDateView = new();
            foreach(var item in KDDate)
            {
                DispatchTaskKingDeeView dispatchTaskKingDeeView = new DispatchTaskKingDeeView(item);
                var TaskClass = freeSql.Select<DispatchClass>().Where(x => x.ID == item.ClassID).ToOne();
                dispatchTaskKingDeeView.Y9Key = TaskClass.Y9Key;
                dispatchTaskKingDeeView.KingDeeAccountID = TaskClass.KingDeeAccountID;
                dispatchTaskKingDeeView.KingDeeAppID = TaskClass.KingDeeAppID;
                dispatchTaskKingDeeView.KingDeeAppSec = TaskClass.KingDeeAppSec;
                dispatchTaskKingDeeView.KingDeeLCID = TaskClass.KingDeeLCID;
                dispatchTaskKingDeeView.KingDeeServerUrl = TaskClass.KingDeeServerUrl;
                dispatchTaskKingDeeView.KingDeeUserName = TaskClass.KingDeeUserName;
                KDDateView.Add(dispatchTaskKingDeeView);
            }

            return KDDateView;
        }

        public DispatchTaskKingDee? GetDispatchTaskByID(long? ID)
        {
            return freeSql.Select<DispatchTaskKingDee>().Where(x => x.ID == ID).ToOne();
        }

        public DispatchTaskKingDee? GetDispatchTaskByName(string? Name)
        {
            return freeSql.Select<DispatchTaskKingDee>().Where(x => x.Name == Name).ToOne();
        }

        public DispatchTask? GetDispatchTaskByName2(string? Name)
        {
            return freeSql.Select<DispatchTask>().Where(x => x.Name == Name).ToOne();
        }

        public void UpdateSingleRun(DispatchTaskKingDee model)
        {
            freeSql!.Update<DispatchTaskKingDee>()
                .Set(item => item.Status, SchedulerStatus.Running)
                .Where(item => item.ID == model.ID || item.Name == model.Name)
                .ExecuteAffrows();
        }

        public void UpdateSingleDisable(DispatchTaskKingDee model)
        {
            freeSql!.Update<DispatchTaskKingDee>()
                .Set(item => item.Status, SchedulerStatus.Disabled)
                .Where(item => item.Name.Equals(model.Name))
                .ExecuteAffrows();
        }

        public void DeleteSingle(DispatchTaskKingDee model)
        {
            freeSql!.Delete<DispatchTaskKingDee>()
                    .Where(item => item.Name.Equals(model.Name))
                    .ExecuteAffrows();
        }

        public long ReturnID(DispatchTaskKingDee model)
        {
            return freeSql!.Insert(model).ExecuteIdentity();
        }

        public void UpdateSingleAll(DispatchTaskKingDee model)
        {
            // 更新数据
            freeSql!.Update<DispatchTaskKingDee>()
            .Set(item => item.Name, model.Name)
            .Set(item => item.ApiUrl, model.ApiUrl)
            .Set(item => item.ReturnApiUrl, model.ReturnApiUrl)
            .Set(item => item.Trigger, model.Trigger)
            .Set(item => item.ClassID, model.ClassID)
            .Set(item => item.FNo, model.FNo)
            .Set(item => item.Status, model.Status)
            .Set(item => item.IsLog, model.IsLog)
            .Set(item => item.KingDeeFormId, model.KingDeeFormId)
            .Set(item => item.KingDeeFields, model.KingDeeFields)
            .Set(item => item.KingDeeFilterString, model.KingDeeFilterString)
            .Where(item => item.ID == model.ID)
            .ExecuteAffrows();
        }
    }
}
