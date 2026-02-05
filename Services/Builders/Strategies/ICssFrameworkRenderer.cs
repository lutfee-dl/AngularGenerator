namespace AngularGenerator.Services.Builders.Strategies
{
    /// <summary>
    /// Interface for CSS Framework rendering strategies
    /// Implements Strategy Pattern for framework-specific CSS classes and HTML rendering
    /// </summary>
    public interface ICssFrameworkRenderer
    {
        /// <summary>
        /// Get container CSS class
        /// </summary>
        string GetContainerClass();
        
        /// <summary>
        /// Get button CSS class by variant
        /// </summary>
        /// <param name="variant">Button variant (primary, secondary, info, warning, danger, success)</param>
        string GetButtonClass(string variant = "primary");
        
        /// <summary>
        /// Get table CSS class
        /// </summary>
        string GetTableClass();
        
        /// <summary>
        /// Get badge CSS class
        /// </summary>
        /// <param name="isActive">Whether badge represents active state</param>
        string GetBadgeClass(bool isActive);
        
        /// <summary>
        /// Get form input CSS class
        /// </summary>
        string GetInputClass();
        
        /// <summary>
        /// Render button HTML (for frameworks with special structure like Material)
        /// </summary>
        string RenderButton(string text, string onClick, string icon = null);
        
        /// <summary>
        /// Render form input HTML (for frameworks with special structure like Material)
        /// </summary>
        string RenderFormInput(string fieldKey, string fieldType, int? maxLength = null);
        
        /// <summary>
        /// Get required TypeScript imports for this framework
        /// </summary>
        string[] GetRequiredImports();
        
        /// <summary>
        /// Get imports declaration for component decorator
        /// </summary>
        string GetImportsDeclaration();
        
        /// <summary>
        /// Check if this framework requires special table rendering
        /// </summary>
        bool RequiresSpecialTableRendering();
    }
}
