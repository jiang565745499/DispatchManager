using BootstrapBlazor.Components;
using DispatchManager.DataAccess.FreeSql.Models;
using Microsoft.AspNetCore.Components.Routing;

namespace DispatchManager.Components.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class MainLayout
    {
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
                //new() { Text = "返回组件库", Icon = "fa-solid fa-fw fa-home", Url = "https://www.blazor.zone/components" },
                //new() { Text = "Index", Icon = "fa-solid fa-fw fa-flag", Url = "/" , Match = NavLinkMatch.All},
                new() { Text = "任务系统", Icon = "fa-solid fa-fw fa-home", Url = "/DispatchClass" , Match = NavLinkMatch.All},
                new() { Text = "任务调度", Icon = "fa-solid fa-fw fa-flag", Url = "/DispatchTasks" , Match = NavLinkMatch.All},
                new() { Text = "金蝶对接", Icon = "fa-solid fa-fw fa-table", Url = "/DispatchTasksKingDee" , Match = NavLinkMatch.All},
                new() { Text = "任务日志", Icon = "fa-solid fa-fw fa-database", Url = "/DispatchLog" , Match = NavLinkMatch.All},
                // new() { Text = "日志仪表板", Icon = "fa-solid fa-fw fa-chart-pie", Url = "/LogDashboard" , Match = NavLinkMatch.All},
                // new() { Text = "日志分析报告", Icon = "fa-solid fa-fw fa-file-alt", Url = "/LogAnalysisReport" , Match = NavLinkMatch.All},
                // new() { Text = "性能指标", Icon = "fa-solid fa-fw fa-chart-line", Url = "/PerformanceMetrics" , Match = NavLinkMatch.All},
                
            };

            return menus;
        }

        /// <summary>
        /// 切换主题
        /// </summary>
        private void ToggleTheme()
        {
            Theme = Theme == "dark" ? "" : "dark";
        }
    }
}
