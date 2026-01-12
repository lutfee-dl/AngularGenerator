using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AngularGenerator.Core.Models
{
    /// <summary>
    /// Model representing a database column schema
    /// </summary>
    public class DbColumnInfo
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string IsNullable { get; set; } = "NO";
        public int? MaxLength { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public string? ColumnDefault { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
    }
}
