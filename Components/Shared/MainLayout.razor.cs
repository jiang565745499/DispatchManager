using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace DispatchManager.Components.Shared
{
    /// <summary>
    ///
    /// </summary>
    public sealed partial class MainLayout
    {
        [Inject]
        [System.Diagnostics.CodeAnalysis.NotNull]
        private IJSRuntime? JSRuntime { get; set; }

        private bool UseTabSet { get; set; } = true;

        private string Theme { get; set; } = ""; // "dark" for dark theme

        private bool IsDarkMode => Theme == "dark";

        private bool IsOpen { get; set; }

        private bool IsFixedHeader { get; set; } = true;

        private bool IsFixedFooter { get; set; } = true;

        private bool IsFullSide { get; set; } = true;

        private bool ShowFooter { get; set; } = true;

        private List<MenuItem>? Menus { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            Menus = GetIconSideMenuItems();
        }

        private static List<MenuItem> GetIconSideMenuItems()
        {
            var menus = new List<MenuItem>
            {
                new() { Text = "主页", Icon = "fa-solid fa-fw fa-home", Url = "/" , Match = NavLinkMatch.All},
                new() { Text = "任务系统", Icon = "fa-solid fa-fw fa-cogs", Url = "/DispatchClass" , Match = NavLinkMatch.All},
                new() { Text = "任务调度", Icon = "fa-solid fa-fw fa-flag", Url = "/DispatchTasks" , Match = NavLinkMatch.All},
                new() { Text = "金蝶对接", Icon = "fa-solid fa-fw fa-table", Url = "/DispatchTasksKingDee" , Match = NavLinkMatch.All},
                new() { Text = "任务日志", Icon = "fa-solid fa-fw fa-database", Url = "/DispatchLog" , Match = NavLinkMatch.All},
            };

            return menus;
        }

        /// <summary>
        /// 切换主题
        /// </summary>
        private async void ToggleTheme()
        {
            Theme = Theme == "dark" ? "" : "dark";
            await JSRuntime.InvokeVoidAsync("setTheme", Theme == "dark" ? "dark" : "light");
        }
    }
}
