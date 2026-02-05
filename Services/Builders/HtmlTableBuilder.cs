using System.Text;
using AngularGenerator.Core.Models;
using AngularGenerator.Services.Builders.Strategies;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder for Table View HTML with support for multiple CSS frameworks
    /// Uses Strategy Pattern for framework-specific rendering
    /// </summary>
    public class HtmlTableBuilder
    {
        private readonly ComponentDefinition _definition;
        private readonly ICssFrameworkRenderer _renderer;
        
        public HtmlTableBuilder(ComponentDefinition definition)
        {
            _definition = definition;
            _renderer = CssRendererFactory.Create(definition.CssFramework);
        }
        
        public string Build()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"<!-- {_definition.EntityName} Component - Table View -->");
            sb.AppendLine($"<!-- CSS Framework: {_definition.CssFramework} -->");
            sb.AppendLine();

            BuildHeader(sb);
            BuildLoadingIndicator(sb);
            BuildDataTable(sb);
            BuildModal(sb);
            
            sb.AppendLine("</div>");
            
            return sb.ToString();
        }

        private void BuildHeader(StringBuilder sb)
        {
            var containerClass = _renderer.GetContainerClass();
            
            sb.AppendLine($"<div class=\"{containerClass}\">");
            sb.AppendLine("  <div class=\"header-section\">");
            sb.AppendLine($"    <h2>{_definition.EntityName} Management</h2>");
            sb.AppendLine("    <div class=\"header-actions\">");
            sb.AppendLine("      <input type=\"text\" class=\"search-box\" placeholder=\"Search...\" [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
            
            if (_definition.IsPost)
            {
                sb.AppendLine(_renderer.RenderButton("Add New", "openCreate()", "add"));
            }
            
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine();
        }

        private void BuildLoadingIndicator(StringBuilder sb)
        {
            if (_definition.IsPost || _definition.IsUpdate)
            {
                sb.AppendLine("  @if (isLoading() && !showModal()) {");
            }
            else
            {
                sb.AppendLine("  @if (isLoading()) {");
            }
            sb.AppendLine("    <div class=\"loading\">Loading...</div>");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        private void BuildDataTable(StringBuilder sb)
        {
            sb.AppendLine("  @if (!isLoading() || dataList().length > 0) {");
            
            if (_renderer.RequiresSpecialTableRendering())
            {
                sb.AppendLine("    <div class=\"mat-elevation-z2\">");
                BuildMaterialTable(sb);
                sb.AppendLine("    </div>");
            }
            else
            {
                sb.AppendLine("    <div class=\"table-responsive\">");
                BuildStandardTable(sb);
                sb.AppendLine("    </div>");
            }
            
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        private void BuildStandardTable(StringBuilder sb)
        {
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";

            sb.AppendLine($"      <table class=\"{_renderer.GetTableClass()}\">");
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
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                sb.AppendLine("            <th class=\"text-center\">Actions</th>");
            }
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
                    sb.AppendLine($"                  <span class=\"{_renderer.GetBadgeClass(false)}\">Inactive</span>");
                    sb.AppendLine("                } @else {");
                    sb.AppendLine($"                  <span class=\"{_renderer.GetBadgeClass(true)}\">Active</span>");
                    sb.AppendLine("                }");
                    sb.AppendLine("              </td>");
                }
            }
            
            // Actions
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                sb.AppendLine("              <td class=\"text-center action-col\">");
            }
            
            if (_definition.IsGet && _definition.IsGetById)
            {
                sb.AppendLine($"                <button class=\"{_renderer.GetButtonClass("info")}\" (click)=\"openDetail(item)\">View</button>");
            }
            
            if (_definition.IsUpdate)
            {
                sb.AppendLine($"                <button class=\"{_renderer.GetButtonClass("warning")}\" (click)=\"openEdit(item)\">Edit</button>");
            }
            
            if (_definition.IsDelete)
            {
                sb.AppendLine($"                <button class=\"{_renderer.GetButtonClass("danger")}\" (click)=\"onDelete(item)\">Delete</button>");
            }
            if (_definition.IsGetById || _definition.IsUpdate || _definition.IsDelete)
            {
            sb.AppendLine("              </td>");
            }
            
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
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
            var hasCheckbox = _definition.Fields.Any(f => f.UIControl == ControlType.Checkbox);
            
            sb.AppendLine("    <table mat-table [dataSource]=\"filteredList()\" class=\"w-100\">");
            sb.AppendLine();

            // Primary Key Column
            sb.AppendLine($"      <!-- Column: {primaryKey} -->");
            sb.AppendLine($"      <ng-container matColumnDef=\"{primaryKey}\">");
            sb.AppendLine("        <th mat-header-cell *matHeaderCellDef mat-sort-header>ID</th>");
            sb.AppendLine($"        <td mat-cell *matCellDef=\"let item\">{{{{ item.{primaryKey} }}}}</td>");
            sb.AppendLine("      </ng-container>");
            sb.AppendLine();

            // Generate other columns (exclude PK and checkbox)
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey && f.UIControl != ControlType.Checkbox))
            {
                sb.AppendLine($"      <!-- Column: {field.FieldName} -->");
                sb.AppendLine($"      <ng-container matColumnDef=\"{field.FieldName}\">");
                sb.AppendLine($"        <th mat-header-cell *matHeaderCellDef mat-sort-header>{field.Label}</th>");
                sb.AppendLine($"        <td mat-cell *matCellDef=\"let item\">{{{{ item.{field.FieldName} }}}}</td>");
                sb.AppendLine("      </ng-container>");
                sb.AppendLine();
            }

            // Status column (for checkbox fields)
            if (hasCheckbox)
            {
                var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                if (checkboxField != null)
                {
                    sb.AppendLine("      <!-- Status Column -->");
                    sb.AppendLine("      <ng-container matColumnDef=\"status\">");
                    sb.AppendLine("        <th mat-header-cell *matHeaderCellDef>Status</th>");
                    sb.AppendLine("        <td mat-cell *matCellDef=\"let item\">");
                    sb.AppendLine($"          @if (item.{checkboxField.FieldName}) {{");
                    sb.AppendLine("            <mat-chip color=\"warn\">Inactive</mat-chip>");
                    sb.AppendLine("          } @else {");
                    sb.AppendLine("            <mat-chip color=\"primary\">Active</mat-chip>");
                    sb.AppendLine("          }");
                    sb.AppendLine("        </td>");
                    sb.AppendLine("      </ng-container>");
                    sb.AppendLine();
                }
            }

            // Actions column
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                sb.AppendLine("      <!-- Actions Column -->");
                sb.AppendLine("      <ng-container matColumnDef=\"actions\">");
                sb.AppendLine("        <th mat-header-cell *matHeaderCellDef>Actions</th>");
                sb.AppendLine("        <td mat-cell *matCellDef=\"let item\">");
                
                if (_definition.IsGetById)
                {
                    sb.AppendLine("          <button mat-icon-button color=\"primary\" (click)=\"openDetail(item)\" matTooltip=\"View Details\">");
                    sb.AppendLine("            <mat-icon>visibility</mat-icon>");
                    sb.AppendLine("          </button>");
                }
                
                if (_definition.IsUpdate)
                {
                    sb.AppendLine("          <button mat-icon-button color=\"accent\" (click)=\"openEdit(item)\" matTooltip=\"Edit\">");
                    sb.AppendLine("            <mat-icon>edit</mat-icon>");
                    sb.AppendLine("          </button>");
                }
                
                if (_definition.IsDelete)
                {
                    sb.AppendLine("          <button mat-icon-button color=\"warn\" (click)=\"onDelete(item)\" matTooltip=\"Delete\">");
                    sb.AppendLine("            <mat-icon>delete</mat-icon>");
                    sb.AppendLine("          </button>");
                }
                
                sb.AppendLine("        </td>");
                sb.AppendLine("      </ng-container>");
                sb.AppendLine();
            }

            // No data row
            sb.AppendLine("      <!-- No Data Row -->");
            sb.AppendLine("      <tr class=\"mat-row\" *matNoDataRow>");
            sb.AppendLine("        <td class=\"mat-cell\" [attr.colspan]=\"displayedColumns.length\">");
            sb.AppendLine("          <div class=\"no-data\">No data found</div>");
            sb.AppendLine("        </td>");
            sb.AppendLine("      </tr>");
            sb.AppendLine();

            sb.AppendLine("      <tr mat-header-row *matHeaderRowDef=\"displayedColumns\"></tr>");
            sb.AppendLine("      <tr mat-row *matRowDef=\"let row; columns: displayedColumns\"></tr>");
            sb.AppendLine("    </table>");
        }

        private void BuildModal(StringBuilder sb)
        {
            if (!_definition.IsPost && !_definition.IsUpdate && !_definition.IsGetById) return;

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
            
            // Other input types - with framework-specific styling
            sb.AppendLine("                } @else {");
            sb.AppendLine("                  " + _renderer.RenderFormInput("field.key", "field.type", null).Replace("\n", "\n                  "));
            
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
            sb.AppendLine("            @if(isViewMode()) {");
            sb.AppendLine("              <button type=\"button\" class=\"btn btn-sm btn-edit\" (click)=\"enableEditMode()\">Edit</button>");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            @if(!isViewMode()) {");
            sb.AppendLine($"              <button type=\"submit\" class=\"btn btn-success\" [disabled]=\"{_definition.EntityName.ToLower()}Form.invalid\">");
            sb.AppendLine("                {{ isEditMode() ? 'Update' : 'Create' }}");
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
