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
            if (_renderer.RequiresSpecialTableRendering())
            {
                 _sb.AppendLine("<div style=\"padding: 16px; background: #f5f5f5; min-height: 100vh;\">");
            }
            else if (_definition.CssFramework == CSSFramework.Bootstrap)
            {
                _sb.AppendLine("<div class=\"container-fluid px-2 py-2 bg-light min-vh-100\">");
            }
            else
            {
                _sb.AppendLine("<div class=\"container\">");
            }
        }

        protected override void BuildHeader()
        {
            if (_renderer.RequiresSpecialTableRendering()) // Material
            {
                _sb.AppendLine("  <!-- Header Card -->");
                _sb.AppendLine("  <mat-card style=\"margin-bottom: 16px;\">");
                _sb.AppendLine("    <mat-card-content style=\"padding: 12px 16px;\">");
                _sb.AppendLine("      <div style=\"display: flex; justify-content: space-between; align-items: center;\">");
                _sb.AppendLine($"        <h3 style=\"margin: 0; font-weight: 600; color: #212121;\">{_definition.EntityName} Catalog</h3>");
                _sb.AppendLine("        <div style=\"display: flex; gap: 8px; align-items: center;\">");
                _sb.AppendLine("          <mat-form-field appearance=\"outline\" style=\"width: 250px; height: 100%;\" subscriptSizing=\"dynamic\">");
                _sb.AppendLine("            <mat-label>Search...</mat-label>");
                _sb.AppendLine("            <input matInput [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
                _sb.AppendLine("            <mat-icon matPrefix>search</mat-icon>");
                _sb.AppendLine("          </mat-form-field>");

                if (_definition.IsPost)
                {
                    _sb.AppendLine("          <button mat-raised-button color=\"primary\" (click)=\"openCreate()\" style=\"white-space: nowrap;\">");
                    _sb.AppendLine("            <mat-icon style=\"margin-right: 4px;\">add</mat-icon>");
                    _sb.AppendLine($"            Add {_definition.EntityName}");
                    _sb.AppendLine("          </button>");
                }
                
                _sb.AppendLine("        </div>");
                _sb.AppendLine("      </div>");
                _sb.AppendLine("    </mat-card-content>");
                _sb.AppendLine("  </mat-card>");
                _sb.AppendLine();
            }
            else if (_definition.CssFramework == CSSFramework.BasicCSS)
            {
                _sb.AppendLine("  <div class=\"header-section\">");
                _sb.AppendLine($"    <h2>{_definition.EntityName} Management</h2>");
                _sb.AppendLine("    <div class=\"header-actions\">");
                _sb.AppendLine("      <div class=\"search-wrapper\">");
                _sb.AppendLine("        <input type=\"text\" class=\"search-box\" placeholder=\"Search...\" ");
                _sb.AppendLine("               [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
                _sb.AppendLine("      </div>");
                
                if (_definition.IsPost)
                {
                    _sb.AppendLine("      <button class=\"btn btn-add\" (click)=\"openCreate()\">");
                    _sb.AppendLine($"        + New {_definition.EntityName}");
                    _sb.AppendLine("      </button>");
                }
                
                _sb.AppendLine("    </div>");
                _sb.AppendLine("  </div>");
                _sb.AppendLine();
            }
            else
            {
                base.BuildHeader();
            }
        }

        /// <summary>
        /// Override loading indicator for Card view
        /// </summary>
        protected override void BuildLoadingIndicator()
        {
            if (_renderer.RequiresSpecialTableRendering())
            {
                _sb.AppendLine("    @if (isLoading() && !showModal()) {");
                _sb.AppendLine("      <div style=\"display: flex; justify-content: center; align-items: center; padding: 80px;\">");
                _sb.AppendLine("        <mat-spinner diameter=\"50\"></mat-spinner>");
                _sb.AppendLine("      </div>");
                _sb.AppendLine("    }");
                _sb.AppendLine();
            }
            else
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
        }

        protected override void BuildModal()
        {
            if (_renderer.RequiresSpecialTableRendering())
            {
                 if (!_definition.IsPost && !_definition.IsUpdate && !_definition.IsGetById) return;

                _sb.AppendLine();
                _sb.AppendLine("@if (showModal()) {");
                _sb.AppendLine("  <div style=\"position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; backdrop-filter: blur(2px);\">");
                _sb.AppendLine("    <mat-card style=\"width: 700px; max-width: 90vw; max-height: 90vh; overflow: hidden; display: flex; flex-direction: column;\">");
                _sb.AppendLine("      <mat-card-header>");
                _sb.AppendLine("        <mat-card-title>");
                _sb.AppendLine("          <div style=\"display: flex; justify-content: space-between; align-items: center; width: 100%;\">");
                _sb.AppendLine("            <h3 style=\"margin: 0;\">");
                _sb.AppendLine($"              @if(isViewMode()) {{ View {_definition.EntityName} Detail }}");
                _sb.AppendLine($"              @else if(isEditMode()) {{ Edit {_definition.EntityName} }}");
                _sb.AppendLine($"              @else {{ Create New {_definition.EntityName} }}");
                _sb.AppendLine("            </h3>");
                _sb.AppendLine("          </div>");
                _sb.AppendLine("        </mat-card-title>");
                _sb.AppendLine("      </mat-card-header>");
                _sb.AppendLine();
                _sb.AppendLine("      <mat-card-content style=\"overflow-y: auto; flex: 1;\">");
                _sb.AppendLine($"        <form [formGroup]=\"{_definition.EntityName.ToLower()}Form\" (ngSubmit)=\"onSubmit()\">");
                _sb.AppendLine("          <div style=\"display: grid; grid-template-columns: 1fr 1fr; gap: 16px; padding: 16px 0;\">");
                
                _sb.AppendLine("            @for (field of formFields; track field.key) {");
                _sb.AppendLine("              <div [style.grid-column]=\"field.type === 'checkbox' ? 'span 2' : 'span 1'\">");
                _sb.AppendLine("                @if (field.type === 'checkbox') {");
                _sb.AppendLine("                  <mat-checkbox [formControlName]=\"field.key\">");
                _sb.AppendLine("                    {{ field.label }}");
                _sb.AppendLine("                    @if(field.required && !isViewMode()) { <span style=\"color: red;\">*</span> }");
                _sb.AppendLine("                  </mat-checkbox>");
                _sb.AppendLine("                } @else {");
                _sb.AppendLine("                  <mat-form-field appearance=\"outline\" style=\"width: 100%;\" subscriptSizing=\"dynamic\">");
                _sb.AppendLine("                    <mat-label>");
                _sb.AppendLine("                      {{ field.label }}");
                _sb.AppendLine("                      @if(field.required && !isViewMode()) { <span style=\"color: red;\">*</span> }");
                _sb.AppendLine("                    </mat-label>");
                _sb.AppendLine("                    <input matInput ");
                _sb.AppendLine("                           [type]=\"field.type\"");
                _sb.AppendLine("                           [formControlName]=\"field.key\"");
                _sb.AppendLine("                           [attr.maxlength]=\"field.maxLength\"");
                _sb.AppendLine("                           [readonly]=\"isViewMode()\">");
                _sb.AppendLine("                    @if (field.maxLength) {");
                _sb.AppendLine($"                      <mat-hint align=\"end\">{{{{ {_definition.EntityName.ToLower()}Form.get(field.key)?.value?.length || 0 }}}}/{{{{ field.maxLength }}}}</mat-hint>");
                _sb.AppendLine("                    }");
                _sb.AppendLine("                  </mat-form-field>");
                _sb.AppendLine("                }");
                _sb.AppendLine("              </div>");
                _sb.AppendLine("            }");
                
                _sb.AppendLine("          </div>");
                _sb.AppendLine("        </form>");
                _sb.AppendLine("      </mat-card-content>");
                _sb.AppendLine();
                _sb.AppendLine("      <mat-card-actions style=\"display: flex; justify-content: flex-end; gap: 8px; padding: 16px; border-top: 1px solid #e0e0e0;\">");
                _sb.AppendLine("        <button mat-button (click)=\"onClose()\">");
                _sb.AppendLine("          <mat-icon>close</mat-icon>");
                _sb.AppendLine("          {{ isViewMode() ? 'Close' : 'Cancel' }}");
                _sb.AppendLine("        </button>");
                _sb.AppendLine();
                _sb.AppendLine("        @if(isViewMode()) {");
                _sb.AppendLine("          <button mat-raised-button color=\"accent\" (click)=\"enableEditMode()\">");
                _sb.AppendLine("            <mat-icon>edit</mat-icon>");
                _sb.AppendLine("            Edit");
                _sb.AppendLine("          </button>");
                _sb.AppendLine("        }");
                _sb.AppendLine();
                _sb.AppendLine("        @if(!isViewMode()) {");
                _sb.AppendLine($"          <button mat-raised-button color=\"primary\" (click)=\"onSubmit()\" [disabled]=\"{_definition.EntityName.ToLower()}Form.invalid\">");
                _sb.AppendLine("            <mat-icon>{{ isEditMode() ? 'save' : 'add' }}</mat-icon>");
                _sb.AppendLine($"            {{{{ isEditMode() ? 'Update' : 'Create' }}}}");
                _sb.AppendLine("          </button>");
                _sb.AppendLine("        }");
                _sb.AppendLine("      </mat-card-actions>");
                _sb.AppendLine("    </mat-card>");
                _sb.AppendLine("  </div>");
                _sb.AppendLine("}");
            }
            else
            {
                base.BuildModal();
            }
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
            
            if (_definition.CssFramework == CSSFramework.BasicCSS)
            {
                _sb.AppendLine("    <div class=\"product-grid\">");
                _sb.AppendLine($"      @for (item of filteredList(); track item.{primaryKey}) {{");
                BuildBasicCssCard(primaryKey, hasCheckbox);
                _sb.AppendLine("      }");
                _sb.AppendLine("    </div>");
            }
            else if (_renderer.RequiresSpecialTableRendering())
            {
                _sb.AppendLine("    <div class=\"product-grid\" style=\"display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px;\">");
                _sb.AppendLine($"      @for (item of filteredList(); track item.{primaryKey}) {{");
                BuildMaterialCard(primaryKey, hasCheckbox);
                _sb.AppendLine("      }");
                _sb.AppendLine("    </div>");
            }
            else // Bootstrap
            {
                _sb.AppendLine("    <div class=\"row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-4\">");
                _sb.AppendLine($"      @for (item of filteredList(); track item.{primaryKey}) {{");
                BuildBootstrapCard(primaryKey, hasCheckbox);
                _sb.AppendLine("      }");
                 _sb.AppendLine("    </div>");
            }

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
            if (_definition.IsGetById)
            {
               _sb.AppendLine("          <img mat-card-image src=\"https://placehold.co/600x400?text=No+Image\" alt=\"Product\" (click)=\"openDetail(item)\" style=\"cursor: pointer;\">");
            }
            else
            {
               _sb.AppendLine("          <img mat-card-image src=\"https://placehold.co/600x400?text=No+Image\" alt=\"Product\">");
            }
            
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
            if (_definition.CssFramework == CSSFramework.Bootstrap)
            {
                BuildBootstrapCard(primaryKey, hasCheckbox);
            }
            else
            {
                BuildBasicCssCard(primaryKey, hasCheckbox);
            }
        }

        private void BuildBootstrapCard(string primaryKey, bool hasCheckbox)
        {
             _sb.AppendLine("        <div class=\"col mb-4\">");
             _sb.AppendLine("          <div class=\"card h-100 shadow-sm border-0\">");
             
             // Image
             _sb.AppendLine("            <div class=\"position-relative\">");
             if (_definition.IsGetById)
             {
                 _sb.AppendLine("              <img src=\"https://placehold.co/600x400?text=No+Image\" class=\"card-img-top\" alt=\"Product\" (click)=\"openDetail(item)\" style=\"cursor: pointer; height: 200px; object-fit: cover;\">");
             }
             else
             {
                 _sb.AppendLine("              <img src=\"https://placehold.co/600x400?text=No+Image\" class=\"card-img-top\" alt=\"Product\" style=\"height: 200px; object-fit: cover;\">");
             }
             
             if (hasCheckbox)
             {
                 var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                 if (checkboxField != null)
                 {
                     _sb.AppendLine($"              <span class=\"position-absolute top-0 end-0 m-2 badge rounded-pill\" [class.bg-success]=\"!item.{checkboxField.FieldName}\" [class.bg-danger]=\"item.{checkboxField.FieldName}\">");
                     _sb.AppendLine($"                {{{{ item.{checkboxField.FieldName} ? 'Inactive' : 'Active' }}}}");
                     _sb.AppendLine("              </span>");
                 }
             }
             _sb.AppendLine("            </div>");
             
             _sb.AppendLine("            <div class=\"card-body\">");
             
             var titleField = _definition.Fields.FirstOrDefault(f => !f.IsPrimaryKey);
             if (titleField != null)
             {
                 _sb.AppendLine($"              <h5 class=\"card-title fw-bold text-truncate\" title=\"{{{{ item.{titleField.FieldName} }}}}\">{{{{ item.{titleField.FieldName} }}}}</h5>");
             }

             // Show some fields
             var displayFields = _definition.Fields
                 .Where(f => !f.IsPrimaryKey && f.UIControl != ControlType.Checkbox && f != titleField)
                 .Take(3)
                 .ToList();

             foreach(var field in displayFields)
             {
                 _sb.AppendLine("              <p class=\"card-text mb-1 small text-muted\">");
                 _sb.AppendLine($"                <strong>{field.Label}:</strong> {{{{ item.{field.FieldName} }}}}");
                 _sb.AppendLine("              </p>");
             }
             
             _sb.AppendLine("            </div>");
             
             // Footer Actions
             _sb.AppendLine("            <div class=\"card-footer bg-white border-top-0 d-flex gap-2 justify-content-between p-3\">");
             
             if (_definition.IsGetById)
             {
                 _sb.AppendLine("              <button class=\"btn btn-sm btn-outline-info flex-fill\" (click)=\"openDetail(item)\">");
                 _sb.AppendLine("                <i class=\"bi bi-eye\"></i> View");
                 _sb.AppendLine("              </button>");
             }
             if (_definition.IsUpdate)
             {
                 _sb.AppendLine("              <button class=\"btn btn-sm btn-outline-warning flex-fill\" (click)=\"openEdit(item)\">");
                 _sb.AppendLine("                <i class=\"bi bi-pencil\"></i> Edit");
                 _sb.AppendLine("              </button>");
             }
             if (_definition.IsDelete)
             {
                 _sb.AppendLine("              <button class=\"btn btn-sm btn-outline-danger flex-fill\" (click)=\"onDelete(item)\">");
                 _sb.AppendLine("                <i class=\"bi bi-trash\"></i> Del");
                 _sb.AppendLine("              </button>");
             }
             
             _sb.AppendLine("            </div>");
             _sb.AppendLine("          </div>");
             _sb.AppendLine("        </div>");
        }

        private void BuildBasicCssCard(string primaryKey, bool hasCheckbox)
        {
            _sb.AppendLine("        <div class=\"card\" >");
            _sb.AppendLine("          <div class=\"card-image-wrapper\">");
            if (_definition.IsGetById)
            {
                _sb.AppendLine("            <img src=\"https://placehold.co/600x400?text=No+Image\" alt=\"Product\" (click)=\"openDetail(item)\" style=\"cursor: pointer;\"/>");
            }
            else
            {
                _sb.AppendLine("            <img src=\"https://placehold.co/600x400?text=No+Image\" alt=\"Product\"/>");
            }
            
            // Badge for checkbox field
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
            
            // Title (use first non-PK field)
            var titleField = _definition.Fields.FirstOrDefault(f => !f.IsPrimaryKey);
            if (titleField != null)
            {
                _sb.AppendLine($"            <h3 class=\"card-title\" title=\"{{{{ item.{titleField.FieldName} }}}}\">{{{{ item.{titleField.FieldName} }}}}</h3>");
            }
            
            _sb.AppendLine("            ");
            _sb.AppendLine("            <div class=\"card-info\">");
            
            // Display next 2 fields (skip checkbox and PK)
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
