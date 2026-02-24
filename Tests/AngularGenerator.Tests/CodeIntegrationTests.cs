using Xunit;
using AngularGenerator.Services;
using AngularGenerator.Services.Builders;
using AngularGenerator.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace AngularGenerator.Tests
{
    public class CodeIntegrationTests
    {
        [Fact]
        public void ServiceBuilder_ShouldInjectCustomBaseUrl()
        {
            // Arrange
            var def = new ComponentDefinition 
            { 
                EntityName = "Product", 
                PrimaryKeyName = "Id",
                IsGet = true 
            };
            var builder = new ComponentBuilder(def);
            string customUrl = "https://api.myapp.com/v1/products";

            // Act
            var serviceCode = builder.BuildService(customUrl);

            // Assert
            Assert.Contains("private baseUrl = 'https://api.myapp.com/v1';", serviceCode);
            Assert.Contains("`${this.baseUrl}/products`", serviceCode);
        }

        [Fact]
        public void TypeScriptBuilder_ShouldImportInterface_WhenSeparated()
        {
            // Arrange
            var def = new ComponentDefinition 
            { 
                EntityName = "Customer", 
                SeparateInterface = true,
                Selector = "app-customer"
            };
            var builder = new ComponentBuilder(def);

            // Act
            var tsCode = builder.BuildTypeScript();

            // Assert
            Assert.Contains("import { CustomerModel } from './customer.interface';", tsCode);
        }

        [Fact]
        public void TypeScriptBuilder_ShouldIncludeValidators_WhenFieldIsRequired()
        {
            // Arrange
            var def = new ComponentDefinition
            {
                EntityName = "Order",
                IsPost = true,
                Fields = new List<AngularField>
                {
                    new AngularField { FieldName = "OrderNo", Label = "Order No", IsRequired = true, TsType = "string" },
                    new AngularField { FieldName = "Note", Label = "Note", IsRequired = false, TsType = "string" }
                }
            };
            var builder = new ComponentBuilder(def);

            // Act
            var tsCode = builder.BuildTypeScript();

            // Assert
            Assert.Contains("if (field.required) validators.push(Validators.required);", tsCode);
            // Verify that the data structure passed to initForm has the required property
            Assert.Contains("required: true", tsCode); // From formFields array
            Assert.Contains("required: false", tsCode); // From formFields array
        }

        [Fact]
        public void AngularComponentFactory_ShouldHandleEmptyColumnList()
        {
            // Arrange
            var factory = new AngularComponentFactory();
            
            // Act
            var result = factory.Create("EmptyTable", new List<DbColumnInfo>());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EmptyTable", result.EntityName);
            Assert.Empty(result.Fields);
        }

        [Fact]
        public void ServiceBuilder_ShouldHandleApiUrlWithBraces()
        {
            // Arrange
            var def = new ComponentDefinition 
            { 
                EntityName = "User", 
                PrimaryKeyName = "Id",
                IsGet = true
            };
            var builder = new ComponentBuilder(def);
            string customUrl = "http://api.test.com/v1/{users}";

            // Act
            var serviceCode = builder.BuildService(customUrl);

            // Assert
            Assert.Contains("private baseUrl = 'http://api.test.com/v1';", serviceCode);
            Assert.Contains("`${this.baseUrl}/users`", serviceCode);
        }

        [Fact]
        public void TypeScriptBuilder_ShouldIncludeExportMethods_WhenIsGetIsTrue()
        {
            // Arrange
            var def = new ComponentDefinition 
            { 
                EntityName = "Report", 
                IsGet = true,
                Fields = new List<AngularField> { new AngularField { FieldName = "Name", Label = "Name" } }
            };
            var builder = new ComponentBuilder(def);

            // Act
            var tsCode = builder.BuildTypeScript();
            var htmlCode = builder.BuildHtml();

            // Assert
            Assert.Contains("exportToExcel()", tsCode);
            Assert.Contains("exportToPdf()", tsCode);
            Assert.Contains("import('xlsx')", tsCode);
            Assert.Contains("import('jspdf')", tsCode);
            
            // Check HTML buttons
            Assert.Contains("exportToExcel()", htmlCode);
            Assert.Contains("exportToPdf()", htmlCode);
        }
    }
}
