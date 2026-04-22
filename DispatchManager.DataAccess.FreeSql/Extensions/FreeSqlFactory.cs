using FreeSql;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.DataAccess.FreeSql.Extensions
{
    public static class FreeSqlFactory
    {
        private static readonly ConcurrentDictionary<string, IFreeSql> _instances = new();

        public static IFreeSql GetInstance(string name)
        {
            return _instances.GetOrAdd(name, CreateInstance);
        }

        private static IFreeSql CreateInstance(string name)
        {
            var connectionString = GetConnectionString(name);
            var dataType = GetDataType(name);

            return new FreeSqlBuilder()
                .UseConnectionString(dataType, connectionString)
                .UseAutoSyncStructure(false) // 不建议自动同步结构
                .Build();
        }

        private static string GetConnectionString(string name)
        {
            // 从配置中获取连接字符串
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString(name);
            if (string.IsNullOrEmpty(connectionString)) {
                throw new Exception($"Connection string '{name}' not found in configuration.");
            }

            if (name is "DMSqlite" or "DMLogSqlite")
            {
                connectionString = NormalizeSqliteConnectionString(connectionString, basePath);
            }

            return connectionString;
        }

        private static DataType GetDataType(string name)
        {
            // 根据名称判断数据库类型
            return name switch
            {
                "SqlServer" => DataType.SqlServer,
                "MySql" => DataType.MySql,
                "Oracle" => DataType.Oracle,
                "DMSqlite" => DataType.Sqlite,
                "DMLogSqlite" => DataType.Sqlite,
                _ => throw new ArgumentException($"Unsupported database type: {name}")
            };
        }

        private static string NormalizeSqliteConnectionString(string connectionString, string baseDirectory)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
            const string dataSourceKey = "Data Source";

            if (!builder.TryGetValue(dataSourceKey, out var dataSourceObj) || dataSourceObj == null)
            {
                throw new Exception($"SQLite connection string missing '{dataSourceKey}'. ");
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
    }
}
