using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    /// <summary>
    /// Factory สำหรับสร้าง IDbSchemaService ตาม Database Type
    /// </summary>
    public class DbSchemaServiceFactory
    {
        private readonly IConfiguration _config;
        private IDbSchemaService? _currentService;

        public DbSchemaServiceFactory(IConfiguration config)
        {
            _config = config;
            // สร้าง default service (SQL Server)
            _currentService = CreateDefaultService();
        }

        public IDbSchemaService GetCurrentService()
        {
            return _currentService ?? CreateDefaultService();
        }

        public IDbSchemaService CreateService(DatabaseType dbType, string connectionString)
        {
            IDbSchemaService service = dbType switch
            {
                DatabaseType.SqlServer => new SqlServerSchemaService(connectionString),
                DatabaseType.MySQL => new MySqlSchemaService(connectionString),
                DatabaseType.PostgreSQL => new PostgreSqlSchemaService(connectionString),
                DatabaseType.AS400 => new As400SchemaService(connectionString),
                _ => throw new NotSupportedException($"Database type {dbType} is not supported")
            };

            // เก็บ service ปัจจุบัน
            _currentService = service;
            return service;
        }

        /// <summary>
        /// สร้าง service เพื่อทดสอบการเชื่อมต่อเท่านั้น โดยไม่เปลี่ยน _currentService
        /// </summary>
        public IDbSchemaService TestConnectionOnly(DatabaseType dbType, string connectionString)
        {
            IDbSchemaService service = dbType switch
            {
                DatabaseType.SqlServer => new SqlServerSchemaService(connectionString),
                DatabaseType.MySQL => new MySqlSchemaService(connectionString),
                DatabaseType.PostgreSQL => new PostgreSqlSchemaService(connectionString),
                DatabaseType.AS400 => new As400SchemaService(connectionString),
                _ => throw new NotSupportedException($"Database type {dbType} is not supported")
            };

            // ไม่เซ็ต _currentService เพื่อไม่ให้เปลี่ยน database
            return service;
        }

        /// <summary>
        /// สร้าง service เพื่อทดสอบการเชื่อมต่อเท่านั้น โดยไม่เปลี่ยน _currentService
        /// </summary>
        public IDbSchemaService TestConnectionOnly(string dbTypeString, string connectionString)
        {
            var dbType = Enum.Parse<DatabaseType>(dbTypeString, ignoreCase: true);
            return TestConnectionOnly(dbType, connectionString);
        }

        public IDbSchemaService CreateService(string dbTypeString, string connectionString)
        {
            var dbType = Enum.Parse<DatabaseType>(dbTypeString, ignoreCase: true);
            return CreateService(dbType, connectionString);
        }

        private IDbSchemaService CreateDefaultService()
        {
            var connString = _config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("DefaultConnection not found in appsettings.json");
            }
            return new SqlServerSchemaService(connString);
        }
    }

    // Extension สำหรับ SQL Server service
    public static class DbSchemaServiceExtensions
    {
        public static SqlServerSchemaService AsSqlServer(this IDbSchemaService service)
        {
            return (SqlServerSchemaService)service;
        }
    }
}
