using Dapper;
using Npgsql;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    public class PostgreSqlSchemaService : IDbSchemaService
    {
        private readonly string _connString;

        public DatabaseType DbType => DatabaseType.PostgreSQL;
        public string ConnectionString => _connString;

        public PostgreSqlSchemaService(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("PostgreSQL Connection String is required");
            }
            _connString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connString);
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PostgreSQL Connection Error: {ex.Message}");
                throw new InvalidOperationException($"Cannot connect to PostgreSQL: {ex.Message}", ex);
            }
        }

        public string GetDatabaseName()
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(_connString);
                return builder.Database ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public IEnumerable<string> GetTables()
        {
            using var conn = new NpgsqlConnection(_connString);
            
            string sql = @"
                SELECT table_name
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_type = 'BASE TABLE'
                ORDER BY table_name";
            
            return conn.Query<string>(sql);
        }

        public IEnumerable<DbColumnInfo> GetSchema(string tableName)
        {
            using var conn = new NpgsqlConnection(_connString);

            tableName = tableName.Trim();

            // PostgreSQL is case-sensitive, typically uses lowercase
            tableName = tableName.ToLower();

            if (tableName.Contains("."))
            {
                tableName = tableName.Split('.').Last();
            }

            string sql = @"
                SELECT 
                    c.column_name as ColumnName,
                    c.data_type as DataType,
                    c.is_nullable as IsNullable,
                    c.character_maximum_length as MaxLength,
                    c.numeric_precision as NumericPrecision,
                    c.numeric_scale as NumericScale,
                    c.column_default as ColumnDefault,
                    CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as IsPrimaryKey,
                    CASE WHEN c.column_default LIKE 'nextval%' THEN true ELSE false END as IsIdentity
                FROM information_schema.columns c
                LEFT JOIN (
                    SELECT ku.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage ku
                        ON tc.constraint_name = ku.constraint_name
                        AND tc.table_schema = ku.table_schema
                    WHERE tc.constraint_type = 'PRIMARY KEY'
                      AND tc.table_name = @Table
                      AND tc.table_schema = 'public'
                ) pk ON c.column_name = pk.column_name
                WHERE c.table_name = @Table
                  AND c.table_schema = 'public'
                ORDER BY c.ordinal_position";

            var result = conn.Query<DbColumnInfo>(sql, new { Table = tableName });

            return result;
        }
    }
}
