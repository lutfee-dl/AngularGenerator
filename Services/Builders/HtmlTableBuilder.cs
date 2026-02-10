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
                        
            return sb.ToString();
        }

        private void BuildHeader(StringBuilder sb)
        {
            sb.AppendLine($"<div class=\"container-fluid px-2 py-2 bg-light min-vh-100\">");
            sb.AppendLine("  <div class=\"card shadow-sm mb-2 border-0 bg-white\">");
            sb.AppendLine("    <div class=\"card-body d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3\">");
            sb.AppendLine($"      <h3 class=\"m-0 fw-bold text-dark\">{_definition.EntityName} Catalog</h3>");
            sb.AppendLine("      <div class=\"d-flex gap-2\">");
            sb.AppendLine("        <input type=\"text\" class=\"form-control\" style=\"width: 300px;\" placeholder=\"Search...\" ");
            sb.AppendLine("               [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
            
            if (_definition.IsPost)
            {
                sb.AppendLine("        <button class=\"btn btn-primary d-flex align-items-center justify-content-center px-4 shadow-sm fw-bold\" (click)=\"openCreate()\">");
                sb.AppendLine($"          <span class=\"me-2 fs-5\">&#10010;</span> Add {_definition.EntityName}");
                sb.AppendLine("        </button>");
            }
            
            sb.AppendLine("      </div>");
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

            sb.AppendLine("  <div class=\"card shadow-sm border-0\">");
            sb.AppendLine("    <div class=\"table-responsive\" style=\"max-height: 80vh;\">");
            sb.AppendLine($"      <table class=\"table table-hover table-striped align-middle m-0\">");
            sb.AppendLine("        <thead class=\"table-info bg-light sticky-top\" style=\"z-index: 1020;\">");
            sb.AppendLine("          <tr>");
            
            // PK Column with sort - Sticky Left
            sb.AppendLine($"            <th class=\"ps-4 position-sticky start-0\" style=\"width: 80px; z-index: 1021;\">ID</th>");
            
            // Other columns (exclude checkbox fields from columns)
            sb.AppendLine("            @for (field of formFields; track field.key) {");
            sb.AppendLine("              @if (field.type !== 'checkbox') {");
            sb.AppendLine("                <th class=\"text-nowrap\" style=\"cursor: pointer;\" (click)=\"onSort(field.key)\">");
            sb.AppendLine("                  {{ field.label }}");
            sb.AppendLine("                  @if(sortColumn() === field.key) {");
            sb.AppendLine("                    <span>{{ sortDirection() === 'asc' ? '▲' : '▼' }}</span>");
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
            
            // Actions column - Sticky Right
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                sb.AppendLine("            <th class=\"text-center table-striped position-sticky end-0 border-start\" ");
                sb.AppendLine("                style=\"width: 200px; z-index: 1021;\">Actions</th>");
            }
            sb.AppendLine("          </tr>");
            sb.AppendLine("        </thead>");
            sb.AppendLine("        <tbody>");
            
            // Data rows using filteredList
            sb.AppendLine($"          @for (item of filteredList(); track item.{primaryKey}) {{");
            sb.AppendLine("            <tr class=\"bg-white\">");
            sb.AppendLine($"              <td class=\"ps-4 fw-bold position-sticky start-0 bg-white border-end\">{{{{ item.{primaryKey} }}}}</td>");
            
            // Field values (exclude checkbox)
            sb.AppendLine("              @for (field of formFields; track field.key) {");
            sb.AppendLine("                @if (field.type !== 'checkbox') {");
            sb.AppendLine("                  <td class=\"text-nowrap small text-muted\">");
            sb.AppendLine("                    {{ $any(item)[field.key] || '-' }}");
            sb.AppendLine("                  </td>");
            sb.AppendLine("                }");
            sb.AppendLine("              }");
            
            // Status badge (for checkbox fields)
            if (hasCheckbox)
            {
                var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                if (checkboxField != null)
                {
                    sb.AppendLine("              <td class=\"text-center\">");
                    sb.AppendLine($"                <span class=\"badge rounded-pill {{{{ item.{checkboxField.FieldName} ? 'bg-danger-subtle text-danger' : 'bg-success-subtle text-success' }}}} px-3\">");
                    sb.AppendLine($"                  {{{{ item.{checkboxField.FieldName} ? 'Inactive' : 'Active' }}}}");
                    sb.AppendLine("                </span>");
                    sb.AppendLine("              </td>");
                }
            }
            
            // Actions - Sticky Right
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                sb.AppendLine("              <td class=\"text-center position-sticky end-0 bg-white border-start shadow-sm\">");
                sb.AppendLine("                <div class=\"d-flex justify-content-center gap-1\">");
            }
            
            if (_definition.IsGet && _definition.IsGetById)
            {
                sb.AppendLine("                  <button class=\"btn btn-sm btn-info text-white d-flex align-items-center justify-content-center shadow-sm\" ");
                sb.AppendLine("                          (click)=\"openDetail(item)\" title=\"View Detail\">");
                sb.AppendLine("                    <span class=\"fs-6\">&#128065;</span>");
                sb.AppendLine("                    <span class=\"d-none d-md-inline ms-1\">View</span>");
                sb.AppendLine("                  </button>");
            }
            
            if (_definition.IsUpdate)
            {
                sb.AppendLine("                  <button class=\"btn btn-sm btn-warning d-flex align-items-center justify-content-center shadow-sm\" ");
                sb.AppendLine("                          (click)=\"openEdit(item)\" title=\"Edit\">");
                sb.AppendLine("                    <span class=\"fs-6\">&#9998;</span>");
                sb.AppendLine("                    <span class=\"d-none d-md-inline ms-1\">Edit</span>");
                sb.AppendLine("                  </button>");
            }
            
            if (_definition.IsDelete)
            {
                sb.AppendLine("                  <button class=\"btn btn-sm btn-danger d-flex align-items-center justify-content-center shadow-sm\" ");
                sb.AppendLine("                          (click)=\"onDelete(item)\" title=\"Delete\">");
                sb.AppendLine("                    <span class=\"fs-6\">&#128465;</span>");
                sb.AppendLine("                    <span class=\"d-none d-md-inline ms-1\">Del</span>");
                sb.AppendLine("                  </button>");
            }
            if (_definition.IsGetById || _definition.IsUpdate || _definition.IsDelete)
            {
                sb.AppendLine("                </div>");
                sb.AppendLine("              </td>");
            }
            
            sb.AppendLine("            </tr>");
            sb.AppendLine("          }");
            
            sb.AppendLine("        </tbody>");
            sb.AppendLine("      </table>");
            sb.AppendLine("    </div>");
            
            // Add Pagination Footer
            BuildPaginationFooter(sb);
            
            sb.AppendLine("  </div>");
        }

        private void BuildPaginationFooter(StringBuilder sb)
        {
            sb.AppendLine("    <div class=\"card-footer bg-white border-top py-3\">");
            sb.AppendLine("      <div class=\"d-flex justify-content-between align-items-center flex-wrap gap-3\">");
            sb.AppendLine("    ");
            sb.AppendLine("    <div class=\"d-flex align-items-center gap-3\">");
            sb.AppendLine("      <div class=\"d-flex align-items-center\">");
            sb.AppendLine("        <span class=\"small text-muted me-2 text-nowrap\">Show</span>");
            sb.AppendLine("        <select class=\"form-select form-select-sm shadow-none\" ");
            sb.AppendLine("                style=\"width: 70px;\"");
            sb.AppendLine("                [ngModel]=\"pageSize()\" ");
            sb.AppendLine("                (ngModelChange)=\"setPageSize($any($event))\">");
            sb.AppendLine("          <option [value]=\"10\">10</option>");
            sb.AppendLine("          <option [value]=\"20\">20</option>");
            sb.AppendLine("          <option [value]=\"50\">50</option>");
            sb.AppendLine("          <option [value]=\"100\">100</option>");
            sb.AppendLine("        </select>");
            sb.AppendLine("        <span class=\"small text-muted ms-2 text-nowrap\">entries</span>");
            sb.AppendLine("      </div>");
            sb.AppendLine("      ");
            sb.AppendLine("      <div class=\"small text-muted border-start ps-3 d-none d-md-block\">");
            sb.AppendLine("        Showing page <span class=\"fw-bold text-primary\">{{ currentPage() }}</span> of {{ totalPages() }}");
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    ");
            sb.AppendLine($"      <nav aria-label=\"{_definition.EntityName} Page Navigation\">");
            sb.AppendLine("        <ul class=\"pagination pagination-sm m-0 shadow-sm\">");
            sb.AppendLine("          <li class=\"page-item\" [class.disabled]=\"currentPage() === 1\">");
            sb.AppendLine("            <a class=\"page-link\" role=\"button\" (click)=\"setPage(currentPage() - 1)\">Previous</a>");
            sb.AppendLine("          </li>");
            sb.AppendLine();
            sb.AppendLine("          @for (p of [].constructor(totalPages()); track $index) {");
            sb.AppendLine("            @if (($index + 1) >= (currentPage() - 1) && ($index + 1) <= (currentPage() + 1)) {");
            sb.AppendLine("              <li class=\"page-item\" [class.active]=\"currentPage() === ($index + 1)\">");
            sb.AppendLine("                <a class=\"page-link\" role=\"button\" (click)=\"setPage($index + 1)\">");
            sb.AppendLine("                  {{ $index + 1 }}");
            sb.AppendLine("                </a>");
            sb.AppendLine("              </li>");
            sb.AppendLine("            }");
            sb.AppendLine("          }");
            sb.AppendLine();
            sb.AppendLine("          <li class=\"page-item\" [class.disabled]=\"currentPage() === totalPages() || totalPages() === 0\">");
            sb.AppendLine("            <a class=\"page-link\" role=\"button\" (click)=\"setPage(currentPage() + 1)\">Next</a>");
            sb.AppendLine("          </li>");
            sb.AppendLine("        </ul>");
            sb.AppendLine("      </nav>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</div>");
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
            sb.AppendLine("@if (showModal()) {");
            sb.AppendLine("<div class=\"modal fade show\" tabindex=\"-1\" style=\"display: block; background: rgba(0,0,0,0.5); z-index: 1060;\">");
            sb.AppendLine("  <div class=\"modal-dialog modal-lg modal-dialog-centered\">");
            sb.AppendLine("    <div class=\"modal-content shadow-lg border-0\">");
            sb.AppendLine("      ");
            sb.AppendLine("      <div class=\"modal-header bg-light py-3\">");
            sb.AppendLine("        <h5 class=\"modal-title fw-bold\">");
            sb.AppendLine("          <i class=\"bi bi-box-seam me-2\"></i>");
            sb.AppendLine($"          @if(isViewMode()) {{ View {_definition.EntityName} Detail }}");
            sb.AppendLine($"          @else if(isEditMode()) {{ Edit {_definition.EntityName} Information }}");
            sb.AppendLine($"          @else {{ Create New {_definition.EntityName} }}");
            sb.AppendLine("        </h5>");
            sb.AppendLine("        <button type=\"button\" class=\"btn-close\" (click)=\"onClose()\"></button>");
            sb.AppendLine("      </div>");
            sb.AppendLine();
            
            // Form
            sb.AppendLine($"      <form [formGroup]=\"{_definition.EntityName.ToLower()}Form\" (ngSubmit)=\"onSubmit()\">");
            sb.AppendLine("        <div class=\"modal-body p-4\">");
            sb.AppendLine("          <div class=\"row g-3\"> @for (field of formFields; track field.key) {");
            sb.AppendLine("              <div [class]=\"field.type === 'checkbox' ? 'col-12' : 'col-md-6'\">");
            sb.AppendLine("                <label class=\"form-label fw-semibold mb-1\">");
            sb.AppendLine("                  {{ field.label }}");
            sb.AppendLine("                  @if(field.required && !isViewMode()) { <span class=\"text-danger\">*</span> }");
            sb.AppendLine("                </label>");
            sb.AppendLine();
            
            // Checkbox field
            sb.AppendLine("                @if (field.type === 'checkbox') {");
            sb.AppendLine("                  <div class=\"form-check form-switch border rounded p-2 ps-5\">");
            sb.AppendLine("                    <input type=\"checkbox\" class=\"form-check-input\" [formControlName]=\"field.key\" [id]=\"field.key\">");
            sb.AppendLine("                    <label class=\"form-check-label text-muted\" [for]=\"field.key\">Enable this option</label>");
            sb.AppendLine("                  </div>");
            
            // Other input types
            sb.AppendLine("                } @else {");
            sb.AppendLine("                  <input [type]=\"field.type\"");
            sb.AppendLine("                         [formControlName]=\"field.key\"");
            sb.AppendLine("                         class=\"form-control\"");
            sb.AppendLine("                         [class.bg-light]=\"isViewMode()\"");
            sb.AppendLine("                         [readonly]=\"isViewMode()\"");
            sb.AppendLine("                         [placeholder]=\"'Enter ' + field.label\"");
            sb.AppendLine("                         [attr.maxlength]=\"field.maxLength\">");
            sb.AppendLine("                }");
            sb.AppendLine("                ");
            sb.AppendLine($"                @if ({_definition.EntityName.ToLower()}Form.get(field.key)?.touched && {_definition.EntityName.ToLower()}Form.get(field.key)?.invalid) {{");
            sb.AppendLine("                  <small class=\"text-danger\">This field is required.</small>");
            sb.AppendLine("                }");
            sb.AppendLine("              </div>");
            sb.AppendLine("            }");
            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine();
            
            // Modal Actions
            sb.AppendLine("        <div class=\"modal-footer bg-light border-0 px-4\">");
            sb.AppendLine("          <button type=\"button\" class=\"btn btn-outline-secondary px-4\" (click)=\"onClose()\">");
            sb.AppendLine("            {{ isViewMode() ? 'Close' : 'Cancel' }}");
            sb.AppendLine("          </button>");
            sb.AppendLine();
            sb.AppendLine("          @if(isViewMode()) {");
            sb.AppendLine("            <button type=\"button\" class=\"btn btn-primary px-4\" (click)=\"enableEditMode()\">");
            sb.AppendLine("              <i class=\"bi bi-pencil-square me-1\"></i> Edit");
            sb.AppendLine("            </button>");
            sb.AppendLine("          }");
            sb.AppendLine();
            sb.AppendLine("          @if(!isViewMode()) {");
            sb.AppendLine($"            <button type=\"submit\" class=\"btn btn-success px-4\" [disabled]=\"{_definition.EntityName.ToLower()}Form.invalid\">");
            sb.AppendLine("              <i class=\"bi bi-check-lg me-1\"></i>");
            sb.AppendLine($"              {{{{ isEditMode() ? 'Update Changes' : 'Create {_definition.EntityName}' }}}}");
            sb.AppendLine("            </button>");
            sb.AppendLine("          }");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </form>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");
            sb.AppendLine("}");
        }
    }
}
