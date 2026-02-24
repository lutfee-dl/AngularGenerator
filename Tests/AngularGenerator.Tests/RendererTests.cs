using Xunit;
using AngularGenerator.Services.Builders.Strategies;

namespace AngularGenerator.Tests
{
    public class RendererTests
    {
        [Fact]
        public void BootstrapRenderer_ShouldReturnCorrectButtonClasses()
        {
            // Arrange
            var renderer = new BootstrapCssRenderer();

            // Act & Assert
            Assert.Equal("btn btn-primary", renderer.GetButtonClass("primary"));
            Assert.Equal("btn btn-sm btn-danger", renderer.GetButtonClass("danger"));
            Assert.Equal("btn btn-success", renderer.GetButtonClass("success"));
            Assert.Equal("btn btn-primary", renderer.GetButtonClass("unknown")); // Default case
        }

        [Fact]
        public void MaterialRenderer_ShouldReturnMatClasses()
        {
            var renderer = new MaterialCssRenderer();
            
            Assert.Contains("mat-raised-button", renderer.GetButtonClass("primary"));
            Assert.Contains("mat-primary", renderer.GetButtonClass("primary"));
            Assert.Equal("mat-table", renderer.GetTableClass());
            Assert.Equal("mat-icon-button", renderer.GetButtonClass("danger"));
        }

        [Fact]
        public void BootstrapRenderer_ShouldReturnStandardInputClass()
        {
            var renderer = new BootstrapCssRenderer();
            Assert.Equal("form-control", renderer.GetInputClass());
        }
    }
}
