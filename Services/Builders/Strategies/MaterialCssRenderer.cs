namespace AngularGenerator.Services.Builders.Strategies
{
    /// <summary>
    /// Angular Material CSS Framework Renderer
    /// </summary>
    public class MaterialCssRenderer : ICssFrameworkRenderer
    {
        public string GetContainerClass() => "container";
        
        public string GetButtonClass(string variant = "primary")
        {
            return variant switch
            {
                "primary" => "mat-raised-button mat-primary",
                "secondary" => "mat-raised-button",
                "info" => "mat-icon-button",
                "warning" => "mat-icon-button",
                "danger" => "mat-icon-button",
                "success" => "mat-raised-button",
                _ => "mat-raised-button mat-primary"
            };
        }
        
        public string GetTableClass() => "mat-table";
        
        public string GetBadgeClass(bool isActive) => "";  // Material uses mat-chip
        
        public string GetInputClass() => "";  // Material uses mat-form-field
        
        public string RenderButton(string text, string onClick, string icon = null)
        {
            if (string.IsNullOrEmpty(icon))
            {
                return $"<button mat-raised-button color=\"primary\" (click)=\"{onClick}\">{text}</button>";
            }
            
            return $$"""
                  <button mat-raised-button color="primary" (click)="{{onClick}}">
                        <mat-icon>{{icon}}</mat-icon> {{text}}
                      </button>
            """;
        }
        
        public string RenderFormInput(string fieldKey, string fieldType, int? maxLength = null)
        {
            var maxLengthAttr = maxLength.HasValue ? $"[attr.maxlength]=\"{maxLength}\"" : "";
            return $$"""
                  <mat-form-field appearance="outline" class="w-100">
                        <input matInput [type]="{{fieldType}}"
                               [formControlName]="{{fieldKey}}"
                               {{maxLengthAttr}}>
                      </mat-form-field>
            """;
        }
        
        public string[] GetRequiredImports() => new[]
        {
            "MatTableModule",
            "MatButtonModule",
            "MatIconModule",
            "MatFormFieldModule",
            "MatInputModule",
            "MatChipsModule",
            "MatTooltipModule",
            "MatSortModule",
            "MatCardModule"
        };
        
        public string GetImportsDeclaration() 
            => "imports: [CommonModule, ReactiveFormsModule, FormsModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatChipsModule, MatTooltipModule, MatSortModule, MatCardModule]";
        
        public bool RequiresSpecialTableRendering() => true;
    }
}
