using AngularGenerator.Core.Models;

namespace AngularGenerator.Services.Builders.Strategies
{
    /// <summary>
    /// Factory for creating CSS Framework Renderers
    /// Implements Factory Pattern
    /// </summary>
    public static class CssRendererFactory
    {
        /// <summary>
        /// Create appropriate renderer based on CSS framework
        /// </summary>
        public static ICssFrameworkRenderer Create(CSSFramework framework)
        {
            return framework switch
            {
                CSSFramework.Bootstrap => new BootstrapCssRenderer(),
                CSSFramework.AngularMaterial => new MaterialCssRenderer(),
                CSSFramework.BasicCSS => new BasicCssRenderer(),
                _ => new BasicCssRenderer()
            };
        }
    }
}
