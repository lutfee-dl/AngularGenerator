using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    public enum DatabaseType
    {
        SqlServer,
        MySQL,
        PostgreSQL,
        AS400
    }

    public interface IDbSchemaService
    {
        DatabaseType DbType { get; }
        string ConnectionString { get; }
        bool TestConnection();
        string GetDatabaseName();
        IEnumerable<string> GetTables();
        IEnumerable<DbColumnInfo> GetSchema(string tableName);
    }
}
