namespace AngularGenerator.Core.Models
{
    public class DbConnectionConfig
    {
        public string DbType { get; set; } = "SqlServer"; // SqlServer, MySQL, PostgreSQL
        public string ConnectionString { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int Port { get; set; }
        public bool UseWindowsAuth { get; set; } = true;
    }

    public class TestConnectionRequest
    {
        public string DbType { get; set; } = "SqlServer";
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class TestConnectionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? DatabaseName { get; set; }
    }
}
