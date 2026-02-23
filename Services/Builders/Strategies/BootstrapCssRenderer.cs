namespace AngularGenerator.Services.Builders.Strategies
{
    /// <summary>
    /// Bootstrap CSS Framework Renderer
    /// </summary>
    public class BootstrapCssRenderer : ICssFrameworkRenderer
    {
        public string GetContainerClass() => "container mt-4";
        
        public string GetButtonClass(string variant = "primary")
        {
            return variant switch
            {
                "primary" => "btn btn-primary",
                "secondary" => "btn btn-secondary",
                "info" => "btn btn-sm btn-info",
                "warning" => "btn btn-sm btn-warning",
                "danger" => "btn btn-sm btn-danger",
                "success" => "btn btn-success",
                _ => "btn btn-primary"
            };
        }
        
        public string GetTableClass() => "table table-striped table-hover";
        
        public string GetBadgeClass(bool isActive) 
            => isActive ? "badge bg-success" : "badge bg-danger";
        
        public string GetInputClass() => "form-control";
        
        public string RenderButton(string text, string onClick, string? icon = null)
        {
            return $"<button class=\"{GetButtonClass()}\" (click)=\"{onClick}\">+ {text}</button>";
        }
        
        public string RenderFormInput(string fieldKey, string fieldType, int? maxLength = null)
        {
            var maxLengthAttr = maxLength.HasValue ? $"[attr.maxlength]=\"{maxLength}\"" : "";
            return $$"""
                  <input [type]="{{fieldType}}"
                         [formControlName]="{{fieldKey}}"
                         class="{{GetInputClass()}}"
                         {{maxLengthAttr}}>
            """;
        }
        
        public string[] GetRequiredImports() => Array.Empty<string>();
        
        public string GetImportsDeclaration() 
            => "imports: [CommonModule, ReactiveFormsModule, FormsModule]";
        
        public bool RequiresSpecialTableRendering() => false;
    }
}
