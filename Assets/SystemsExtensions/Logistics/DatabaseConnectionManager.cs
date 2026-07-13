using System.IO;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    public static class DatabaseConnectionManager
    {
        public static string DefaultDbPath => Path.Combine(Application.persistentDataPath, "ascendant_world.db");

        public static SqliteConnection CreateConnection(string connectionStringOrPath)
        {
            string connStr;
            if (connectionStringOrPath.StartsWith("URI=") || connectionStringOrPath.Contains(";"))
            {
                connStr = connectionStringOrPath;
            }
            else
            {
                // Ensure parent directory exists if a absolute path is provided
                if (Path.IsPathRooted(connectionStringOrPath))
                {
                    var dir = Path.GetDirectoryName(connectionStringOrPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                connStr = $"URI=file:{connectionStringOrPath}";
            }

            var conn = new SqliteConnection(connStr);
            conn.Open();
            return conn;
        }
    }
}
