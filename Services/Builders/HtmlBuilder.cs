using System.Text;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder for Table View HTML with support for multiple CSS frameworks
    /// </summary>
    public class HtmlBuilder
    {
        private readonly ComponentDefinition _definition;
        
        public HtmlBuilder(ComponentDefinition definition)
        {
            _definition = definition;
        }
        
        public string Build()
        {
            var framework = _definition.CssFramework;
            var sb = new StringBuilder();
            
            sb.AppendLine($"<!-- {_definition.EntityName} Component - Table View -->");
            sb.AppendLine($"<!-- CSS Framework: {framework} -->");
            sb.AppendLine();

            BuildHeader(sb, framework);
            BuildLoadingIndicator(sb, framework);
            BuildDataTable(sb, framework);
            BuildModal(sb, framework);
            
            sb.AppendLine("</div>");
            
            return sb.ToString();
        }

        private void BuildHeader(StringBuilder sb, CSSFramework framework)
        {
            var containerClass = framework switch
            {
                CSSFramework.Bootstrap => "container mt-4",
                CSSFramework.AngularMaterial => "container",
                _ => "container"
            };

            var buttonClass = framework switch
            {
                CSSFramework.Bootstrap => "btn btn-primary",
                CSSFramework.AngularMaterial => "mat-raised-button mat-primary",
                _ => "btn btn-add"
            };

            sb.AppendLine($"<div class=\"{containerClass}\">");
            sb.AppendLine("  <div class=\"header-section\">");
            sb.AppendLine($"    <h2>{_definition.EntityName} Management</h2>");
            sb.AppendLine("    <div class=\"header-actions\">");
            sb.AppendLine("      <input type=\"text\" class=\"search-box\" placeholder=\"Search...\" [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
            
            if (_definition.IsPost)
            {
                if (framework == CSSFramework.AngularMaterial)
                {
                    sb.AppendLine($"      <button {buttonClass} (click)=\"openCreate()\">");
                    sb.AppendLine("        <mat-icon>add</mat-icon> Add New");
                    sb.AppendLine("      </button>");
                }
                else
                {
                    sb.AppendLine($"      <button class=\"{buttonClass}\" (click)=\"openCreate()\">+ Add New</button>");
                }
            }
            
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine();
        }

        private void BuildLoadingIndicator(StringBuilder sb, CSSFramework framework)
        {
            sb.AppendLine("  @if (isLoading() && !showModal()) {");
            sb.AppendLine("    <div class=\"loading\">Loading...</div>");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        private void BuildDataTable(StringBuilder sb, CSSFramework framework)
        {
            sb.AppendLine("  @if (!isLoading() || dataList().length > 0) {");
            sb.AppendLine("    <div class=\"table-responsive\">");
            BuildStandardTable(sb, framework);
            sb.AppendLine("    </div>");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        private void BuildStandardTable(StringBuilder sb, CSSFramework framework)
        {
            var tableClass = framework == CSSFramework.Bootstrap 
                ? "table table-striped table-hover" 
                : "table";

            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";

            sb.AppendLine($"      <table class=\"{tableClass}\">");
            sb.AppendLine("        <thead>");
            sb.AppendLine("          <tr>");
            
            // PK Column with sort
            sb.AppendLine($"            <th (click)=\"onSort('{primaryKey}')\" class=\"sortable\" style=\"width: 70px;\">");
            sb.AppendLine($"              ID");
            sb.AppendLine($"              @if(sortColumn() === '{primaryKey}') {{");
            sb.AppendLine("                <span class=\"sort-arrow\">{{ sortDirection() === 'asc' ? '▲' : '▼' }}</span>");
            sb.AppendLine("              }");
            sb.AppendLine("            </th>");
            
            // Other columns (exclude checkbox fields from columns)
            sb.AppendLine("            @for (field of formFields; track field.key) {");
            sb.AppendLine("              @if (field.type !== 'checkbox') {");
            sb.AppendLine("                <th (click)=\"onSort(field.key)\" class=\"sortable\">");
            sb.AppendLine("                  {{ field.label }}");
            sb.AppendLine("                  @if(sortColumn() === field.key) {");
            sb.AppendLine("                    <span class=\"sort-arrow\">{{ sortDirection() === 'asc' ? '▲' : '▼' }}</span>");
            sb.AppendLine("                  }");
            sb.AppendLine("                </th>");
            sb.AppendLine("              }");
            sb.AppendLine("            }");
            
            // Status column (for checkbox fields)
            var hasCheckbox = _definition.Fields.Any(f => f.UIControl == ControlType.Checkbox);
            if (hasCheckbox)
            {
                sb.AppendLine("            <th class=\"text-center\">Status</th>");
            }
            
            // Actions column
            sb.AppendLine("            <th class=\"text-center\">Actions</th>");
            sb.AppendLine("          </tr>");
            sb.AppendLine("        </thead>");
            sb.AppendLine("        <tbody>");
            
            // Data rows using filteredList
            sb.AppendLine($"          @for (item of filteredList(); track item.{primaryKey}) {{");
            sb.AppendLine("            <tr>");
            sb.AppendLine($"              <td>{{{{ item.{primaryKey} }}}}</td>");
            
            // Field values (exclude checkbox)
            sb.AppendLine("              @for (field of formFields; track field.key) {");
            sb.AppendLine("                @if (field.type !== 'checkbox') {");
            sb.AppendLine("                  <td>{{ $any(item)[field.key] }}</td>");
            sb.AppendLine("                }");
            sb.AppendLine("              }");
            
            // Status badge (for checkbox fields)
            if (hasCheckbox)
            {
                var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                if (checkboxField != null)
                {
                    sb.AppendLine("              <td class=\"text-center\">");
                    sb.AppendLine($"                @if ($any(item)['{checkboxField.FieldName}']) {{");
                    sb.AppendLine("                  <span class=\"badge badge-inactive\">Inactive</span>");
                    sb.AppendLine("                } @else {");
                    sb.AppendLine("                  <span class=\"badge badge-active\">Active</span>");
                    sb.AppendLine("                }");
                    sb.AppendLine("              </td>");
                }
            }
            
            // Actions
            sb.AppendLine("              <td class=\"text-center action-col\">");
            
            if (_definition.IsGet && _definition.IsGetById)
            {
                var btnClass = framework == CSSFramework.Bootstrap ? "btn btn-sm btn-info" : "btn btn-sm btn-info";
                sb.AppendLine($"                <button class=\"{btnClass}\" (click)=\"openDetail(item.{primaryKey})\">View</button>");
            }
            
            if (_definition.IsUpdate)
            {
                var btnClass = framework == CSSFramework.Bootstrap ? "btn btn-sm btn-warning" : "btn btn-sm btn-edit";
                sb.AppendLine($"                <button class=\"{btnClass}\" (click)=\"openEdit(item.{primaryKey})\">Edit</button>");
            }
            
            if (_definition.IsDelete)
            {
                var btnClass = framework == CSSFramework.Bootstrap ? "btn btn-sm btn-danger" : "btn btn-sm btn-delete";
                sb.AppendLine($"                <button class=\"{btnClass}\" (click)=\"onDelete(item)\">Delete</button>");
            }
            
            sb.AppendLine("              </td>");
            sb.AppendLine("            </tr>");
            sb.AppendLine("          }");
            
            // No results message
            sb.AppendLine();
            sb.AppendLine("          @if(filteredList().length === 0 && dataList().length > 0) {");
            sb.AppendLine($"            <tr><td [attr.colspan]=\"formFields.length + 3\" class=\"text-center\">No results found</td></tr>");
            sb.AppendLine("          }");
            sb.AppendLine("        </tbody>");
            sb.AppendLine("      </table>");
        }

        private void BuildMaterialTable(StringBuilder sb)
        {
            sb.AppendLine("    <table mat-table [dataSource]=\"dataList()\" class=\"mat-elevation-z2\">");
            sb.AppendLine();

            // Generate columns
            foreach (var field in _definition.Fields)
            {
                sb.AppendLine($"      <!-- Column: {field.FieldName} -->");
                sb.AppendLine($"      <ng-container matColumnDef=\"{field.FieldName}\">");
                sb.AppendLine($"        <th mat-header-cell *matHeaderCellDef>{field.Label}</th>");
                sb.AppendLine($"        <td mat-cell *matCellDef=\"let element\">{{{{ element.{field.FieldName} }}}}</td>");
                sb.AppendLine("      </ng-container>");
                sb.AppendLine();
            }

            // Actions column
            if (_definition.IsUpdate || _definition.IsDelete)
            {
                sb.AppendLine("      <!-- Actions Column -->");
                sb.AppendLine("      <ng-container matColumnDef=\"actions\">");
                sb.AppendLine("        <th mat-header-cell *matHeaderCellDef>Actions</th>");
                sb.AppendLine("        <td mat-cell *matCellDef=\"let element\">");
                
                if (_definition.IsUpdate)
                {
                    sb.AppendLine("          <button mat-icon-button color=\"primary\" (click)=\"onEdit(element)\">");
                    sb.AppendLine("            <mat-icon>edit</mat-icon>");
                    sb.AppendLine("          </button>");
                }
                
                if (_definition.IsDelete)
                {
                    var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
                    sb.AppendLine($"          <button mat-icon-button color=\"warn\" (click)=\"onDelete(element.{primaryKey})\">");
                    sb.AppendLine("            <mat-icon>delete</mat-icon>");
                    sb.AppendLine("          </button>");
                }
                
                sb.AppendLine("        </td>");
                sb.AppendLine("      </ng-container>");
                sb.AppendLine();
            }

            sb.AppendLine("      <tr mat-header-row *matHeaderRowDef=\"displayedColumns\"></tr>");
            sb.AppendLine("      <tr mat-row *matRowDef=\"let row; columns: displayedColumns;\"></tr>");
            sb.AppendLine("    </table>");
        }

        private void BuildModal(StringBuilder sb, CSSFramework framework)
        {
            if (!_definition.IsPost && !_definition.IsUpdate) return;

            sb.AppendLine();
            sb.AppendLine("  @if (showModal()) {");
            sb.AppendLine("    <div class=\"modal-backdrop\">");
            sb.AppendLine("      <div class=\"modal-content\">");
            
            // Modal Title
            sb.AppendLine("        <h3>");
            sb.AppendLine("          @if(isViewMode()) { View Detail }");
            sb.AppendLine($"          @else if(isEditMode()) {{ Edit {_definition.EntityName} }}");
            sb.AppendLine($"          @else {{ Create New {_definition.EntityName} }}");
            sb.AppendLine("        </h3>");
            sb.AppendLine();
            
            // Form
            sb.AppendLine($"        <form [formGroup]=\"{_definition.EntityName.ToLower()}Form\" (ngSubmit)=\"onSubmit()\">");
            sb.AppendLine("          <div class=\"form-grid\">");
            
            // Loop through formFields
            sb.AppendLine("            @for (field of formFields; track field.key) {");
            sb.AppendLine("              <div class=\"form-group\" [class.full-width]=\"field.type === 'checkbox'\">");
            sb.AppendLine("                <label>{{ field.label }}");
            sb.AppendLine("                  @if(field.required && !isViewMode()) { <span class=\"required\">*</span> }");
            sb.AppendLine("                </label>");
            sb.AppendLine();
            
            // Checkbox field
            sb.AppendLine("                @if (field.type === 'checkbox') {");
            sb.AppendLine("                  <div class=\"checkbox-wrapper\">");
            sb.AppendLine("                    <input type=\"checkbox\" [formControlName]=\"field.key\">");
            sb.AppendLine("                    <span>Yes / No</span>");
            sb.AppendLine("                  </div>");
            
            // Other input types
            sb.AppendLine("                } @else {");
            sb.AppendLine("                  <input [type]=\"field.type\"");
            sb.AppendLine("                         [formControlName]=\"field.key\"");
            sb.AppendLine("                         class=\"form-control\"");
            sb.AppendLine("                         [attr.maxlength]=\"field.maxLength\">");
            sb.AppendLine("                }");
            sb.AppendLine("              </div>");
            sb.AppendLine("            }");
            sb.AppendLine("          </div>");
            sb.AppendLine();
            
            // Modal Actions
            sb.AppendLine("          <div class=\"modal-actions\">");
            sb.AppendLine("            <button type=\"button\" class=\"btn btn-secondary\" (click)=\"onClose()\">");
            sb.AppendLine("              {{ isViewMode() ? 'Close' : 'Cancel' }}");
            sb.AppendLine("            </button>");
            sb.AppendLine();
            sb.AppendLine("            @if(!isViewMode()) {");
            sb.AppendLine($"              <button type=\"submit\" class=\"btn btn-success\" [disabled]=\"{_definition.EntityName.ToLower()}Form.invalid\">");
            sb.AppendLine("                {{ isEditMode() ? 'Update' : 'Save' }}");
            sb.AppendLine("              </button>");
            sb.AppendLine("            }");
            sb.AppendLine("          </div>");
            sb.AppendLine("        </form>");
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  }");
        }
    }
}
