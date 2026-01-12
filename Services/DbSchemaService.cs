using Dapper;
using Microsoft.Data.SqlClient;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    public class DbSchemaService
    {
        private readonly string _connString;

        public string ConnectionString => _connString;

        public DbSchemaService(IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(conn))
            {
                throw new InvalidOperationException("ไม่พบ Connection String! กรุณาตรวจสอบไฟล์ appsettings.json");
            }
            _connString = conn;
        }

        public bool TestConnection()
        {
            try
            {
                using var conn = new SqlConnection(_connString);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetDatabaseName()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(_connString);
                return builder.InitialCatalog;
            }
            catch
            {
                return "Unknown";
            }
        }

        public IEnumerable<DbColumnInfo> GetSchema(string tableName)
        {
            using var conn = new SqlConnection(_connString);

            tableName = tableName.Trim();

            if (tableName.Contains("."))
            {
                tableName = tableName.Split('.').Last();
            }

            string sql = @"
                SELECT 
                    c.COLUMN_NAME as ColumnName, 
                    c.DATA_TYPE as DataType, 
                    c.IS_NULLABLE as IsNullable,
                    c.CHARACTER_MAXIMUM_LENGTH as MaxLength,
                    c.NUMERIC_PRECISION as NumericPrecision,
                    c.NUMERIC_SCALE as NumericScale,
                    c.COLUMN_DEFAULT as ColumnDefault,
                    CASE 
                        WHEN pk.COLUMN_NAME IS NOT NULL THEN CAST(1 AS BIT)
                        ELSE CAST(0 AS BIT)
                    END as IsPrimaryKey,
                    CASE 
                        WHEN COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') = 1 
                        THEN CAST(1 AS BIT)
                        ELSE CAST(0 AS BIT)
                    END as IsIdentity
                FROM INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN (
                    SELECT ku.COLUMN_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                        ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                        AND tc.TABLE_NAME = ku.TABLE_NAME
                    WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
                        AND tc.TABLE_NAME = @Table
                ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
                WHERE c.TABLE_NAME = @Table
                ORDER BY c.ORDINAL_POSITION";

            // ใช้ Dapper Query
            var result = conn.Query<DbColumnInfo>(sql, new { Table = tableName });

            return result;
        }
    }
}