using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Service;
using Longbow.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;

namespace DispatchManager.Components.Shared.TasksKingDee
{
    public partial class TaskKingDeeEditor
    {
        //[Inject]
        //IFreeSql? freeSql { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        [NotNull]
        public DispatchTaskKingDee? Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        public EventCallback<DispatchTaskKingDee> ValueChanged { get; set; }


        [NotNull]
        private List<SelectedItem>? Items { get; set; } = new List<SelectedItem>();

        [NotNull]
        private List<SelectedItem>? ItemCronSelect { get; set; } = new List<SelectedItem>();

        private bool IsOpen { get; set; }

        private bool BindValue { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            foreach (var item in MainDb.GetAllDispatchClass())
            {
                Items.Add(new SelectedItem(item.ID!.ToString(), item.ClassName));
            }

            if (string.IsNullOrEmpty(Value.Trigger))
            {
                Value.Trigger = Items.First().Value;
            }

            ItemCronSelect.Add(new SelectedItem(1.ToString(), "秒"));
            ItemCronSelect.Add(new SelectedItem(2.ToString(), "分钟"));
            ItemCronSelect.Add(new SelectedItem(3.ToString(), "小时"));
        }

        private Task OnCloseDrawer()
        {
            IsOpen = false;
            return Task.CompletedTask;
        }

        private Task OnCreateCron(DispatchTaskKingDee model)
        {
            IsOpen = false;
            if (model != null)
            {
                if (model.TimeType == 1)
                {
                    model.Trigger = Cron.Secondly(model.TimeNumber);
                }
                else if (model.TimeType == 2)
                {
                    model.Trigger = Cron.Minutely(model.TimeNumber);
                }
                else if (model.TimeType == 3)
                {
                    model.Trigger = Cron.Hourly(model.TimeNumber);
                }
            }
            return Task.CompletedTask;
        }

        private void OnValueChanged(DispatchTaskKingDee model)
        {
            model.IsLog = !model.IsLog;
        }

    }
}
