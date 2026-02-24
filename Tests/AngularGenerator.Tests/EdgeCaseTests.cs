using Xunit;
using AngularGenerator.Services;
using AngularGenerator.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace AngularGenerator.Tests
{
    public class EdgeCaseTests : IDisposable
    {
        private readonly string _testConfigPath;

        public EdgeCaseTests()
        {
            // Create a temporary file for config tests
            _testConfigPath = Path.Combine(Path.GetTempPath(), $"db-configs-test-{Guid.NewGuid()}.json");
        }

        public void Dispose()
        {
            // Clean up temporary file
            if (File.Exists(_testConfigPath))
            {
                File.Delete(_testConfigPath);
            }
        }

        [Fact]
        public void Create_ShouldHandleThaiTableName()
        {
            // Arrange
            var factory = new AngularComponentFactory();
            var columns = new List<DbColumnInfo> 
            { 
                new DbColumnInfo { ColumnName = "ชื่อ", DataType = "varchar", IsNullable = "NO", IsPrimaryKey = true } 
            };

            // Act
            var result = factory.Create("สินค้า", columns);

            // Assert
            Assert.Equal("สินค้า", result.EntityName);
            Assert.Equal("app-สินค้า", result.Selector);
            Assert.Equal("ชื่อ", result.Fields.First().FieldName);
        }

        [Fact]
        public void SaveConfiguration_ShouldUpdateExisting_WhenNameMatches()
        {
            // Arrange
            var service = new DatabaseConfigService(_testConfigPath);
            var config1 = new DatabaseConfig 
            { 
                Name = "LocalDB", 
                DbType = "SqlServer", 
                ConnectionString = "Server=initial;Database=db1;" 
            };
            
            // Act
            service.SaveConfiguration(config1);
            
            var config2 = new DatabaseConfig 
            { 
                Name = "LocalDB", 
                DbType = "SqlServer", 
                ConnectionString = "Server=updated;Database=db2;" 
            };
            service.SaveConfiguration(config2);

            // Assert
            var all = service.GetAllConfigurations();
            Assert.Single(all); // Should still only have 1 config
            Assert.Equal("Server=updated;Database=db2;", all.First().ConnectionString);
        }

        [Fact]
        public void SaveConfiguration_ShouldManageDefaultStatus()
        {
            // Arrange
            var service = new DatabaseConfigService(_testConfigPath);
            var config1 = new DatabaseConfig { Name = "DB1", IsDefault = true };
            var config2 = new DatabaseConfig { Name = "DB2", IsDefault = false };
            
            // Act
            service.SaveConfiguration(config1);
            service.SaveConfiguration(config2);
            
            var conf1 = service.GetConfiguration("DB1");
            var conf2 = service.GetConfiguration("DB2");
            Assert.NotNull(conf1);
            Assert.NotNull(conf2);
            Assert.True(conf1.IsDefault);
            Assert.False(conf2.IsDefault);
            
            // Make DB2 the default
            config2.IsDefault = true;
            service.SaveConfiguration(config2);

            // Assert
            var updated1 = service.GetConfiguration("DB1");
            var updated2 = service.GetConfiguration("DB2");
            Assert.NotNull(updated1);
            Assert.NotNull(updated2);
            Assert.False(updated1.IsDefault);
            Assert.True(updated2.IsDefault);
        }
    }
}
