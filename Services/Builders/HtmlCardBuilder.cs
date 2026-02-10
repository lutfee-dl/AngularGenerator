using AngularGenerator.Core.Models;
using AngularGenerator.Services.Builders.Base;
using AngularGenerator.Services.Builders.Strategies;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder for Card View HTML - Now using OOP inheritance
    /// Extends BaseHtmlBuilder to eliminate code duplication
    /// Uses Strategy Pattern for framework-specific rendering
    /// </summary>
    public class HtmlCardBuilder : BaseHtmlBuilder
    {
        private readonly ICssFrameworkRenderer _renderer;
        
        public HtmlCardBuilder(ComponentDefinition definition) : base(definition)
        {
            _renderer = CssRendererFactory.Create(definition.CssFramework);
        }

        /// <summary>
        /// Override container for Card view (different styling)
        /// </summary>
        protected override void BuildContainer()
        {
            _sb.AppendLine("<div class=\"container\">");
        }

        /// <summary>
        /// Override header for Card view (simpler header)
        /// </summary>
        protected override void BuildHeader()
        {
            _sb.AppendLine("  <div class=\"header-section\">");
            _sb.AppendLine($"    <h2>{_definition.EntityName} management</h2>");
            _sb.AppendLine("    <div class=\"header-actions\">");
            _sb.AppendLine("      <div class=\"search-wrapper\">");
            _sb.AppendLine("        <input type=\"text\" class=\"search-box\" placeholder=\"Search...\" ");
            _sb.AppendLine("               [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
            _sb.AppendLine("      </div>");
            
            if (_definition.IsPost)
            {
                _sb.AppendLine(_renderer.RenderButton($"New {_definition.EntityName}", "openCreate()", "add"));
            }
            
            _sb.AppendLine("    </div>");
            _sb.AppendLine("  </div>");
            _sb.AppendLine();
        }

        /// <summary>
        /// Override loading indicator for Card view
        /// </summary>
        protected override void BuildLoadingIndicator()
        {
            if (_definition.IsPost || _definition.IsUpdate || _definition.IsGetById)
            {
                _sb.AppendLine("  @if (isLoading() && !showModal()) {");
            }
            else
            {
                _sb.AppendLine("  @if (isLoading()) {");
            }
            _sb.AppendLine("    <div class=\"loading-state\">");
            _sb.AppendLine("      <div class=\"spinner\"></div> Loading...");
            _sb.AppendLine("    </div>");
            _sb.AppendLine("  }");
            _sb.AppendLine();
        }

        /// <summary>
        /// Implements abstract method from BaseHtmlBuilder
        /// Builds the card-specific data section
        /// </summary>
        protected override void BuildDataSection()
        {
            var primaryKey = _definition.PrimaryKeyName;
            var hasCheckbox = _definition.Fields.Any(f => f.UIControl == ControlType.Checkbox);
            
            _sb.AppendLine("  @if (!isLoading()) {");
            _sb.AppendLine("    ");
            _sb.AppendLine("    <div class=\"product-grid\">");
            _sb.AppendLine($"      @for (item of filteredList(); track item.{primaryKey}) {{");
            
            if (_renderer.RequiresSpecialTableRendering())
            {
                BuildMaterialCard(primaryKey, hasCheckbox);
            }
            else
            {
                BuildStandardCard(primaryKey, hasCheckbox);
            }
            
            _sb.AppendLine("      }");
            _sb.AppendLine("    </div>");
            _sb.AppendLine();
            _sb.AppendLine("    @if(filteredList().length === 0) {");
            _sb.AppendLine("        <div class=\"empty-state\">");
            _sb.AppendLine("            <p>No data found matching your search.</p>");
            _sb.AppendLine("        </div>");
            _sb.AppendLine("    }");
            _sb.AppendLine("  }");
        }
        
        private void BuildMaterialCard(string primaryKey, bool hasCheckbox)
        {
            _sb.AppendLine("        <mat-card class=\"product-card\">");
            
            // Card Header with Image
            var titleField = _definition.Fields.FirstOrDefault(f => !f.IsPrimaryKey);
            if (titleField != null)
            {
                _sb.AppendLine("          <mat-card-header>");
                _sb.AppendLine($"            <mat-card-title>{{{{ item.{titleField.FieldName} }}}}</mat-card-title>");
                
                if (hasCheckbox)
                {
                    var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                    if (checkboxField != null)
                    {
                        _sb.AppendLine("            <div class=\"card-header-actions\">");
                        _sb.AppendLine($"              @if (item.{checkboxField.FieldName}) {{");
                        _sb.AppendLine("                <mat-chip color=\"primary\">Active</mat-chip>");
                        _sb.AppendLine("              } @else {");
                        _sb.AppendLine("                <mat-chip color=\"warn\">Inactive</mat-chip>");
                        _sb.AppendLine("              }");
                        _sb.AppendLine("            </div>");
                    }
                }
                
                _sb.AppendLine("          </mat-card-header>");
            }
            
            // Card Image
            _sb.AppendLine("          <img mat-card-image src=\"https://placehold.co/600x400?text=No+Image\" alt=\"Product\" (click)=\"openDetail(item)\" style=\"cursor: pointer;\">");
            
            // Card Content - Use loop for fields
            _sb.AppendLine("          <mat-card-content>");
            _sb.AppendLine("            @for (field of cardFields; track field.key) {");
            _sb.AppendLine("              <div class=\"field-row\">");
            _sb.AppendLine("                <strong>{{ field.label }}:</strong>");
            _sb.AppendLine("                @if (field.type === 'number') {");
            _sb.AppendLine("                  <span>{{ item[field.key] | number:'1.2-2' }}</span>");
            _sb.AppendLine("                } @else if (field.type === 'date') {");
            _sb.AppendLine("                  <span>{{ item[field.key] | date:'short' }}</span>");
            _sb.AppendLine("                } @else {");
            _sb.AppendLine("                  <span>{{ item[field.key] }}</span>");
            _sb.AppendLine("                }");
            _sb.AppendLine("              </div>");
            _sb.AppendLine("            }");
            _sb.AppendLine("          </mat-card-content>");
            
            // Card Actions
            _sb.AppendLine("          <mat-card-actions align=\"end\">");
            
            if (_definition.IsGetById)
            {
                _sb.AppendLine("            <button mat-button color=\"primary\" (click)=\"openDetail(item)\">");
                _sb.AppendLine("              <mat-icon>visibility</mat-icon> View");
                _sb.AppendLine("            </button>");
            }
            
            if (_definition.IsUpdate)
            {
                _sb.AppendLine("            <button mat-button color=\"accent\" (click)=\"openEdit(item)\">");
                _sb.AppendLine("              <mat-icon>edit</mat-icon> Edit");
                _sb.AppendLine("            </button>");
            }
            
            if (_definition.IsDelete)
            {
                _sb.AppendLine("            <button mat-button color=\"warn\" (click)=\"onDelete(item)\">");
                _sb.AppendLine("              <mat-icon>delete</mat-icon> Delete");
                _sb.AppendLine("            </button>");
            }
            
            _sb.AppendLine("          </mat-card-actions>");
            _sb.AppendLine("        </mat-card>");
        }
        
        private void BuildStandardCard(string primaryKey, bool hasCheckbox)
        {
            _sb.AppendLine("        <div class=\"card\" >");
            _sb.AppendLine("          <div class=\"card-image-wrapper\">");
            _sb.AppendLine("            <img src=\"https://placehold.co/600x400?text=No+Image\" alt=\"Product\" (click)=\"openDetail(item)\"/>");
            
            // Badge สำหรบ checkbox field
            if (hasCheckbox)
            {
                var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                if (checkboxField != null)
                {
                    _sb.AppendLine($"            @if($any(item)['{checkboxField.FieldName}']) {{");
                    _sb.AppendLine("                <span class=\"badge-color\">Active</span>");
                    _sb.AppendLine("            }");
                }
            }
            
            _sb.AppendLine("          </div>");
            _sb.AppendLine();
            _sb.AppendLine("          <div class=\"card-body\">");
            
            // Title (ใช field แรกทไมใช PK)
            var titleField = _definition.Fields.FirstOrDefault(f => !f.IsPrimaryKey);
            if (titleField != null)
            {
                _sb.AppendLine($"            <h3 class=\"card-title\" title=\"{{{{ item.{titleField.FieldName} }}}}\">{{{{ item.{titleField.FieldName} }}}}</h3>");
            }
            
            _sb.AppendLine("            ");
            _sb.AppendLine("            <div class=\"card-info\">");
            
            // แสดง 2 fields ถดไป (ขามcheckbox และ PK)
            var displayFields = _definition.Fields
                .Where(f => !f.IsPrimaryKey && f.UIControl != ControlType.Checkbox)
                .Take(3)
                .ToList();
            
            if (displayFields.Count > 1)
            {
                var field1 = displayFields[1];
                _sb.AppendLine("              <span class=\"price\">");
                _sb.AppendLine($"                {{{{ item.{field1.FieldName} ? (item.{field1.FieldName} | number:'1.2-2') + ' ฿' : 'N/A' }}}}");
                _sb.AppendLine("              </span>");
            }
            
            if (displayFields.Count > 2)
            {
                var field2 = displayFields[2];
                _sb.AppendLine($"              <span class=\"stock\">Stock: {{{{ item.{field2.FieldName} }}}}</span>");
            }
            
            _sb.AppendLine("            </div>");
            _sb.AppendLine("          </div>");
            _sb.AppendLine();
            
            // Card Footer with Actions
            _sb.AppendLine("          <div class=\"card-footer\">");
            
            if (_definition.IsGetById)
            {
                _sb.AppendLine($"             <button class=\"{_renderer.GetButtonClass("info")}\" (click)=\"openDetail(item)\">View</button>");
            }
            
            if (_definition.IsUpdate)
            {
                _sb.AppendLine($"             <button class=\"{_renderer.GetButtonClass("warning")}\" (click)=\"openEdit(item)\">Edit</button>");
            }
            
            if (_definition.IsDelete)
            {
                _sb.AppendLine($"             <button class=\"{_renderer.GetButtonClass("danger")}\" (click)=\"onDelete(item)\">Delete</button>");
            }
            
            _sb.AppendLine("          </div>");
            _sb.AppendLine("        </div>");
        }
    }
}
