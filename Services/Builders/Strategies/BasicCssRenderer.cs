namespace AngularGenerator.Services.Builders.Strategies
{
    /// <summary>
    /// Basic CSS Framework Renderer
    /// </summary>
    public class BasicCssRenderer : ICssFrameworkRenderer
    {
        public string GetContainerClass() => "container";
        
        public string GetButtonClass(string variant = "primary")
        {
            return variant switch
            {
                "primary" => "btn btn-add",
                "secondary" => "btn btn-secondary",
                "info" => "btn btn-sm btn-info",
                "warning" => "btn btn-sm btn-edit",
                "danger" => "btn btn-sm btn-delete",
                "success" => "btn btn-success",
                _ => "btn btn-add"
            };
        }
        
        public string GetTableClass() => "table";
        
        public string GetBadgeClass(bool isActive) 
            => isActive ? "badge badge-active" : "badge badge-inactive";
        
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
