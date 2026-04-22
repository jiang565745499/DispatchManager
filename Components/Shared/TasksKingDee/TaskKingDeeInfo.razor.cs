using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using Longbow.Tasks;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace DispatchManager.Components.Shared.TasksKingDee
{
    public partial class TaskKingDeeInfo : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        [NotNull]
        [EditorRequired]
        public DispatchTaskKingDee? Model { get; set; }

        private List<ConsoleMessageItem> Messages { get; } = new(24);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var sche = TaskServicesManager.Get(Model.Name);
                if (sche != null)
                {
                    sche.Triggers.First().PulseCallback = async t => await DispatchMessage(t);
                    await DispatchMessage(sche.Triggers.First());
                }
            }
        }

        private async Task DispatchMessage(ITrigger trigger)
        {
            var message = $"任务调度类型({trigger.GetType().Name}) 上次运行时间: {trigger.LastRuntime} 运行结果({trigger.LastResult}) 下次运行时间: {trigger.NextRuntime} 耗时: {trigger.LastRunElapsedTime.TotalSeconds}";
            Messages.Add(new ConsoleMessageItem()
            {
                Message = message
            });
            if (Messages.Count > 20)
            {
                Messages.RemoveAt(0);
            }
            await InvokeAsync(StateHasChanged);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                var sche = TaskServicesManager.Get(Model.Name);
                if (sche != null)
                {
                    sche.Triggers.First().PulseCallback = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
