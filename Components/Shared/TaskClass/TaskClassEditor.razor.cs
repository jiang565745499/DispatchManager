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
            new(Color.Active.ToString(), "Active"),
            new(Color.Primary.ToString(), "Primary"),
            new(Color.Secondary.ToString(), "Secondary"),
            new(Color.Success.ToString(), "Success"),
            new(Color.Danger.ToString(), "Danger"),
            new(Color.Warning.ToString(), "Warning"),
            new(Color.Info.ToString(), "Info"),
            new(Color.Light.ToString(), "Light"),
            new(Color.Dark.ToString(), "Dark"),
            new(Color.Link.ToString(), "Link")
        };

        }

    }
}
