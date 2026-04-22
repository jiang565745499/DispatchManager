using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using DispatchManager.Logging;
using Serilog;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Serilog.Sinks.SQLite;
using System.Data.Common;

namespace Microsoft.Extensions.DependencyInjection;
/// <summary>
/// Serilog 注入服务拓展类
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注入 Serilog 服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSerilogServices(this IServiceCollection services)
    {
        var baseDirectory = AppContext.BaseDirectory;

        // 加载配置文件
        var configuration = new ConfigurationBuilder()
            .SetBasePath(baseDirectory)
            .AddJsonFile("SerilogConf.json", optional: true, reloadOnChange: true)
            .Build();

        // 检查配置文件是否存在
        var configFile = Path.Combine(baseDirectory, "SerilogConf.json");
        // Console.WriteLine($"Serilog配置文件路径: {configFile}");
        // Console.WriteLine($"配置文件是否存在: {File.Exists(configFile)}");

        // 从配置中获取数据库路径
        string databasePath = Path.Combine(baseDirectory, "DB", "DispatchManagerLog.db");
        var connString = configuration.GetConnectionString("DMLogSqlite");
        if (!string.IsNullOrWhiteSpace(connString))
        {
            databasePath = ResolveSqliteDataSourcePath(connString, baseDirectory);
        }

        var dbDirectory = Path.GetDirectoryName(databasePath);

        // 确保数据库目录存在
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
            // Console.WriteLine($"创建数据库目录: {dbDirectory}");
        }

        var fileLogPath = Path.Combine(baseDirectory, "Log", "Serilog", "Logs_.txt");
        var fileLogDirectory = Path.GetDirectoryName(fileLogPath);
        if (!string.IsNullOrWhiteSpace(fileLogDirectory) && !Directory.Exists(fileLogDirectory))
        {
            Directory.CreateDirectory(fileLogDirectory);
        }

        // 配置Serilog - 使用代码方式配置SQLite sink以匹配我们的表结构
        Console.WriteLine($"SQLite数据库路径: {databasePath}");
        Console.WriteLine($"数据库文件是否存在: {System.IO.File.Exists(databasePath)}");

        // 启用Serilog的自诊断日志
        Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"Serilog SelfLog: {msg}"));



        // 配置 SQLite sink 选项，使用自定义列映射
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            // 过滤掉ASP.NET Core的默认日志
            .Filter.ByExcluding(logEvent =>
                logEvent.Properties.TryGetValue("SourceContext", out var sourceContext) &&
                sourceContext != null &&
                (
                    sourceContext.ToString().Contains("Microsoft") ||
                    sourceContext.ToString().Contains("System") ||
                    sourceContext.ToString().Contains("Longbow")
                )
            )
            .WriteTo.Console()
            .WriteTo.File(
                path: fileLogPath,
                rollingInterval: Serilog.RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8
            )
            .WriteTo.SQLite(
                sqliteDbPath: databasePath,
                tableName: "Log"
            );

        Log.Logger = loggerConfiguration.CreateLogger();
        Log.Logger.Information($"SQLite数据库路径: {databasePath}");

        // 测试日志
        Log.Logger.Information("Serilog配置成功，日志系统已初始化");

        services.AddLogging(config =>
        {
            config.AddSerilog(Log.Logger);
        });
        return services;
    }

    /// <summary>
        /// 注入 ILogRecorder 服务 - 统一日志记录接口
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddLogRecorder(this IServiceCollection services)
        {
            // 注册 SerilogLogger 作为 ILogRecorder 的实现
            services.AddSingleton<ILogRecorder, SerilogLogger>(provider =>
            {
                var logger = new SerilogLogger();
                // 初始化 LogHelperUtil 的静态实例
                LogHelperUtil.Initialize(logger);
                return logger;
            });
            return services;
        }

    private static string ResolveSqliteDataSourcePath(string connectionString, string baseDirectory)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        const string dataSourceKey = "Data Source";

        if (!builder.TryGetValue(dataSourceKey, out var dataSourceObj) || dataSourceObj == null)
        {
            return Path.Combine(baseDirectory, "DB", "DispatchManagerLog.db");
        }

        var dataSource = dataSourceObj.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(dataSource))
        {
            return Path.Combine(baseDirectory, "DB", "DispatchManagerLog.db");
        }

        return Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.GetFullPath(Path.Combine(baseDirectory, dataSource));
    }
}
