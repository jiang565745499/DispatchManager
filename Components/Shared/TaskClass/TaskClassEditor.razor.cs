using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace DispatchManager.Components.Shared.TaskClass
{
    public partial class TaskClassEditor
    {
        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        [NotNull]
        public DispatchClass? Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        public EventCallback<DispatchClass> ValueChanged { get; set; }

        [NotNull]
        private List<SelectedItem>? Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            Items = new List<SelectedItem>
        {
            new(Color.None.ToString(), "无颜色"),
            new(Color.Primary.ToString(), "主色蓝"),
            new(Color.Secondary.ToString(), "灰色"),
            new(Color.Success.ToString(), "成功绿"),
            new(Color.Danger.ToString(), "危险红"),
            new(Color.Warning.ToString(), "警告黄"),
            new(Color.Info.ToString(), "信息蓝"),
            new(Color.Light.ToString(), "浅色"),
            new(Color.Dark.ToString(), "深色"),
            new(Color.Link.ToString(), "链接色")
        };

        }

    }
}
