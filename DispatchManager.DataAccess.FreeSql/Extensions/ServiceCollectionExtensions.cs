// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the LGPL License, Version 3.0. See License.txt in the project root for license information.
// Website: https://admin.blazor.zone



using BootstrapBlazor.Components;
using BootstrapAdmin.DataAccess.FreeSql.Service;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FreeSql.DependencyInjection.Extensions;
using Longbow.Tasks;
using DispatchManager.DataAccess.FreeSql.InterFace;
using DispatchManager.DataAccess.FreeSql.Service;
using DispatchManager.DataAccess.FreeSql.Models;
using DispatchManager.DataAccess.FreeSql.Extensions;
using System.Data.Common;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// FreeSql ORM 注入服务扩展类
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注入 FreeSql 数据服务类
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFreeSqlDataAccessServices(this IServiceCollection services)
    {
        // 增加缓存服务
        services.AddCacheManager();

        // 注册主数据库
        services.AddKeyedSingleton<IFreeSql>("MainDB", (provider, key) =>
        {
            var builder = new FreeSqlBuilder();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connString = configuration.GetConnectionString("DMSqlite");
            connString = NormalizeSqliteConnectionString(connString, AppContext.BaseDirectory);

            builder.UseConnectionString(DataType.Sqlite, connString);
            if (Convert.ToBoolean(configuration.GetValue<bool>("AutoGenerate")))
            {
                builder.UseAutoSyncStructure(true);// 自动同步实体结构

            }
#if DEBUG
            //调试 sql 语句输出
            // builder.UseMonitorCommand(cmd => System.Console.WriteLine(cmd.CommandText));
#endif

            var instance = builder.Build();
            instance.Mapper();
            return instance;
            //FreeSqlFactory.GetInstance("DMSqlite");
        }
        );


        // 注册日志数据库
        services.AddKeyedSingleton<IFreeSql>("LogDB", (provider, key) =>
            {
                var builder = new FreeSqlBuilder();
                var configuration = provider.GetRequiredService<IConfiguration>();

                // 优先读取配置，但最终强制日志库文件名为 DispatchManagerLog.db，防止误指向主库
                var configuredConnString = configuration.GetConnectionString("DMLogSqlite");
                var connString = BuildLogDbConnectionString(configuredConnString, AppContext.BaseDirectory);

                System.Console.WriteLine($"[LogDB] 连接字符串: {connString}");
                builder.UseConnectionString(DataType.Sqlite, connString);
                if (Convert.ToBoolean(configuration.GetValue<bool>("AutoGenerate")))
                {
                    builder.UseAutoSyncStructure(true);// 自动同步实体结构
                }
                builder.UseMonitorCommand(cmd => System.Console.WriteLine($"[LogDB SQL] {cmd.CommandText}"));

                var instance = builder.Build();
                instance.Mapper();

                // 始终确保 Log 表的索引存在（不依赖 AutoGenerate 开关）
                instance.CodeFirst.SyncStructure<Log>();

                return instance;
            }
        ); // 默认使用DMSqlite

        // 主业务数据库
        //services.TryAddSingleton(provider =>
        //{
        //    var builder = new FreeSqlBuilder();
        //    var configuration = provider.GetRequiredService<IConfiguration>();
        //    var connString = configuration.GetConnectionString("DMSqlite");
        //    builder.UseConnectionString(DataType.Sqlite, connString);
        //    ConfigurationManager configurationManager = new();
        //    if (Convert.ToBoolean(configurationManager.GetValue<bool>("AutoGenerate")))
        //    {
        //        builder.UseAutoSyncStructure(true);// 自动同步实体结构

        //    }
        //    #if DEBUG
        //    //调试 sql 语句输出
        //    builder.UseMonitorCommand(cmd => System.Console.WriteLine(cmd.CommandText));
        //    #endif

        //    var instance = builder.Build();
        //    instance.Mapper();
        //    return instance;
        //});

        // 日志数据库
        //services.TryAddTransient(provider =>
        //{
        //    var builder = new FreeSqlBuilder();
        //    var configuration = provider.GetRequiredService<IConfiguration>();
        //    var connString = configuration.GetConnectionString("DMLogSqlite");
        //    builder.UseConnectionString(DataType.Sqlite, connString);
        //        builder.UseAutoSyncStructure(true);// 自动同步实体结构
        //    #if DEBUG
        //    //调试 sql 语句输出
        //    builder.UseMonitorCommand(cmd => System.Console.WriteLine(cmd.CommandText));
        //    #endif

        //    var instance = builder.Build();
        //    instance.Mapper();
        //    // 同步Log表结构
        //    instance.CodeFirst.SyncStructure<Log>();
        //    return instance;
        //});

        // 增加数据服务
        services.AddSingleton(typeof(IDataService<>), typeof(DefaultDataService<>));

        // 增加业务服务
        //services.AddSingleton<IScheduleTask, ScheduleTaskService>();
        //services.AddSingleton<IDispatchClass, DispatchClassService>();
        //services.AddSingleton<ISqliteLog, SqliteLogService>();

        services.AddSingleton<ScheduleTaskService>();
        services.AddSingleton<ScheduleTaskKingDeeService>();
        services.AddSingleton<DispatchClassService>();
        services.AddSingleton<SqliteLogService>();

        return services;
    }

    private static string NormalizeSqliteConnectionString(string? connectionString, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("SQLite connection string is empty.");
        }

        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        const string dataSourceKey = "Data Source";

        if (!builder.TryGetValue(dataSourceKey, out var dataSourceObj) || dataSourceObj == null)
        {
            throw new Exception($"SQLite connection string missing '{dataSourceKey}'.");
        }

        var dataSource = dataSourceObj.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(dataSource))
        {
            throw new Exception("SQLite Data Source is empty.");
        }

        if (!Path.IsPathRooted(dataSource))
        {
            dataSource = Path.GetFullPath(Path.Combine(baseDirectory, dataSource));
        }

        var dbDirectory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        builder[dataSourceKey] = dataSource;
        return builder.ConnectionString;
    }

    private static string BuildLogDbConnectionString(string? configuredConnectionString, string baseDirectory)
    {
        var logDbPath = Path.Combine(baseDirectory, "DB", "DispatchManagerLog.db");

        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = configuredConnectionString };
            const string dataSourceKey = "Data Source";
            if (builder.TryGetValue(dataSourceKey, out var dataSourceObj) && dataSourceObj != null)
            {
                var dataSource = dataSourceObj.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(dataSource))
                {
                    var absPath = Path.IsPathRooted(dataSource)
                        ? dataSource
                        : Path.GetFullPath(Path.Combine(baseDirectory, dataSource));

                    // 目录沿用配置，但文件名强制为 DispatchManagerLog.db
                    var directory = Path.GetDirectoryName(absPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        logDbPath = Path.Combine(directory, "DispatchManagerLog.db");
                    }
                }
            }
        }

        var logDbDirectory = Path.GetDirectoryName(logDbPath);
        if (!string.IsNullOrWhiteSpace(logDbDirectory) && !Directory.Exists(logDbDirectory))
        {
            Directory.CreateDirectory(logDbDirectory);
        }

        var logBuilder = new DbConnectionStringBuilder();
        logBuilder["Data Source"] = logDbPath;
        return logBuilder.ConnectionString;
    }
}
