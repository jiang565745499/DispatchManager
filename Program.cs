using BootstrapBlazor.Components;
using DispatchManager.Components;
using DispatchManager.Schedule.Service;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

if (OperatingSystem.IsWindows())
{
    builder.Host.UseWindowsService();
}

// Add services to the container.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddBootstrapBlazor();
// 增加 Table Excel 导出服务
builder.Services.AddBootstrapBlazorTableExportService();
// 增加 SignalR 服务数据传输大小限制配置
builder.Services.Configure<HubOptions>(option => option.MaximumReceiveMessageSize = null);

#region 依赖注入
// Serilog
builder.Services.AddSerilogServices();
// ILogRecorder 统一日志接口
builder.Services.AddLogRecorder();
// 任务调度
builder.Services.AddScheduleServices();
// FreeSql
builder.Services.AddFreeSqlDataAccessServices();
// 后台任务
builder.Services.AddHostedService<DispatchTaskService>();
// 金蝶对接任务
builder.Services.AddHostedService<DispatchTaskKingDeeService>();
#endregion
var app = builder.Build();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    app.UseResponseCompression();
//}

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
