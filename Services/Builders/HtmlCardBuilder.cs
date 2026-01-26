using AngularGenerator.Core.Models;
using System.Text;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder สำหรบสราง HTML แบบ Card View (Grid Layout)
    /// </summary>
    public class HtmlCardBuilder
    {
        private readonly ComponentDefinition _definition;
        private readonly StringBuilder _sb = new();
        
        public HtmlCardBuilder(ComponentDefinition definition)
        {
            _definition = definition;
        }
        
        public string Build()
        {
            var primaryKey = _definition.PrimaryKeyName;
            var hasCheckbox = _definition.Fields.Any(f => f.UIControl == ControlType.Checkbox);
            
            _sb.AppendLine("<div class=\"container\">");
            BuildHeader();
            BuildLoadingIndicator();
            BuildCardGrid(primaryKey, hasCheckbox);
            BuildModal();
            _sb.AppendLine("</div>");
            
            return _sb.ToString();
        }
        
        private void BuildHeader()
        {
            var framework = _definition.CssFramework;
            
            _sb.AppendLine("  <div class=\"header-section\">");
            _sb.AppendLine($"    <h2>{_definition.EntityName} management</h2>");
            _sb.AppendLine("    <div class=\"header-actions\">");
            _sb.AppendLine("      <div class=\"search-wrapper\">");
            _sb.AppendLine("        <input type=\"text\" class=\"search-box\" placeholder=\"Search...\" ");
            _sb.AppendLine("               [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
            _sb.AppendLine("      </div>");
            
            if (_definition.IsPost)
            {
                if (framework == CSSFramework.AngularMaterial)
                {
                    _sb.AppendLine("      <button mat-raised-button color=\"primary\" (click)=\"openCreate()\">");
                    _sb.AppendLine("        <mat-icon>add</mat-icon>");
                    _sb.AppendLine($"        New {_definition.EntityName}");
                    _sb.AppendLine("      </button>");
                }
                else if (framework == CSSFramework.Bootstrap)
                {
                    _sb.AppendLine($"      <button class=\"btn btn-primary\" (click)=\"openCreate()\">+ New {_definition.EntityName}</button>");
                }
                else
                {
                    _sb.AppendLine($"      <button class=\"btn btn-add\" (click)=\"openCreate()\">+ New {_definition.EntityName}</button>");
                }
            }
            
            _sb.AppendLine("    </div>");
            _sb.AppendLine("  </div>");
            _sb.AppendLine();
        }
        
        private void BuildLoadingIndicator()
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
        
        private void BuildCardGrid(string primaryKey, bool hasCheckbox)
        {
            var framework = _definition.CssFramework;
            
            _sb.AppendLine("  @if (!isLoading()) {");
            _sb.AppendLine("    ");
            _sb.AppendLine("    <div class=\"product-grid\">");
            _sb.AppendLine($"      @for (item of filteredList(); track item.{primaryKey}) {{");
            
            if (framework == CSSFramework.AngularMaterial)
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
            var framework = _definition.CssFramework;
            _sb.AppendLine("          <div class=\"card-footer\">");
            
            if (_definition.IsGetById)
            {
                if (framework == CSSFramework.Bootstrap)
                {
                    _sb.AppendLine("             <button class=\"btn btn-sm btn-info\" (click)=\"openDetail(item)\">View</button>");
                }
                else
                {
                    _sb.AppendLine("             <button class=\"btn btn-primary view\" (click)=\"openDetail(item)\" title=\"View\">View</button>");
                }
            }
            
            if (_definition.IsUpdate)
            {
                if (framework == CSSFramework.Bootstrap)
                {
                    _sb.AppendLine("             <button class=\"btn btn-sm btn-warning\" (click)=\"openEdit(item)\">Edit</button>");
                }
                else
                {
                    _sb.AppendLine("             <button class=\"btn btn-warning edit\" (click)=\"openEdit(item)\" title=\"Edit\">Edit</button>");
                }
            }
            
            if (_definition.IsDelete)
            {
                if (framework == CSSFramework.Bootstrap)
                {
                    _sb.AppendLine("             <button class=\"btn btn-sm btn-danger\" (click)=\"onDelete(item)\">Delete</button>");
                }
                else
                {
                    _sb.AppendLine("             <button class=\"btn btn-danger delete\" (click)=\"onDelete(item)\" title=\"Delete\">Delete</button>");
                }
            }
            
            _sb.AppendLine("          </div>");
            _sb.AppendLine("        </div>");
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
        
        private void BuildModal()
        {
            if (!_definition.IsPost && !_definition.IsUpdate && !_definition.IsGetById) return;
            
            _sb.AppendLine();
            _sb.AppendLine("@if (showModal()) {");
            _sb.AppendLine("  <div class=\"modal-backdrop\">");
            _sb.AppendLine("    <div class=\"modal-content\">");
            _sb.AppendLine("      <div class=\"modal-header\">");
            _sb.AppendLine("        <h3>");
            _sb.AppendLine($"          @if(isViewMode()) {{ {_definition.EntityName} Details }}");
            _sb.AppendLine($"          @else if(isEditMode()) {{ Edit {_definition.EntityName} }}");
            _sb.AppendLine($"          @else {{ New {_definition.EntityName} }}");
            _sb.AppendLine("        </h3>");
            _sb.AppendLine("        <button class=\"btn-close-icon\" (click)=\"onClose()\"></button>");
            _sb.AppendLine("      </div>");
            _sb.AppendLine("      ");
            _sb.AppendLine($"      <form [formGroup]=\"{_definition.EntityName.ToLower()}Form\" (ngSubmit)=\"onSubmit()\">");
            _sb.AppendLine("        <div class=\"form-grid\">");
            _sb.AppendLine("          @for (field of formFields; track field.key) {");
            _sb.AppendLine("            <div class=\"form-group\" [class.full-width]=\"field.type === 'checkbox'\">");
            _sb.AppendLine("              <label>{{ field.label }} ");
            _sb.AppendLine("                @if(field.required && !isViewMode()){ <span class=\"required\">*</span> }");
            _sb.AppendLine("              </label>");
            _sb.AppendLine();
            _sb.AppendLine("              @if (field.type === 'checkbox') {");
            _sb.AppendLine("                <div class=\"checkbox-wrapper\">");
            _sb.AppendLine("                  <input type=\"checkbox\" [formControlName]=\"field.key\"> ");
            _sb.AppendLine("                  <span>Yes / No</span>");
            _sb.AppendLine("                </div>");
            _sb.AppendLine("              } @else {");
            _sb.AppendLine("                <input [type]=\"field.type\" ");
            _sb.AppendLine("                       [formControlName]=\"field.key\" ");
            _sb.AppendLine("                       class=\"form-control\"");
            _sb.AppendLine($"                       [class.is-invalid]=\"{_definition.EntityName.ToLower()}Form.get(field.key)?.invalid && {_definition.EntityName.ToLower()}Form.get(field.key)?.touched\">");
            _sb.AppendLine("              }");
            _sb.AppendLine("            </div>");
            _sb.AppendLine("          }");
            _sb.AppendLine("        </div>");
            _sb.AppendLine();
            var framework = _definition.CssFramework;
            _sb.AppendLine("        <div class=\"modal-actions\">");
            
            if (framework == CSSFramework.AngularMaterial)
            {
                _sb.AppendLine("          <button type=\"button\" mat-button (click)=\"onClose()\">Cancel</button>");
                
                if (_definition.IsGetById)
                {
                    _sb.AppendLine("          @if (isViewMode()) {");
                    _sb.AppendLine("            <button type=\"button\" mat-raised-button color=\"primary\" (click)=\"enableEditMode()\">Edit</button>");
                    _sb.AppendLine("          }");
                }
                
                _sb.AppendLine("          @if(!isViewMode()) {");
                _sb.AppendLine($"            <button type=\"submit\" mat-raised-button color=\"primary\" [disabled]=\"{_definition.EntityName.ToLower()}Form.invalid\">Save</button>");
                _sb.AppendLine("          }");
            }
            else if (framework == CSSFramework.Bootstrap)
            {
                _sb.AppendLine("          <button type=\"button\" class=\"btn btn-secondary\" (click)=\"onClose()\">Cancel</button>");
                
                if (_definition.IsGetById)
                {
                    _sb.AppendLine("          @if (isViewMode()) {");
                    _sb.AppendLine("            <button type=\"button\" class=\"btn btn-warning\" (click)=\"enableEditMode()\">Edit</button>");
                    _sb.AppendLine("          }");
                }
                
                _sb.AppendLine("          @if(!isViewMode()) {");
                _sb.AppendLine($"            <button type=\"submit\" class=\"btn btn-success\" [disabled]=\"{_definition.EntityName.ToLower()}Form.invalid\">Save</button>");
                _sb.AppendLine("          }");
            }
            else
            {
                _sb.AppendLine("          <button type=\"button\" class=\"btn btn-secondary\" (click)=\"onClose()\">Cancel</button>");
                
                if (_definition.IsGetById)
                {
                    _sb.AppendLine("          @if (isViewMode()) {");
                    _sb.AppendLine("            <button type=\"button\" class=\"btn btn-edit\" (click)=\"enableEditMode()\">Edit</button>");
                    _sb.AppendLine("          }");
                }
                
                _sb.AppendLine("          @if(!isViewMode()) {");
                _sb.AppendLine($"            <button type=\"submit\" class=\"btn btn-success\" [disabled]=\"{_definition.EntityName.ToLower()}Form.invalid\">Save</button>");
                _sb.AppendLine("          }");
            }
            
            _sb.AppendLine("        </div>");
            _sb.AppendLine("      </form>");
            _sb.AppendLine("    </div>");
            _sb.AppendLine("  </div>");
            _sb.AppendLine("}");
        }
    }
}
