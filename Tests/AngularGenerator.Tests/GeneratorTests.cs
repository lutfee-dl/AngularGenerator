using Xunit;
using AngularGenerator.Services;
using AngularGenerator.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace AngularGenerator.Tests
{
    public class GeneratorTests
    {
        [Fact]
        public void Create_ShouldMapSqlIntToTsNumber()
        {
            // Arrange
            var factory = new AngularComponentFactory();
            var columns = new List<DbColumnInfo>
            {
                new DbColumnInfo { ColumnName = "UserId", DataType = "int", IsNullable = "NO", IsPrimaryKey = true }
            };

            // Act
            var result = factory.Create("User", columns);

            // Assert
            var field = result.Fields.First(f => f.FieldName == "UserId");
            Assert.Equal("number", field.TsType);
            Assert.Equal(ControlType.Number, field.UIControl);
        }

        [Fact]
        public void Create_ShouldMapSqlVarcharToTsString()
        {
            // Arrange
            var factory = new AngularComponentFactory();
            var columns = new List<DbColumnInfo>
            {
                new DbColumnInfo { ColumnName = "UserName", DataType = "varchar", IsNullable = "YES" }
            };

            // Act
            var result = factory.Create("User", columns);

            // Assert
            var field = result.Fields.First(f => f.FieldName == "UserName");
            Assert.Equal("string", field.TsType);
            Assert.Equal(ControlType.Text, field.UIControl);
        }

        [Fact]
        public void Create_ShouldIdentifyPrimaryKey()
        {
            // Arrange
            var factory = new AngularComponentFactory();
            var columns = new List<DbColumnInfo>
            {
                new DbColumnInfo { ColumnName = "ID", DataType = "int", IsNullable = "NO", IsPrimaryKey = true }
            };

            // Act
            var result = factory.Create("Employee", columns);

            // Assert
            Assert.Equal("ID", result.PrimaryKeyName);
            Assert.True(result.Fields.First(f => f.FieldName == "ID").IsPrimaryKey);
        }

        [Theory]
        [InlineData("date", "Date", ControlType.DatePicker)]
        [InlineData("datetime", "Date", ControlType.DatePicker)]
        [InlineData("timestamp", "Date", ControlType.DatePicker)]
        [InlineData("clob", "string", ControlType.TextArea)]
        public void Create_ShouldMapAs400TypesCorrectly(string sqlType, string expectedTsType, ControlType expectedControl)
        {
            // Arrange
            var factory = new AngularComponentFactory();
            var columns = new List<DbColumnInfo>
            {
                new DbColumnInfo { ColumnName = "TestCol", DataType = sqlType, IsNullable = "YES" }
            };

            // Act
            var result = factory.Create("TestTable", columns);

            // Assert
            var field = result.Fields.First(f => f.FieldName == "TestCol");
            Assert.Equal(expectedTsType, field.TsType);
            Assert.Equal(expectedControl, field.UIControl);
        }
    }
}
