using DispatchManager.DataAccess.FreeSql.InterFace;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Models.View;
using Longbow.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace DispatchManager.DataAccess.FreeSql.Service
{
    public class ScheduleTaskService: IScheduleTask
    {
        private readonly IFreeSql freeSql;
        private readonly IMemoryCache memoryCache;
        private const string TaskListCacheKey = "TaskListCache";
        private const string TaskViewListCacheKey = "TaskViewListCache";
        private const int CacheExpirationMinutes = 5;

        public ScheduleTaskService([FromKeyedServices("MainDB")] IFreeSql freeSql, IMemoryCache memoryCache)
        {
            this.freeSql = freeSql;
            this.memoryCache = memoryCache;
        }

        public List<DispatchTask> GetAllEnableDispatchTask()
        {
            return GetAllDispatchTask().Where(x => x.Status != Longbow.Tasks.SchedulerStatus.Disabled).ToList();
        }

        public List<DispatchTask> GetAllDispatchTask()
        {
            return memoryCache.GetOrCreate(TaskListCacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(CacheExpirationMinutes);
                return freeSql.Select<DispatchTask>().ToList();
            });
        }

        public List<DispatchTaskView> GetAllDispatchTaskView()
        {
            return memoryCache.GetOrCreate(TaskViewListCacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(CacheExpirationMinutes);
                List<DispatchTask> KDDate = GetAllDispatchTask();
                List<DispatchTaskView> dispatchTaskViews = new();
                
                // 批量获取所有需要的 TaskClass
                var classIds = KDDate.Select(x => x.ClassID).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
                var taskClasses = freeSql.Select<DispatchClass>().Where(x => x.ID.HasValue && classIds.Contains(Convert.ToInt32(x.ID.Value))).ToList().ToDictionary(x => (int)x.ID.Value);
                
                foreach (var item in KDDate)
                {
                    DispatchTaskView dispatchTaskView = new DispatchTaskView(item);
                    if (item.ClassID.HasValue && taskClasses.TryGetValue(item.ClassID.Value, out var taskClass))
                    {
                        dispatchTaskView.Y9Key = taskClass.Y9Key;
                    }
                    dispatchTaskViews.Add(dispatchTaskView);
                }

                return dispatchTaskViews;
            });
        }

        public DispatchTask? GetDispatchTaskByID(long? ID)
        {
            if (ID == null)
                return null;
            
            var cacheKey = $"TaskByID:{ID}";
            return memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(CacheExpirationMinutes);
                return freeSql.Select<DispatchTask>().Where(x => x.ID == ID).ToOne();
            });
        }

        public DispatchTask? GetDispatchTaskByName(string? Name)
        {
            if (string.IsNullOrEmpty(Name))
                return null;
            
            var cacheKey = $"TaskByName:{Name}";
            return memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(CacheExpirationMinutes);
                return freeSql.Select<DispatchTask>().Where(x => x.Name == Name).ToOne();
            });
        }

        public DispatchTaskKingDee? GetDispatchTaskKingDeeByName(string? Name)
        {
            if (string.IsNullOrEmpty(Name))
                return null;
            
            var cacheKey = $"TaskKingDeeByName:{Name}";
            return memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(CacheExpirationMinutes);
                return freeSql.Select<DispatchTaskKingDee>().Where(x => x.Name == Name).ToOne();
            });
        }

        public void UpdateSingleRun(DispatchTask model)
        {
            freeSql!.Update<DispatchTask>()
                .Set(item => item.Status, SchedulerStatus.Running)
                .Where(item => item.ID == model.ID || item.Name == model.Name)
                .ExecuteAffrows();
            
            // 清除缓存
            ClearTaskCache(model);
        }

        public void UpdateSingleDisable(DispatchTask model)
        {
            freeSql!.Update<DispatchTask>()
                .Set(item => item.Status, SchedulerStatus.Disabled)
                .Where(item => item.Name.Equals(model.Name))
                .ExecuteAffrows();
            
            // 清除缓存
            ClearTaskCache(model);
        }

        public void DeleteSingle(DispatchTask model)
        {
            freeSql!.Delete<DispatchTask>()
                    .Where(item => item.Name.Equals(model.Name))
                    .ExecuteAffrows();
            
            // 清除缓存
            ClearTaskCache(model);
        }

        public long ReturnID(DispatchTask model)
        {
            var id = freeSql!.Insert(model).ExecuteIdentity();

            // 清除列表缓存，同时清除按名称查询的缓存
            // （新增前的重复校验会将 null 结果缓存到 TaskByName，导致新增后立即查询仍返回 null）
            ClearListCache();
            memoryCache.Remove($"TaskByName:{model.Name}");

            return id;
        }

        public void UpdateSingleAll(DispatchTask model)
        {
            // 更新数据
            freeSql!.Update<DispatchTask>()
            .Set(item => item.Name, model.Name)
            .Set(item => item.ApiUrl, model.ApiUrl)
            .Set(item => item.ReturnApiUrl, model.ReturnApiUrl)
            .Set(item => item.Trigger, model.Trigger)
            .Set(item => item.ClassID, model.ClassID)
            .Set(item => item.FNo, model.FNo)
            .Set(item => item.Status, model.Status)
            .Set(item => item.IsLog, model.IsLog)
            .Set(item => item.FISRetry, model.FISRetry)
            .Set(item => item.DllPath, model.DllPath)
            .Set(item => item.MethodName, model.MethodName)
            .Set(item => item.IsDllTask, model.IsDllTask)
            .Where(item => item.ID == model.ID)
            .ExecuteAffrows();
            
            // 清除缓存
            ClearTaskCache(model);
        }

        #region 缓存管理

        private void ClearTaskCache(DispatchTask model)
        {
            // 清除单个任务缓存
            memoryCache.Remove($"TaskByID:{model.ID}");
            memoryCache.Remove($"TaskByName:{model.Name}");
            
            // 清除列表缓存
            ClearListCache();
        }

        private void ClearListCache()
        {
            memoryCache.Remove(TaskListCacheKey);
            memoryCache.Remove(TaskViewListCacheKey);
        }

        #endregion
    }
}
