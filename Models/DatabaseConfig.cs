namespace AngularGenerator.Core.Models
{
    /// <summary>
    /// Model for managing multiple database configurations
    /// </summary>
    public class DatabaseConfig
    {
        public string Name { get; set; } = string.Empty;
        public string DbType { get; set; } = "SqlServer"; // SqlServer, MySQL, PostgreSQL, AS400
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public DateTime LastUsed { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
    }

    public class DatabaseConfigCollection
    {
        public List<DatabaseConfig> Configurations { get; set; } = new List<DatabaseConfig>();
        public string? DefaultConfigName { get; set; }
    }

    public class SaveConfigRequest
    {
        public string Name { get; set; } = string.Empty;
        public string DbType { get; set; } = "SqlServer";
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public string Description { get; set; } = string.Empty;
    }

    public class DeleteConfigRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
