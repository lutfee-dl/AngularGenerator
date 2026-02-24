using Xunit;
using AngularGenerator.Services;
using AngularGenerator.Services.Builders;
using AngularGenerator.Core.Models;
using System.Collections.Generic;

namespace AngularGenerator.Tests
{
    public class CrudTests
    {
        [Fact]
        public void BuildService_ShouldOnlyIncludeSelectedMethods()
        {
            // Arrange
            var def = new ComponentDefinition
            {
                EntityName = "User",
                PrimaryKeyName = "Id",
                IsGet = true,
                IsPost = true,
                IsUpdate = false, // DISABLED
                IsDelete = false  // DISABLED
            };
            var builder = new ComponentBuilder(def);

            // Act
            var serviceCode = builder.BuildService();

            // Assert
            Assert.Contains("getAll()", serviceCode);
            Assert.Contains("create(", serviceCode);
            Assert.DoesNotContain("update(", serviceCode);
            Assert.DoesNotContain("delete(", serviceCode);
        }

        [Fact]
        public void BuildTypeScript_ShouldOnlyIncludeSelectedFeatures()
        {
            // Arrange
            var def = new ComponentDefinition
            {
                EntityName = "Product",
                PrimaryKeyName = "Id",
                IsGet = true,
                IsPost = false,
                IsUpdate = false,
                IsDelete = true // Enabled Delete
            };
            var builder = new ComponentBuilder(def);

            // Act
            var tsCode = builder.BuildTypeScript();

            // Assert
            Assert.Contains("loadData()", tsCode);
            Assert.Contains("onDelete(", tsCode);
            Assert.DoesNotContain("onCreate()", tsCode);
            Assert.DoesNotContain("onUpdate()", tsCode);
            Assert.DoesNotContain("FormGroup", tsCode); // Should not have forms if only Get/Delete
        }

        [Fact]
        public void BuildHtml_CardView_ShouldHaveCardClasses()
        {
            // Arrange
            var def = new ComponentDefinition
            {
                EntityName = "News",
                LayoutType = UILayoutType.CardView,
                CssFramework = CSSFramework.Bootstrap,
                Fields = new List<AngularField>
                {
                    new AngularField { FieldName = "Title", Label = "Title" }
                }
            };
            var builder = new ComponentBuilder(def);

            // Act
            var htmlCode = builder.BuildHtml();

            // Assert
            Assert.Contains("card", htmlCode);
            // In Card View, we expect multiple card-body occurrences (one for header, others for items)
            Assert.Contains("card-body", htmlCode);
            Assert.DoesNotContain("<table", htmlCode);
        }

        [Fact]
        public void BuildHtml_TableView_ShouldHaveTableTag()
        {
            // Arrange
            var def = new ComponentDefinition
            {
                EntityName = "News",
                LayoutType = UILayoutType.TableView,
                CssFramework = CSSFramework.Bootstrap,
                Fields = new List<AngularField>
                {
                    new AngularField { FieldName = "Title", Label = "Title" }
                }
            };
            var builder = new ComponentBuilder(def);

            // Act
            var htmlCode = builder.BuildHtml();

            // Assert
            Assert.Contains("<table", htmlCode);
            // Note: Even table view uses card-body for the header section
            Assert.Contains("card-body", htmlCode); 
        }
    }
}
