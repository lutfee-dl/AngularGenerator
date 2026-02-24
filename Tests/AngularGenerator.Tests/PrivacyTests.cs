using Xunit;
using AngularGenerator.Services;

namespace AngularGenerator.Tests
{
    public class PrivacyTests
    {
        [Theory]
        [InlineData("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=****;")]
        [InlineData("Host=localhost;Port=5432;Database=test;Username=admin;pwd=secret_pass;", "Host=localhost;Port=5432;Database=test;Username=admin;pwd=****;")]
        [InlineData("DataSource=MYSERVER;Default Collection=MYLIB;User ID=USER;Password=MYPASS", "DataSource=MYSERVER;Default Collection=MYLIB;User ID=USER;Password=****")]
        [InlineData("NoPasswordHere=123;Something=Else;", "NoPasswordHere=123;Something=Else;")]
        public void MaskPassword_ShouldHideSensitiveData(string input, string expected)
        {
            // Act
            string result = DatabaseConfigService.MaskPassword(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }

    public class As400TypeTests
    {
        [Theory]
        [InlineData("CHAR", "CHAR")]
        [InlineData("CHARACTER", "CHAR")]
        [InlineData("INTEGER", "INT")]
        [InlineData("DECIMAL", "DECIMAL")]
        [InlineData("TIMESTAMP", "DATETIME")]
        [InlineData("CLOB", "TEXT")]
        [InlineData("UNKNOWN_TYPE", "UNKNOWN_TYPE")]
        public void MapAS400Type_ShouldMapToStandardSqlTypes(string input, string expected)
        {
            // Arrange
            var service = new As400SchemaService("fake_conn");

            // Act
            string result = service.MapAS400Type(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
