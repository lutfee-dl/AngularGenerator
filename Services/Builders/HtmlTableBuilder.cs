using AngularGenerator.Core.Models;
using AngularGenerator.Services.Builders.Base;
using AngularGenerator.Services.Builders.Strategies;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder for Table View HTML - Now using OOP inheritance
    /// Extends BaseHtmlBuilder to eliminate code duplication
    /// Uses Strategy Pattern for framework-specific rendering
    /// </summary>
    public class HtmlTableBuilder : BaseHtmlBuilder
    {
        private readonly ICssFrameworkRenderer _renderer;
        
        public HtmlTableBuilder(ComponentDefinition definition) : base(definition)
        {
            _renderer = CssRendererFactory.Create(definition.CssFramework);
        }

        /// <summary>
        /// Implements abstract method from BaseHtmlBuilder
        /// Builds the table-specific data section
        /// </summary>
        protected override void BuildDataSection()
        {
            _sb.AppendLine("  @if (!isLoading() || dataList().length > 0) {");
            
            if (_renderer.RequiresSpecialTableRendering())
            {
                _sb.AppendLine("    <div class=\"mat-elevation-z2\">");
                BuildMaterialTable();
                _sb.AppendLine("    </div>");
            }
            else
            {
                _sb.AppendLine("    <div class=\"table-responsive\">");
                BuildStandardTable();
                _sb.AppendLine("    </div>");
            }
            
            _sb.AppendLine("  }");
            _sb.AppendLine();
        }

        private void BuildStandardTable()
        {
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";

            _sb.AppendLine("  <div class=\"card shadow-sm border-0\">");
            _sb.AppendLine("    <div class=\"table-responsive\" style=\"max-height: 80vh;\">");
            _sb.AppendLine($"      <table class=\"table table-hover table-striped align-middle m-0\">");
            _sb.AppendLine("        <thead class=\"table-info bg-light sticky-top\" style=\"z-index: 1020;\">");
            _sb.AppendLine("          <tr>");
            
            // PK Column with sort - Sticky Left
            _sb.AppendLine($"            <th class=\"ps-4 position-sticky start-0\" style=\"width: 80px; z-index: 1021;\">ID</th>");
            
            // Other columns (exclude checkbox fields from columns)
            _sb.AppendLine("            @for (field of formFields; track field.key) {");
            _sb.AppendLine("              @if (field.type !== 'checkbox') {");
            _sb.AppendLine("                <th class=\"text-nowrap\" style=\"cursor: pointer;\" (click)=\"onSort(field.key)\">");
            _sb.AppendLine("                  {{ field.label }}");
            _sb.AppendLine("                  @if(sortColumn() === field.key) {");
            _sb.AppendLine("                    <span>{{ sortDirection() === 'asc' ? '▲' : '▼' }}</span>");
            _sb.AppendLine("                  }");
            _sb.AppendLine("                </th>");
            _sb.AppendLine("              }");
            _sb.AppendLine("            }");
            
            // Status column (for checkbox fields)
            var hasCheckbox = _definition.Fields.Any(f => f.UIControl == ControlType.Checkbox);
            if (hasCheckbox)
            {
                _sb.AppendLine("            <th class=\"text-center\">Status</th>");
            }
            
            // Actions column - Sticky Right
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                _sb.AppendLine("            <th class=\"text-center table-striped position-sticky end-0 border-start\" ");
                _sb.AppendLine("                style=\"width: 200px; z-index: 1021;\">Actions</th>");
            }
            _sb.AppendLine("          </tr>");
            _sb.AppendLine("        </thead>");
            _sb.AppendLine("        <tbody>");
            
            // Data rows using filteredList
            _sb.AppendLine($"          @for (item of filteredList(); track item.{primaryKey}) {{");
            _sb.AppendLine("            <tr class=\"bg-white\">");
            _sb.AppendLine($"              <td class=\"ps-4 fw-bold position-sticky start-0 bg-white border-end\">{{{{ item.{primaryKey} }}}}</td>");
            
            // Field values (exclude checkbox)
            _sb.AppendLine("              @for (field of formFields; track field.key) {");
            _sb.AppendLine("                @if (field.type !== 'checkbox') {");
            _sb.AppendLine("                  <td class=\"text-nowrap small text-muted\">");
            _sb.AppendLine("                    {{ $any(item)[field.key] || '-' }}");
            _sb.AppendLine("                  </td>");
            _sb.AppendLine("                }");
            _sb.AppendLine("              }");
            
            // Status badge (for checkbox fields)
            if (hasCheckbox)
            {
                var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                if (checkboxField != null)
                {
                    _sb.AppendLine("              <td class=\"text-center\">");
                    _sb.AppendLine($"                <span class=\"badge rounded-pill {{{{ item.{checkboxField.FieldName} ? 'bg-danger-subtle text-danger' : 'bg-success-subtle text-success' }}}} px-3\">");
                    _sb.AppendLine($"                  {{{{ item.{checkboxField.FieldName} ? 'Inactive' : 'Active' }}}}");
                    _sb.AppendLine("                </span>");
                    _sb.AppendLine("              </td>");
                }
            }
            
            // Actions - Sticky Right
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                _sb.AppendLine("              <td class=\"text-center position-sticky end-0 bg-white border-start shadow-sm\">");
                _sb.AppendLine("                <div class=\"d-flex justify-content-center gap-1\">");
            }
            
            if (_definition.IsGet && _definition.IsGetById)
            {
                _sb.AppendLine("                  <button class=\"btn btn-sm btn-info text-white d-flex align-items-center justify-content-center shadow-sm\" ");
                _sb.AppendLine("                          (click)=\"openDetail(item)\" title=\"View Detail\">");
                _sb.AppendLine("                    <span class=\"fs-6\">&#128065;</span>");
                _sb.AppendLine("                    <span class=\"d-none d-md-inline ms-1\">View</span>");
                _sb.AppendLine("                  </button>");
            }
            
            if (_definition.IsUpdate)
            {
                _sb.AppendLine("                  <button class=\"btn btn-sm btn-warning d-flex align-items-center justify-content-center shadow-sm\" ");
                _sb.AppendLine("                          (click)=\"openEdit(item)\" title=\"Edit\">");
                _sb.AppendLine("                    <span class=\"fs-6\">&#9998;</span>");
                _sb.AppendLine("                    <span class=\"d-none d-md-inline ms-1\">Edit</span>");
                _sb.AppendLine("                  </button>");
            }
            
            if (_definition.IsDelete)
            {
                _sb.AppendLine("                  <button class=\"btn btn-sm btn-danger d-flex align-items-center justify-content-center shadow-sm\" ");
                _sb.AppendLine("                          (click)=\"onDelete(item)\" title=\"Delete\">");
                _sb.AppendLine("                    <span class=\"fs-6\">&#128465;</span>");
                _sb.AppendLine("                    <span class=\"d-none d-md-inline ms-1\">Del</span>");
                _sb.AppendLine("                  </button>");
            }
            if (_definition.IsGetById || _definition.IsUpdate || _definition.IsDelete)
            {
                _sb.AppendLine("                </div>");
                _sb.AppendLine("              </td>");
            }
            
            _sb.AppendLine("            </tr>");
            _sb.AppendLine("          }");
            
            _sb.AppendLine("        </tbody>");
            _sb.AppendLine("      </table>");
            _sb.AppendLine("    </div>");
            
            // Add Pagination Footer
            BuildPaginationFooter();
            
            _sb.AppendLine("  </div>");
        }

        private void BuildMaterialTable()
        {
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
            var hasCheckbox = _definition.Fields.Any(f => f.UIControl == ControlType.Checkbox);
            
            _sb.AppendLine("    <table mat-table [dataSource]=\"filteredList()\" class=\"w-100\">");
            _sb.AppendLine();

            // Primary Key Column
            _sb.AppendLine($"      <!-- Column: {primaryKey} -->");
            _sb.AppendLine($"      <ng-container matColumnDef=\"{primaryKey}\">");
            _sb.AppendLine("        <th mat-header-cell *matHeaderCellDef mat-sort-header>ID</th>");
            _sb.AppendLine($"        <td mat-cell *matCellDef=\"let item\">{{{{ item.{primaryKey} }}}}</td>");
            _sb.AppendLine("      </ng-container>");
            _sb.AppendLine();

            // Generate other columns (exclude PK and checkbox)
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey && f.UIControl != ControlType.Checkbox))
            {
                _sb.AppendLine($"      <!-- Column: {field.FieldName} -->");
                _sb.AppendLine($"      <ng-container matColumnDef=\"{field.FieldName}\">");
                _sb.AppendLine($"        <th mat-header-cell *matHeaderCellDef mat-sort-header>{field.Label}</th>");
                _sb.AppendLine($"        <td mat-cell *matCellDef=\"let item\">{{{{ item.{field.FieldName} }}}}</td>");
                _sb.AppendLine("      </ng-container>");
                _sb.AppendLine();
            }

            // Status column (for checkbox fields)
            if (hasCheckbox)
            {
                var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                if (checkboxField != null)
                {
                    _sb.AppendLine("      <!-- Status Column -->");
                    _sb.AppendLine("      <ng-container matColumnDef=\"status\">");
                    _sb.AppendLine("        <th mat-header-cell *matHeaderCellDef>Status</th>");
                    _sb.AppendLine("        <td mat-cell *matCellDef=\"let item\">");
                    _sb.AppendLine($"          @if (item.{checkboxField.FieldName}) {{");
                    _sb.AppendLine("            <mat-chip color=\"warn\">Inactive</mat-chip>");
                    _sb.AppendLine("          } @else {");
                    _sb.AppendLine("            <mat-chip color=\"primary\">Active</mat-chip>");
                    _sb.AppendLine("          }");
                    _sb.AppendLine("        </td>");
                    _sb.AppendLine("      </ng-container>");
                    _sb.AppendLine();
                }
            }

            // Actions column
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                _sb.AppendLine("      <!-- Actions Column -->");
                _sb.AppendLine("      <ng-container matColumnDef=\"actions\">");
                _sb.AppendLine("        <th mat-header-cell *matHeaderCellDef>Actions</th>");
                _sb.AppendLine("        <td mat-cell *matCellDef=\"let item\">");
                
                if (_definition.IsGetById)
                {
                    _sb.AppendLine("          <button mat-icon-button color=\"primary\" (click)=\"openDetail(item)\" matTooltip=\"View Details\">");
                    _sb.AppendLine("            <mat-icon>visibility</mat-icon>");
                    _sb.AppendLine("          </button>");
                }
                
                if (_definition.IsUpdate)
                {
                    _sb.AppendLine("          <button mat-icon-button color=\"accent\" (click)=\"openEdit(item)\" matTooltip=\"Edit\">");
                    _sb.AppendLine("            <mat-icon>edit</mat-icon>");
                    _sb.AppendLine("          </button>");
                }
                
                if (_definition.IsDelete)
                {
                    _sb.AppendLine("          <button mat-icon-button color=\"warn\" (click)=\"onDelete(item)\" matTooltip=\"Delete\">");
                    _sb.AppendLine("            <mat-icon>delete</mat-icon>");
                    _sb.AppendLine("          </button>");
                }
                
                _sb.AppendLine("        </td>");
                _sb.AppendLine("      </ng-container>");
                _sb.AppendLine();
            }

            // No data row
            _sb.AppendLine("      <!-- No Data Row -->");
            _sb.AppendLine("      <tr class=\"mat-row\" *matNoDataRow>");
            _sb.AppendLine("        <td class=\"mat-cell\" [attr.colspan]=\"displayedColumns.length\">");
            _sb.AppendLine("          <div class=\"no-data\">No data found</div>");
            _sb.AppendLine("        </td>");
            _sb.AppendLine("      </tr>");
            _sb.AppendLine();

            _sb.AppendLine("      <tr mat-header-row *matHeaderRowDef=\"displayedColumns\"></tr>");
            _sb.AppendLine("      <tr mat-row *matRowDef=\"let row; columns: displayedColumns\"></tr>");
            _sb.AppendLine("    </table>");
        }
    }
}
