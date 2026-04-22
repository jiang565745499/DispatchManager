using DispatchManager.DataAccess.FreeSql.InterFace;
using DispatchManager.DataAccess.FreeSql.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DispatchManager.DataAccess.FreeSql.Service
{
    public class DispatchClassService : IDispatchClass
    {
        private readonly IFreeSql freeSql;
        private readonly IMemoryCache memoryCache;
        private const string TaskListCacheKey = "TaskListCache";
        private const string TaskViewListCacheKey = "TaskViewListCache";

        public DispatchClassService([FromKeyedServices("MainDB")] IFreeSql freeSql, IMemoryCache memoryCache)
        {
            this.freeSql = freeSql;
            this.memoryCache = memoryCache;
        }

        public List<DispatchClass> GetAllDispatchClass()
        {
            return freeSql.Select<DispatchClass>().ToList();
        }

        public DispatchClass? GetColorByName(string? Name)
        {
            return freeSql.Select<DispatchClass>().Where(x => x.ClassName == Name).ToOne();
        }

        public DispatchClass? GetDispatchClassByID(long? iD)
        {
            return freeSql.Select<DispatchClass>().Where(x => x.ID == iD).ToOne();
        }

        public void UpdateSingle(DispatchClass model) {
            freeSql!.Update<DispatchClass>()
                    .Set(item => item.ClassName, model.ClassName)
                    .Set(item => item.Y9Key, model.Y9Key)
                    .Set(item => item.FColor, model.FColor)
                    .Set(item => item.KingDeeAccountID, model.KingDeeAccountID)
                    .Set(item => item.KingDeeUserName, model.KingDeeUserName)
                    .Set(item => item.KingDeeAppID, model.KingDeeAppID)
                    .Set(item => item.KingDeeAppSec, model.KingDeeAppSec)
                    .Set(item => item.KingDeeLCID, model.KingDeeLCID)
                    .Set(item => item.KingDeeServerUrl, model.KingDeeServerUrl)
                    .Where(item => item.ID == model.ID)
                    .ExecuteAffrows();
            
            // 清除任务相关缓存，确保下次获取任务时使用最新的Y9密钥
            ClearTaskCache();
        }

        public long ReturnID(DispatchClass model)
        {
            var id = freeSql!.Insert(model).ExecuteIdentity();
            
            // 清除任务相关缓存
            ClearTaskCache();
            
            return id;
        }

        public void DeleteSingle(DispatchClass model)
        {
            freeSql!.Delete<DispatchClass>()
                    .Where(item => item.ClassName.Equals(model.ClassName))
                    .ExecuteAffrows();
            
            // 清除任务相关缓存
            ClearTaskCache();
        }
        
        /// <summary>
        /// 清除任务相关缓存
        /// </summary>
        private void ClearTaskCache()
        {
            memoryCache.Remove(TaskListCacheKey);
            memoryCache.Remove(TaskViewListCacheKey);
        }
    }
}
