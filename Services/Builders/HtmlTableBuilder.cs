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
                BuildMaterialTable();
                BuildPaginationFooter();
            }
            else if (_definition.CssFramework == CSSFramework.BasicCSS)
            {
                BuildBasicCssTable();
            }
            else
            {
                _sb.AppendLine("    <div class=\"table-responsive\">");
                BuildStandardTable();
                _sb.AppendLine("    </div>");
            }
            
            _sb.AppendLine("  }");
            
            if (_renderer.RequiresSpecialTableRendering())
            {
                _sb.AppendLine("  </mat-card>");
            }
            
            _sb.AppendLine();
        }

        private void BuildBasicCssTable()
        {
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
            var hasCheckbox = _definition.Fields.Any(f => f.UIControl == ControlType.Checkbox);

            _sb.AppendLine("    <div class=\"table-responsive\">");
            _sb.AppendLine("      <table class=\"table\">");
            _sb.AppendLine("        <thead>");
            _sb.AppendLine("          <tr>");
            
            // PK Column
            _sb.AppendLine($"            <th class=\"sortable\" (click)=\"onSort('{primaryKey}')\">");
            _sb.AppendLine($"              ID");
            _sb.AppendLine($"              @if(sortColumn() === '{primaryKey}') {{ <span class=\"sort-arrow\">{{{{ sortDirection() === 'asc' ? '▲' : '▼' }}}}</span> }}");
            _sb.AppendLine("            </th>");

            // Other columns
            _sb.AppendLine("            @for (field of formFields; track field.key) {");
            _sb.AppendLine("              @if (field.type !== 'checkbox') {");
            _sb.AppendLine("                <th class=\"sortable\" (click)=\"onSort(field.key)\">");
            _sb.AppendLine("                  {{ field.label }}");
            _sb.AppendLine("                  @if(sortColumn() === field.key) {");
            _sb.AppendLine("                    <span class=\"sort-arrow\">{{ sortDirection() === 'asc' ? '▲' : '▼' }}</span>");
            _sb.AppendLine("                  }");
            _sb.AppendLine("                </th>");
            _sb.AppendLine("              }");
            _sb.AppendLine("            }");

            // Status column
            if (hasCheckbox)
            {
                _sb.AppendLine("            <th>Status</th>");
            }

            // Actions column
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                _sb.AppendLine("            <th>Actions</th>");
            }
            _sb.AppendLine("          </tr>");
            _sb.AppendLine("        </thead>");
            _sb.AppendLine("        <tbody>");
            
            // Data rows
            _sb.AppendLine($"          @for (item of filteredList(); track item.{primaryKey}) {{");
            _sb.AppendLine("            <tr>");
            _sb.AppendLine($"              <td>{{{{ item.{primaryKey} }}}}</td>");

            _sb.AppendLine("              @for (field of formFields; track field.key) {");
            _sb.AppendLine("                @if (field.type !== 'checkbox') {");
            _sb.AppendLine("                  <td>");
            _sb.AppendLine("                    {{ $any(item)[field.key] || '-' }}");
            _sb.AppendLine("                  </td>");
            _sb.AppendLine("                }");
            _sb.AppendLine("              }");

            // Status badge
            if (hasCheckbox)
            {
                var checkboxField = _definition.Fields.FirstOrDefault(f => f.UIControl == ControlType.Checkbox);
                if (checkboxField != null)
                {
                    _sb.AppendLine("              <td class=\"text-center\">");
                    _sb.AppendLine($"                <span class=\"badge\" [class.badge-active]=\"!item.{checkboxField.FieldName}\" [class.badge-inactive]=\"item.{checkboxField.FieldName}\">");
                    _sb.AppendLine($"                  {{{{ item.{checkboxField.FieldName} ? 'Inactive' : 'Active' }}}}");
                    _sb.AppendLine("                </span>");
                    _sb.AppendLine("              </td>");
                }
            }

            // Actions
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                _sb.AppendLine("              <td class=\"action-col\">");
                
                if (_definition.IsGetById)
                {
                    _sb.AppendLine("                  <button class=\"btn btn-info\" (click)=\"openDetail(item)\">View</button>");
                }
                
                if (_definition.IsUpdate)
                {
                    _sb.AppendLine("                  <button class=\"btn btn-edit\" (click)=\"openEdit(item)\">Edit</button>");
                }
                
                if (_definition.IsDelete)
                {
                    _sb.AppendLine("                  <button class=\"btn btn-delete\" (click)=\"onDelete(item)\">Delete</button>");
                }
                
                _sb.AppendLine("              </td>");
            }

            _sb.AppendLine("            </tr>");
            _sb.AppendLine("          }");

            _sb.AppendLine("        </tbody>");
            _sb.AppendLine("      </table>");
            _sb.AppendLine("    </div>");
            
            // Pagination
            BuildPaginationFooter();
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
            
            _sb.AppendLine("    <div class=\"mat-elevation-z2\">");
            _sb.AppendLine("    <div style=\"overflow-x: auto; max-height: 85vh;\">");
            _sb.AppendLine($"      <table mat-table [dataSource]=\"filteredList()\" matSort style=\"width: 100%;\">");
            _sb.AppendLine();
            
            // Dynamic Columns Loop
            _sb.AppendLine("        <!-- Dynamic Columns -->");
            _sb.AppendLine("        @for (col of formFields; track col.key) {");
            _sb.AppendLine("          <ng-container [matColumnDef]=\"col.key\" [sticky]=\"col.key === '" + primaryKey + "'\">");
            _sb.AppendLine("            <th mat-header-cell *matHeaderCellDef mat-sort-header");
            _sb.AppendLine($"                [style.padding-left]=\"col.key === '{primaryKey}' ? '16px' : '12px'\"");
            _sb.AppendLine($"                [style.border-right]=\"col.key === '{primaryKey}' ? '1px solid #e0e0e0' : 'none'\"");
            _sb.AppendLine($"                [style.z-index]=\"col.key === '{primaryKey}' ? 3 : 'auto'\"");
            _sb.AppendLine("                style=\"background: #fafafa; white-space: nowrap; padding: 8px 12px; font-size: 13px;\">");
            _sb.AppendLine("              {{ col.label }}");
            _sb.AppendLine("            </th>");
            _sb.AppendLine("            <td mat-cell *matCellDef=\"let item\"");
            _sb.AppendLine($"                [style.padding-left]=\"col.key === '{primaryKey}' ? '16px' : '12px'\"");
            _sb.AppendLine($"                [style.font-weight]=\"col.key === '{primaryKey}' ? '600' : 'normal'\"");
            _sb.AppendLine($"                [style.color]=\"col.key === '{primaryKey}' ? '#212121' : '#666'\"");
            _sb.AppendLine($"                [style.background]=\"col.key === '{primaryKey}' ? 'white' : 'inherit'\"");
            _sb.AppendLine($"                [style.border-right]=\"col.key === '{primaryKey}' ? '1px solid #e0e0e0' : 'none'\"");
            _sb.AppendLine("                style=\"white-space: nowrap; font-size: 12px; padding: 8px 12px;\">");
            
            // Handle specific field types if needed, otherwise default display
            _sb.AppendLine("              {{ item[col.key] || '-' }}");
            
            _sb.AppendLine("            </td>");
            _sb.AppendLine("          </ng-container>");
            _sb.AppendLine("        }");
            _sb.AppendLine();

            // Actions Column
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                _sb.AppendLine("        <!-- Actions Column -->");
                _sb.AppendLine("        <ng-container matColumnDef=\"actions\" stickyEnd>");
                _sb.AppendLine("          <th mat-header-cell *matHeaderCellDef style=\"background: #fafafa; text-align: center; padding: 8px 12px; font-size: 13px; width: 100px;\">Actions</th>");
                _sb.AppendLine("          <td mat-cell *matCellDef=\"let item\" style=\"text-align: center; padding: 8px 12px;\" (click)=\"$event.stopPropagation()\">");
                
                if (_definition.IsUpdate)
                {
                    _sb.AppendLine("            <button mat-icon-button color=\"primary\" (click)=\"openEdit(item)\" matTooltip=\"Edit\">");
                    _sb.AppendLine("              <mat-icon>edit</mat-icon>");
                    _sb.AppendLine("            </button>");
                }
                
                if (_definition.IsDelete)
                {
                    _sb.AppendLine("            <button mat-icon-button color=\"warn\" (click)=\"onDelete(item)\" matTooltip=\"Delete\">");
                    _sb.AppendLine("              <mat-icon>delete</mat-icon>");
                    _sb.AppendLine("            </button>");
                }
                
                _sb.AppendLine("          </td>");
                _sb.AppendLine("        </ng-container>");
            }
            
            // No Data Row
            _sb.AppendLine("        <!-- No Data Row -->");
            _sb.AppendLine("        <tr class=\"mat-row\" *matNoDataRow>");
            _sb.AppendLine("          <td class=\"mat-cell\" [attr.colspan]=\"displayedColumns.length\" style=\"text-align: center; padding: 60px;\">");
            _sb.AppendLine("            <mat-icon style=\"font-size: 64px; width: 64px; height: 64px; color: #bdbdbd;\">search_off</mat-icon>");
            _sb.AppendLine("            <p style=\"color: #757575; margin-top: 16px; font-size: 16px;\">No data found</p>");
            _sb.AppendLine("          </td>");
            _sb.AppendLine("        </tr>");

            _sb.AppendLine();
            _sb.AppendLine("        <tr mat-header-row *matHeaderRowDef=\"displayedColumns; sticky: true\"></tr>");
            if (_definition.IsGetById)
            {
                _sb.AppendLine("        <tr mat-row *matRowDef=\"let row; columns: displayedColumns\" class=\"hover-row\" (click)=\"openDetail(row)\"></tr>");
            }
            else
            {
                _sb.AppendLine("        <tr mat-row *matRowDef=\"let row; columns: displayedColumns\" class=\"hover-row\"></tr>");
            }
            _sb.AppendLine("      </table>");
            _sb.AppendLine("    </div>");
            _sb.AppendLine("    </div>");
        }

        protected override void BuildContainer()
        {
            if (_renderer.RequiresSpecialTableRendering())
            {
                 _sb.AppendLine("<div style=\"padding: 16px; background: #f5f5f5; min-height: 100vh;\">");
            }
            else if (_definition.CssFramework == CSSFramework.BasicCSS)
            {
                _sb.AppendLine("<div class=\"container\">");
            }
            else
            {
                base.BuildContainer();
            }
        }

        protected override void BuildPaginationFooter()
        {
            if (_renderer.RequiresSpecialTableRendering())
            {
                _sb.AppendLine("    <mat-paginator [pageSizeOptions]=\"[5, 10, 25, 100]\"");
                _sb.AppendLine("                   showFirstLastButtons");
                _sb.AppendLine("                   aria-label=\"Select page of periodic elements\">");
                _sb.AppendLine("    </mat-paginator>");
            }
            else if (_definition.CssFramework == CSSFramework.BasicCSS)
            {
                _sb.AppendLine("    <div class=\"pagination-container\" style=\"margin-top: 20px; display: flex; justify-content: space-between; align-items: center;\">");
                _sb.AppendLine("      <div>");
                _sb.AppendLine("        Show ");
                _sb.AppendLine("        <select [ngModel]=\"pageSize()\" (ngModelChange)=\"setPageSize($any($event))\" style=\"padding: 5px;\">");
                _sb.AppendLine("          <option [value]=\"10\">10</option>");
                _sb.AppendLine("          <option [value]=\"20\">20</option>");
                _sb.AppendLine("          <option [value]=\"50\">50</option>");
                _sb.AppendLine("          <option [value]=\"100\">100</option>");
                _sb.AppendLine("        </select>");
                _sb.AppendLine("        entries");
                _sb.AppendLine("      </div>");
                
                _sb.AppendLine("      <div class=\"pagination-controls\">");
                _sb.AppendLine("        <button class=\"btn\" [disabled]=\"currentPage() === 1\" (click)=\"setPage(currentPage() - 1)\">Previous</button>");
                _sb.AppendLine("        <span style=\"margin: 0 10px;\"> Page {{ currentPage() }} of {{ totalPages() }} </span>");
                _sb.AppendLine("        <button class=\"btn\" [disabled]=\"currentPage() === totalPages()\" (click)=\"setPage(currentPage() + 1)\">Next</button>");
                _sb.AppendLine("      </div>");
                _sb.AppendLine("    </div>");
            }
            else
            {
                base.BuildPaginationFooter();
            }
        }

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
                base.BuildLoadingIndicator();
            }
        }

        protected override void BuildHeader()
        {
            if (_renderer.RequiresSpecialTableRendering())
            {
                _sb.AppendLine("  <!-- Header Card -->");
                _sb.AppendLine("  <mat-card style=\"margin-bottom: 16px;\">");
                _sb.AppendLine("    <mat-card-content style=\"padding: 12px 16px;\">");
                _sb.AppendLine("      <div style=\"display: flex; justify-content: space-between; align-items: center;\">");
                _sb.AppendLine($"        <h3 style=\"margin: 0; font-weight: 600; color: #212121;\">{_definition.EntityName} Management</h3>");
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
                
                // Open result card
                _sb.AppendLine("  <!-- Table Card -->");
                _sb.AppendLine("  <mat-card>");
            }
            else if (_definition.CssFramework == CSSFramework.BasicCSS)
            {
                _sb.AppendLine("  <div class=\"header-section\">");
                _sb.AppendLine($"    <h3>{_definition.EntityName} List</h3>");
                _sb.AppendLine("    <div class=\"header-actions\">");
                _sb.AppendLine("      <input type=\"text\" class=\"search-box\" placeholder=\"Search...\" ");
                _sb.AppendLine("             [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
                
                if (_definition.IsPost)
                {
                    _sb.AppendLine("      <button class=\"btn btn-add\" (click)=\"openCreate()\">");
                    _sb.AppendLine($"        + Add {_definition.EntityName}");
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
    }
}
