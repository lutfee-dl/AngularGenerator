using Dapper;
using MySqlConnector;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    public class MySqlSchemaService : IDbSchemaService
    {
        private readonly string _connString;

        public DatabaseType DbType => DatabaseType.MySQL;
        public string ConnectionString => _connString;

        public MySqlSchemaService(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MySQL Connection String is required");
            }
            _connString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                using var conn = new MySqlConnection(_connString);
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MySQL Connection Error: {ex.Message}");
                throw new InvalidOperationException($"Cannot connect to MySQL: {ex.Message}", ex);
            }
        }

        public string GetDatabaseName()
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder(_connString);
                return builder.Database;
            }
            catch
            {
                return "Unknown";
            }
        }

        public IEnumerable<string> GetTables()
        {
            using var conn = new MySqlConnection(_connString);
            
            string sql = @"
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_TYPE = 'BASE TABLE'
                ORDER BY TABLE_NAME";
            
            return conn.Query<string>(sql);
        }

        public IEnumerable<DbColumnInfo> GetSchema(string tableName)
        {
            using var conn = new MySqlConnection(_connString);

            tableName = tableName.Trim();

            if (tableName.Contains("."))
            {
                tableName = tableName.Split('.').Last();
            }

            string sql = @"
                SELECT 
                    COLUMN_NAME as ColumnName,
                    DATA_TYPE as DataType,
                    IS_NULLABLE as IsNullable,
                    CHARACTER_MAXIMUM_LENGTH as MaxLength,
                    NUMERIC_PRECISION as NumericPrecision,
                    NUMERIC_SCALE as NumericScale,
                    COLUMN_DEFAULT as ColumnDefault,
                    CASE WHEN COLUMN_KEY = 'PRI' THEN 1 ELSE 0 END as IsPrimaryKey,
                    CASE WHEN EXTRA = 'auto_increment' THEN 1 ELSE 0 END as IsIdentity
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = @Table
                ORDER BY ORDINAL_POSITION";

            var result = conn.Query<DbColumnInfo>(sql, new { Table = tableName });

            return result;
        }
    }
}
