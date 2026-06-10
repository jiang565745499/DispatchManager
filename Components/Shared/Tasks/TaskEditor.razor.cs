using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Service;
using Longbow.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DispatchManager.Components.Shared.Tasks
{
    public partial class TaskEditor
    {
        //[Inject]
        //IFreeSql? freeSql { get; set; }

        [Inject]
        [NotNull]
        ToastService? toastService { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Parameter]
        [NotNull]
        public DispatchTask? Value { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Parameter]
        public EventCallback<DispatchTask> ValueChanged { get; set; }


        [NotNull]
        private List<SelectedItem>? Items { get; set; } = new List<SelectedItem>();

        [NotNull]
        private List<SelectedItem>? ItemCronSelect { get; set; } = new List<SelectedItem>();

        [NotNull]
        private List<SelectedItem>? ItemResponseTypeSelect { get; set; } = new List<SelectedItem>();

        private bool IsOpen { get; set; }

        private bool BindValue { get; set; } = false;

        /// <summary>
        /// 响应类型的字符串表示，用于Select组件绑定
        /// </summary>
        private string? ResponseTypeValue
        {
            get => ((int)Value.ResponseType).ToString();
            set
            {
                if (int.TryParse(value, out int intValue))
                {
                    Value.ResponseType = (ResponseType)intValue;
                }
            }
        }
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

            ItemResponseTypeSelect.Add(new SelectedItem(((int)ResponseType.Default).ToString(), "默认（JSON回调）"));
            ItemResponseTypeSelect.Add(new SelectedItem(((int)ResponseType.XML).ToString(), "XML（application/xml）"));
        }

        private Task OnCloseDrawer()
        {
            IsOpen = false;
            return Task.CompletedTask;
        }

        private Task OnCreateCron(DispatchTask model)
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



        private async Task OnFileUpload(UploadFile file)
        {
            if (file != null && file.OriginFileName!.EndsWith(".dll"))
            {
                int maxFileLen = 1000000;
                if (file.Size > maxFileLen)
                {
                    await toastService.Information("文件上传提示:", $"{file.File!.Name}超过上传文件最大限制1M!");
                }
                // 创建DLL目录（如果不存在）
                var dllDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DLL");
                if (!Directory.Exists(dllDirectory))
                {
                    Directory.CreateDirectory(dllDirectory);
                }

                // 保存文件
                var filePath = Path.Combine(dllDirectory, file.OriginFileName);
                //await SaveToFile(file);
                file.FileName = $"{Path.GetFileNameWithoutExtension(file.OriginFileName)}{Path.GetExtension(file.OriginFileName)}";
                var ret = await file.SaveToFileAsync(filePath, maxFileLen);// 最大只允许上传1M的文件
                if (ret)
                {
                    // 更新DLL路径
                    Value.DllPath = filePath;
                    // 触发值变更事件，确保父组件更新
                    await ValueChanged.InvokeAsync(Value);
                    // 强制UI更新
                    StateHasChanged();
                }
            }
        }

        /// <summary>
        /// 关闭编辑器
        /// </summary>
        private void Close()
        {
            // 触发值变更事件，通知父组件
            ValueChanged.InvokeAsync(Value);
        }

        /// <summary>
        /// 保存任务
        /// </summary>
        private void Save()
        {
            // 触发值变更事件，通知父组件
            ValueChanged.InvokeAsync(Value);
        }

    }
}
