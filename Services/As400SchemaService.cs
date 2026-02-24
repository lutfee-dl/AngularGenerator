using System.Data;
using System.Data.OleDb;
using System.Runtime.Versioning;
using Dapper;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    [SupportedOSPlatform("windows")]
    public class As400SchemaService : IDbSchemaService
    {
        private readonly string _connString;

        public DatabaseType DbType => DatabaseType.AS400;
        public string ConnectionString => _connString;

        public As400SchemaService(string connectionString)
        {
            _connString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                // Note: Requires System.Data.OleDb package and IBM i Access Client Solutions (ACS) OLE DB Provider installed
                using var conn = new OleDbConnection(_connString);
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AS400 Connection Error: {ex.Message}");
                throw;
            }
        }

        public string GetDatabaseName()
        {
            try
            {
                var builder = new OleDbConnectionStringBuilder(_connString);
                
                // AS400/IBM i ใช้ Library (Schema) แทน Database
                // ลองดึงข้อมูลตามลำดับความสำคัญ
                if (builder.ContainsKey("Default Collection"))
                {
                    var library = builder["Default Collection"].ToString();
                    if (!string.IsNullOrEmpty(library))
                        return $"{library} (Library)";
                }
                
                if (builder.ContainsKey("Data Source"))
                {
                    var dataSource = builder["Data Source"].ToString();
                    if (!string.IsNullOrEmpty(dataSource))
                        return $"{dataSource} (System)";
                }
                
                return "AS400/IBM i";
            }
            catch
            {
                return "AS400/IBM i";
            }
        }

        public IEnumerable<string> GetTables()
        {
            using var conn = new OleDbConnection(_connString);
            
            // Query tables using SYSIBM.SQLTABLES
            // AS400/IBM i ใช้ Library.Table format (Library คือ Schema)
            string sql = @"
                SELECT TABLE_SCHEM, TABLE_NAME 
                FROM SYSIBM.SQLTABLES 
                WHERE TABLE_TYPE = 'TABLE'
                ORDER BY TABLE_SCHEM, TABLE_NAME
            ";
            
            try
            {
                var result = conn.Query(sql);
                // Return in format: LIBRARY.TABLE (เช่น MYLIB.CUSTOMER)
                return result.Select(r => $"{r.TABLE_SCHEM}.{r.TABLE_NAME}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AS400 Error getting tables: {ex.Message}");
                // Fallback: If SQLTABLES fails, return empty list
                return new List<string>();
            }
        }

        public IEnumerable<DbColumnInfo> GetSchema(string tableName)
        {
            try
            {
                using var conn = new OleDbConnection(_connString);
                conn.Open();

                string schema = "";
                string table = tableName;

                // AS400/IBM i ต้องระบุทั้ง TABLE_SCHEM และ TABLE_NAME เสมอ
                // รูปแบบ: LIBRARY.TABLE (เช่น MYLIB.CUSTOMER)
                if (tableName.Contains("."))
                {
                    var parts = tableName.Split(new[] { '.' }, 2);
                    schema = parts[0].Trim().ToUpper(); // AS400 ใช้ตัวพิมพ์ใหญ่
                    table = parts[1].Trim().ToUpper();
                }
                else
                {
                    // AS400 ต้องระบุ Library.Table เสมอ ไม่งั้นจะไม่เจอ Fields
                    throw new ArgumentException(
                        $"AS400 requires LIBRARY.TABLE format. Got: '{tableName}'. " +
                        "Example: TESTLIB.CUSMAS", 
                        nameof(tableName));
                }

                // Validate
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(table))
                {
                    throw new ArgumentException(
                        $"AS400 requires both Library and Table name. Got: Schema='{schema}', Table='{table}'", 
                        nameof(tableName));
                }

                Console.WriteLine($"AS400 GetSchema: Schema='{schema}', Table='{table}'");

                // First, get primary key columns
                var pkColumns = new HashSet<string>();
                try
                {
                    string pkSql = @"
                        SELECT COLUMN_NAME 
                        FROM SYSIBM.SQLPRIMARYKEYS 
                        WHERE TABLE_NAME = ?";
                    
                    if (!string.IsNullOrEmpty(schema))
                    {
                        pkSql += " AND TABLE_SCHEM = ?";
                        var pkData = conn.Query(pkSql, new object[] { table, schema });
                        foreach (var pk in pkData)
                        {
                            pkColumns.Add(pk.COLUMN_NAME?.ToString()?.Trim() ?? "");
                        }
                    }
                    else
                    {
                        var pkData = conn.Query(pkSql, new object[] { table });
                        foreach (var pk in pkData)
                        {
                            pkColumns.Add(pk.COLUMN_NAME?.ToString()?.Trim() ?? "");
                        }
                    }
                    
                    Console.WriteLine($"AS400 Found {pkColumns.Count} primary key columns");
                }
                catch (Exception pkEx)
                {
                    Console.WriteLine($"AS400 PK Query Error: {pkEx.Message}");
                }

                // Get column information - Using user's exact query format
                // AS400/IBM i Query: SELECT table_name, COLUMN_Name, COLUMN_Text, COLUMN_Size, DECIMAL_DIGITS, Type_Name
                // FROM sysibm.SQLcolumns WHERE table_schem='xxxxx' AND table_name = 'xxxxx'
                
                string sql = @"
                    SELECT 
                        TABLE_NAME,
                        COLUMN_NAME, 
                        COLUMN_TEXT,
                        COLUMN_SIZE,
                        DECIMAL_DIGITS,
                        TYPE_NAME,
                        NULLABLE,
                        ORDINAL_POSITION
                    FROM SYSIBM.SQLCOLUMNS 
                    WHERE TABLE_NAME = ?";

                if (!string.IsNullOrEmpty(schema))
                {
                    sql += " AND TABLE_SCHEM = ?";
                }

                sql += " ORDER BY ORDINAL_POSITION";

                Console.WriteLine($"AS400 Executing SQL: {sql}");
                
                IEnumerable<dynamic> columns;
                if (!string.IsNullOrEmpty(schema))
                {
                    columns = conn.Query(sql, new { TableName = table, SchemaName = schema });
                }
                else
                {
                    columns = conn.Query(sql, new { TableName = table });
                }

                var columnsList = columns.ToList();
                
                Console.WriteLine($"AS400 Found {columnsList.Count} columns");
                
                if (columnsList.Count > 0)
                {
                    Console.WriteLine("AS400 First column details:");
                    var firstCol = columnsList.First();
                    foreach (var prop in ((IDictionary<string, object>)firstCol))
                    {
                        Console.WriteLine($"  {prop.Key} = {prop.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("AS400 No columns found, trying without schema filter...");
                    
                    string sqlNoSchema = @"
                        SELECT 
                            TABLE_NAME,
                            COLUMN_NAME, 
                            COLUMN_TEXT,
                            COLUMN_SIZE,
                            DECIMAL_DIGITS,
                            TYPE_NAME,
                            NULLABLE,
                            ORDINAL_POSITION
                        FROM SYSIBM.SQLCOLUMNS 
                        WHERE TABLE_NAME = ?
                        ORDER BY ORDINAL_POSITION";
                    
                    var secondAttemptColumns = conn.Query(sqlNoSchema, new { TableName = table }).ToList();
                    Console.WriteLine($"AS400 Second attempt found {secondAttemptColumns.Count} columns");
                    
                    if (secondAttemptColumns.Count > 0)
                    {
                        // ดึง schema จาก result
                        var firstCol = secondAttemptColumns.First();
                        columnsList = secondAttemptColumns; // อัปเดต columnsList เพื่อนำไปใช้งานต่อ
                        var foundTableName = firstCol.TABLE_NAME?.ToString();
                        Console.WriteLine($"AS400 Found table: {foundTableName}");
                    }
                }

                if (columnsList.Count == 0)
                {
                    // Try alternative query using OleDbConnection GetSchema
                    Console.WriteLine("AS400 Trying alternative method: GetSchema");
                    var dt = conn.GetSchema("Columns", new[] { null, schema, table, null });
                    
                    var result2 = new List<DbColumnInfo>();
                    foreach (DataRow row in dt.Rows)
                    {
                        var colName = row["COLUMN_NAME"]?.ToString()?.Trim() ?? "";
                        result2.Add(new DbColumnInfo
                        {
                            ColumnName = colName,
                            DataType = MapAS400Type(row["DATA_TYPE"]?.ToString() ?? ""),
                            MaxLength = row["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]) : (int?)null,
                            NumericScale = row["NUMERIC_SCALE"] != DBNull.Value ? Convert.ToInt32(row["NUMERIC_SCALE"]) : (int?)null,
                            ColumnText = colName, // AS400 may not have COLUMN_TEXT in GetSchema
                            IsNullable = row["IS_NULLABLE"]?.ToString() == "YES" ? "YES" : "NO",
                            IsPrimaryKey = pkColumns.Contains(colName)
                        });
                    }
                    
                    Console.WriteLine($"AS400 Alternative method found {result2.Count} columns");
                    return result2;
                }

                var result = new List<DbColumnInfo>();
                foreach (var col in columnsList)
                {
                    var colName = col.COLUMN_NAME?.ToString()?.Trim() ?? "";
                    var dataType = col.TYPE_NAME?.ToString()?.Trim() ?? "VARCHAR";
                    var columnText = col.COLUMN_TEXT?.ToString()?.Trim(); // AS400 field description
                    
                    result.Add(new DbColumnInfo
                    {
                        ColumnName = colName,
                        DataType = MapAS400Type(dataType),
                        MaxLength = col.COLUMN_SIZE != null ? (int?)Convert.ToInt32(col.COLUMN_SIZE) : null,
                        NumericScale = col.DECIMAL_DIGITS != null ? (int?)Convert.ToInt32(col.DECIMAL_DIGITS) : null,
                        ColumnText = !string.IsNullOrWhiteSpace(columnText) ? columnText : colName, // Use COLUMN_TEXT if available
                        IsNullable = col.NULLABLE?.ToString() == "1" || col.NULLABLE?.ToString()?.ToUpper() == "YES" ? "YES" : "NO",
                        IsPrimaryKey = pkColumns.Contains(colName)
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AS400 GetSchema Error: {ex.Message}");
                Console.WriteLine($"AS400 StackTrace: {ex.StackTrace}");
                throw new Exception($"Failed to get schema for table '{tableName}': {ex.Message}", ex);
            }
        }

        internal string MapAS400Type(string as400Type)
        {
            // Map AS400/DB2 types to standard SQL types
            var type = as400Type?.ToUpper().Trim() ?? "";
            
            return type switch
            {
                "INTEGER" or "INT" or "SMALLINT" or "BIGINT" => "INT",
                "DECIMAL" or "NUMERIC" or "DEC" => "DECIMAL",
                "REAL" or "FLOAT" or "DOUBLE" => "FLOAT",
                "CHAR" or "CHARACTER" => "CHAR",
                "VARCHAR" or "CHARACTER VARYING" => "VARCHAR",
                "DATE" => "DATE",
                "TIME" => "TIME",
                "TIMESTAMP" => "DATETIME",
                "BLOB" => "BLOB",
                "CLOB" => "TEXT",
                _ => type.Length > 0 ? type : "VARCHAR"
            };
        }
    }
}
