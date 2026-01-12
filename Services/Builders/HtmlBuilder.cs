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
            BuildDetailModal(sb, framework);
            
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
                _ => "btn btn-primary"
            };

            sb.AppendLine($"<div class=\"{containerClass}\">");
            sb.AppendLine("  <div class=\"header\">");
            sb.AppendLine($"    <h2>{_definition.EntityName} Management</h2>");
            
            if (_definition.IsPost)
            {
                sb.AppendLine($"    <button class=\"{buttonClass}\" (click)=\"openCreate()\">");
                
                if (framework == CSSFramework.AngularMaterial)
                    sb.AppendLine("      <mat-icon>add</mat-icon>");
                
                sb.AppendLine("      Create New");
                sb.AppendLine("    </button>");
            }
            
            sb.AppendLine("  </div>");
            sb.AppendLine();
        }

        private void BuildLoadingIndicator(StringBuilder sb, CSSFramework framework)
        {
            sb.AppendLine("  <!-- Loading Indicator -->");
            sb.AppendLine("  @if (isLoading()) {");
            
            if (framework == CSSFramework.AngularMaterial)
            {
                sb.AppendLine("    <div class=\"loading-container\">");
                sb.AppendLine("      <mat-spinner></mat-spinner>");
                sb.AppendLine("    </div>");
            }
            else if (framework == CSSFramework.Bootstrap)
            {
                sb.AppendLine("    <div class=\"text-center py-4\">");
                sb.AppendLine("      <div class=\"spinner-border text-primary\" role=\"status\">");
                sb.AppendLine("        <span class=\"visually-hidden\">Loading...</span>");
                sb.AppendLine("      </div>");
                sb.AppendLine("    </div>");
            }
            else // BasicCSS
            {
                sb.AppendLine("    <div class=\"loading-container\">");
                sb.AppendLine("      <div class=\"spinner\"></div>");
                sb.AppendLine("    </div>");
            }
            
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        private void BuildDataTable(StringBuilder sb, CSSFramework framework)
        {
            sb.AppendLine("  <!-- Data Table -->");
            sb.AppendLine("  @if (!isLoading()) {");
            
            if (framework == CSSFramework.AngularMaterial)
            {
                BuildMaterialTable(sb);
            }
            else
            {
                BuildStandardTable(sb, framework);
            }
            
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        private void BuildStandardTable(StringBuilder sb, CSSFramework framework)
        {
            var tableClass = framework == CSSFramework.Bootstrap 
                ? "table table-striped table-hover" 
                : "data-table";

            sb.AppendLine($"    <table class=\"{tableClass}\">");
            sb.AppendLine("      <thead>");
            sb.AppendLine("        <tr>");
            sb.AppendLine("          @for (col of columnConfig; track col.field) {");
            sb.AppendLine("            <th>{{ col.label }}</th>");
            sb.AppendLine("          }");
            
            if (_definition.IsUpdate || _definition.IsDelete)
            {
                sb.AppendLine("          <th>Actions</th>");
            }
            
            sb.AppendLine("        </tr>");
            sb.AppendLine("      </thead>");
            sb.AppendLine("      <tbody>");
            
            // Empty state
            sb.AppendLine("        @if (dataList().length === 0) {");
            sb.AppendLine("          <tr>");
            sb.AppendLine("            <td [attr.colspan]=\"columnConfig.length + 1\" class=\"text-center\">No data available</td>");
            sb.AppendLine("          </tr>");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Data rows
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
            sb.AppendLine($"        @for (item of dataList(); track trackByFn($index, item)) {{");
            sb.AppendLine("          <tr (click)=\"viewDetail(item)\" style=\"cursor: pointer\" title=\"Click to view details\">");
            sb.AppendLine("            @for (col of columnConfig; track col.field) {");
            sb.AppendLine("              <td>{{ getFieldValue(item, col.field) }}</td>");
            sb.AppendLine("            }");
            
            if (_definition.IsUpdate || _definition.IsDelete)
            {
                var btnClass = framework == CSSFramework.Bootstrap ? "btn btn-sm" : "btn btn-sm";
                
                sb.AppendLine("            <td (click)=\"$event.stopPropagation()\"> <!-- Prevent row click -->");
                
                if (_definition.IsUpdate)
                {
                    sb.AppendLine($"              <button class=\"{btnClass} btn-warning\" (click)=\"onEdit(item)\">Edit</button>");
                }
                
                if (_definition.IsDelete)
                {
                    sb.AppendLine($"              <button class=\"{btnClass} btn-danger\" (click)=\"onDelete(item.{primaryKey})\">Delete</button>");
                }
                
                sb.AppendLine("            </td>");
            }
            
            sb.AppendLine("          </tr>");
            sb.AppendLine("        }");
            sb.AppendLine("      </tbody>");
            sb.AppendLine("    </table>");
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

        private void BuildDetailModal(StringBuilder sb, CSSFramework framework)
        {
            if (!_definition.IsGet) return;

            sb.AppendLine();
            sb.AppendLine("  <!-- Detail Modal -->");
            sb.AppendLine("  @if (showDetailModal() && selectedItem()) {");
            
            if (framework == CSSFramework.Bootstrap)
            {
                BuildBootstrapDetailModal(sb);
            }
            else if (framework == CSSFramework.AngularMaterial)
            {
                BuildMaterialDetailModal(sb);
            }
            else
            {
                BuildBasicDetailModal(sb);
            }
            
            sb.AppendLine("  }");
        }

        private void BuildBootstrapDetailModal(StringBuilder sb)
        {
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
            
            sb.AppendLine("    <div class=\"modal d-block\" tabindex=\"-1\" style=\"background-color: rgba(0,0,0,0.5);\">" );
            sb.AppendLine("      <div class=\"modal-dialog modal-lg\">");
            sb.AppendLine("        <div class=\"modal-content\">");
            sb.AppendLine("          <div class=\"modal-header bg-primary text-white\">");
            sb.AppendLine($"            <h5 class=\"modal-title\"><i class=\"fas fa-info-circle me-2\"></i>{_definition.EntityName} Details</h5>");
            sb.AppendLine("            <button type=\"button\" class=\"btn-close btn-close-white\" (click)=\"closeDetail()\"></button>");
            sb.AppendLine("          </div>");
            sb.AppendLine("          <div class=\"modal-body\">");
            sb.AppendLine("            <div class=\"row g-3\">");
            
            foreach (var field in _definition.Fields)
            {
                sb.AppendLine("              <div class=\"col-md-6\">");
                sb.AppendLine($"                <label class=\"form-label fw-bold text-muted small\">{field.Label}</label>");
                sb.AppendLine($"                <div class=\"form-control-plaintext border rounded p-2 bg-light\">{{{{ getFieldValue(selectedItem()!, '{field.FieldName}') ?? '-' }}}}</div>");
                sb.AppendLine("              </div>");
            }
            
            sb.AppendLine("            </div>");
            sb.AppendLine("          </div>");
            sb.AppendLine("          <div class=\"modal-footer\">");
            
            if (_definition.IsUpdate)
            {
                sb.AppendLine("            <button type=\"button\" class=\"btn btn-warning\" (click)=\"onEdit(selectedItem()!); closeDetail()\"><i class=\"fas fa-edit me-1\"></i>Edit</button>");
            }
            
            if (_definition.IsDelete)
            {
                sb.AppendLine($"            <button type=\"button\" class=\"btn btn-danger\" (click)=\"onDelete(selectedItem()?.{primaryKey}); closeDetail()\"><i class=\"fas fa-trash me-1\"></i>Delete</button>");
            }
            
            sb.AppendLine("            <button type=\"button\" class=\"btn btn-secondary\" (click)=\"closeDetail()\">Close</button>");
            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
        }

        private void BuildBasicDetailModal(StringBuilder sb)
        {
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
            
            sb.AppendLine("    <div class=\"modal-overlay\">");
            sb.AppendLine("      <div class=\"modal-content detail-modal\">");
            sb.AppendLine("        <div class=\"modal-header\">");
            sb.AppendLine($"          <h3>{_definition.EntityName} Details</h3>");
            sb.AppendLine("          <button class=\"close-btn\" (click)=\"closeDetail()\">&times;</button>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"modal-body\">");
            sb.AppendLine("          <div class=\"detail-grid\">");
            
            foreach (var field in _definition.Fields)
            {
                sb.AppendLine("            <div class=\"detail-item\">");
                sb.AppendLine($"              <label>{field.Label}:</label>");
                sb.AppendLine($"              <span>{{{{ getFieldValue(selectedItem()!, '{field.FieldName}') ?? '-' }}}}</span>");
                sb.AppendLine("            </div>");
            }
            
            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"modal-footer\">");
            
            if (_definition.IsUpdate)
            {
                sb.AppendLine("          <button class=\"btn btn-warning\" (click)=\"onEdit(selectedItem()!); closeDetail()\">Edit</button>");
            }
            
            if (_definition.IsDelete)
            {
                sb.AppendLine($"          <button class=\"btn btn-danger\" (click)=\"onDelete(selectedItem()?.{primaryKey}); closeDetail()\">Delete</button>");
            }
            
            sb.AppendLine("          <button class=\"btn\" (click)=\"closeDetail()\">Close</button>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
        }

        private void BuildMaterialDetailModal(StringBuilder sb)
        {
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
            
            sb.AppendLine("    <div class=\"modal-backdrop\">");
            sb.AppendLine("      <mat-card class=\"detail-card\">");
            sb.AppendLine("        <mat-card-header>");
            sb.AppendLine($"          <mat-card-title>{_definition.EntityName} Details</mat-card-title>");
            sb.AppendLine("          <button mat-icon-button (click)=\"closeDetail()\">");
            sb.AppendLine("            <mat-icon>close</mat-icon>");
            sb.AppendLine("          </button>");
            sb.AppendLine("        </mat-card-header>");
            sb.AppendLine("        <mat-card-content>");
            sb.AppendLine("          <mat-list>");
            
            foreach (var field in _definition.Fields)
            {
                sb.AppendLine("            <mat-list-item>");
                sb.AppendLine($"              <strong>{field.Label}:</strong> {{{{ getFieldValue(selectedItem()!, '{field.FieldName}') ?? '-' }}}}");
                sb.AppendLine("            </mat-list-item>");
            }
            
            sb.AppendLine("          </mat-list>");
            sb.AppendLine("        </mat-card-content>");
            sb.AppendLine("        <mat-card-actions>");
            
            if (_definition.IsUpdate)
            {
                sb.AppendLine("          <button mat-raised-button color=\"primary\" (click)=\"onEdit(selectedItem()!); closeDetail()\">Edit</button>");
            }
            
            if (_definition.IsDelete)
            {
                sb.AppendLine($"          <button mat-raised-button color=\"warn\" (click)=\"onDelete(selectedItem()?.{primaryKey}); closeDetail()\">Delete</button>");
            }
            
            sb.AppendLine("          <button mat-button (click)=\"closeDetail()\">Close</button>");
            sb.AppendLine("        </mat-card-actions>");
            sb.AppendLine("      </mat-card>");
            sb.AppendLine("    </div>");
        }

        private void BuildModal(StringBuilder sb, CSSFramework framework)
        {
            if (!_definition.IsPost && !_definition.IsUpdate) return;

            sb.AppendLine("  <!-- Modal for Create/Update -->");
            sb.AppendLine("  @if (showModal()) {");
            
            if (framework == CSSFramework.AngularMaterial)
            {
                BuildMaterialModal(sb);
            }
            else if (framework == CSSFramework.Bootstrap)
            {
                BuildBootstrapModal(sb);
            }
            else
            {
                BuildBasicModal(sb);
            }
            
            sb.AppendLine("  }");
        }

        private void BuildBootstrapModal(StringBuilder sb)
        {
            sb.AppendLine("    <div class=\"modal d-block\" tabindex=\"-1\" style=\"background-color: rgba(0,0,0,0.5);\">");
            sb.AppendLine("      <div class=\"modal-dialog\">");
            sb.AppendLine("        <div class=\"modal-content\">");
            sb.AppendLine("          <div class=\"modal-header\">");
            sb.AppendLine($"            <h5 class=\"modal-title\">{{{{ isEditMode() ? 'Edit' : 'Create' }}}} {_definition.EntityName}</h5>");
            sb.AppendLine("            <button type=\"button\" class=\"btn-close\" (click)=\"closeModal()\"></button>");
            sb.AppendLine("          </div>");
            sb.AppendLine("          <form [formGroup]=\"form\" (ngSubmit)=\"onSubmit()\">");
            sb.AppendLine("            <div class=\"modal-body\">");
            sb.AppendLine("              <div class=\"row\">");
            
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey))
            {
                sb.AppendLine("                <div class=\"col-md-6 mb-3\">");
                sb.AppendLine($"                  <label class=\"form-label\">{field.Label}</label>");
                sb.AppendLine($"                  <input formControlName=\"{field.FieldName}\" class=\"form-control\" />");
                sb.AppendLine("                </div>");
            }
            
            sb.AppendLine("              </div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div class=\"modal-footer\">");
            sb.AppendLine("              <button type=\"button\" class=\"btn btn-secondary\" (click)=\"closeModal()\">Cancel</button>");
            sb.AppendLine("              <button type=\"submit\" class=\"btn btn-primary\" [disabled]=\"form.invalid\">Save</button>");
            sb.AppendLine("            </div>");
            sb.AppendLine("          </form>");
            sb.AppendLine("        </div>");
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
        }

        private void BuildMaterialModal(StringBuilder sb)
        {
            sb.AppendLine("    <div class=\"modal-overlay\">");
            sb.AppendLine("      <mat-card style=\"width: 600px; max-width: 90%;\">");
            sb.AppendLine("        <mat-card-header>");
            sb.AppendLine($"          <mat-card-title>{{{{ isEditMode() ? 'Edit' : 'Create' }}}} {_definition.EntityName}</mat-card-title>");
            sb.AppendLine("        </mat-card-header>");
            sb.AppendLine("        <mat-card-content>");
            sb.AppendLine("          <form [formGroup]=\"form\" (ngSubmit)=\"onSubmit()\">");
            
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey))
            {
                sb.AppendLine("            <mat-form-field appearance=\"outline\" class=\"w-100 mb-3\">");
                sb.AppendLine($"              <mat-label>{field.Label}</mat-label>");
                sb.AppendLine($"              <input matInput formControlName=\"{field.FieldName}\" />");
                sb.AppendLine("            </mat-form-field>");
            }
            
            sb.AppendLine("          </form>");
            sb.AppendLine("        </mat-card-content>");
            sb.AppendLine("        <mat-card-actions align=\"end\">");
            sb.AppendLine("          <button mat-button (click)=\"closeModal()\">Cancel</button>");
            sb.AppendLine("          <button mat-raised-button color=\"primary\" (click)=\"onSubmit()\" [disabled]=\"form.invalid\">Save</button>");
            sb.AppendLine("        </mat-card-actions>");
            sb.AppendLine("      </mat-card>");
            sb.AppendLine("    </div>");
        }

        private void BuildBasicModal(StringBuilder sb)
        {
            sb.AppendLine("    <div class=\"modal-overlay\">");
            sb.AppendLine("      <div class=\"modal-content\">");
            sb.AppendLine("        <div class=\"modal-header\">");
            sb.AppendLine($"          <h3 class=\"modal-title\">{{{{ isEditMode() ? 'Edit' : 'Create' }}}} {_definition.EntityName}</h3>");
            sb.AppendLine("          <button class=\"modal-close\" (click)=\"closeModal()\">&times;</button>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <form [formGroup]=\"form\" (ngSubmit)=\"onSubmit()\">");
            sb.AppendLine("          <div class=\"modal-body\">");
            
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey))
            {
                sb.AppendLine("            <div class=\"form-group\">");
                sb.AppendLine($"              <label class=\"form-label\">{field.Label}</label>");
                sb.AppendLine($"              <input formControlName=\"{field.FieldName}\" class=\"form-control\" />");
                sb.AppendLine("            </div>");
            }
            
            sb.AppendLine("          </div>");
            sb.AppendLine("          <div class=\"modal-footer\">");
            sb.AppendLine("            <button type=\"button\" class=\"btn\" (click)=\"closeModal()\">Cancel</button>");
            sb.AppendLine("            <button type=\"submit\" class=\"btn btn-primary\" [disabled]=\"form.invalid\">Save</button>");
            sb.AppendLine("          </div>");
            sb.AppendLine("        </form>");
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
        }
    }
}
