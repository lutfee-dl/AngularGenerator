using AngularGenerator.Core.Models;
using System.Text;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder สำหรับสร้าง HTML แบบ Card View
    /// </summary>
    public class HtmlCardBuilder
    {
        private readonly ComponentDefinition _definition;
        private readonly StringBuilder _html = new();

        public HtmlCardBuilder(ComponentDefinition definition)
        {
            _definition = definition;
        }

        public string Build()
        {
            var framework = _definition.CssFramework;
            
            _html.Clear();
            _html.AppendLine($"<!-- Generated HTML Card View for {_definition.EntityName} Component -->");
            _html.AppendLine($"<!-- CSS Framework: {framework} -->");
            _html.AppendLine();

            BuildHeader(framework);
            BuildLoadingIndicator(framework);
            BuildCardGrid(framework);
            BuildModal(framework);

            return _html.ToString();
        }

        private void BuildHeader(CSSFramework framework)
        {
            var containerClass = framework switch
            {
                CSSFramework.Bootstrap => "container mt-4",
                CSSFramework.AngularMaterial => "mat-elevation-z2",
                _ => "card-container"
            };

            var buttonClass = framework switch
            {
                CSSFramework.Bootstrap => "btn btn-primary",
                CSSFramework.AngularMaterial => "mat-raised-button mat-primary",
                _ => "btn btn-primary"
            };

            _html.AppendLine($"<div class=\"{containerClass}\">");
            _html.AppendLine("  <div class=\"header\">");
            _html.AppendLine($"    <h2>{_definition.EntityName} Management</h2>");
            
            if (_definition.IsPost)
            {
                _html.AppendLine($"    <button class=\"{buttonClass}\" (click)=\"openCreate()\">");
                
                if (framework == CSSFramework.AngularMaterial)
                    _html.AppendLine("      <mat-icon>add</mat-icon>");
                
                _html.AppendLine("      Create New");
                _html.AppendLine("    </button>");
            }
            
            _html.AppendLine("  </div>");
            _html.AppendLine();
        }

        private void BuildLoadingIndicator(CSSFramework framework)
        {
            _html.AppendLine("  <!-- Loading Indicator -->");
            _html.AppendLine("  @if (isLoading()) {");
            
            if (framework == CSSFramework.AngularMaterial)
            {
                _html.AppendLine("    <div class=\"loading-container\">");
                _html.AppendLine("      <mat-spinner></mat-spinner>");
                _html.AppendLine("    </div>");
            }
            else if (framework == CSSFramework.Bootstrap)
            {
                _html.AppendLine("    <div class=\"text-center py-5\">");
                _html.AppendLine("      <div class=\"spinner-border text-primary\" role=\"status\">");
                _html.AppendLine("        <span class=\"visually-hidden\">Loading...</span>");
                _html.AppendLine("      </div>");
                _html.AppendLine("    </div>");
            }
            else
            {
                _html.AppendLine("    <div class=\"loading-container\">");
                _html.AppendLine("      <div class=\"spinner\"></div>");
                _html.AppendLine("    </div>");
            }
            
            _html.AppendLine("  }");
            _html.AppendLine();
        }

        private void BuildCardGrid(CSSFramework framework)
        {
            _html.AppendLine("  <!-- Card Grid -->");
            _html.AppendLine("  @if (!isLoading()) {");
            
            var gridClass = framework switch
            {
                CSSFramework.Bootstrap => "row g-4",
                CSSFramework.AngularMaterial => "card-grid",
                _ => "card-grid"
            };

            _html.AppendLine($"    <div class=\"{gridClass}\">");
            _html.AppendLine();

            _html.AppendLine("      @if (dataList().length === 0) {");
            
            if (framework == CSSFramework.AngularMaterial)
            {
                _html.AppendLine("        <mat-card class=\"text-center\">");
                _html.AppendLine("          <mat-card-content>");
                _html.AppendLine("            <p>No data available</p>");
                _html.AppendLine("          </mat-card-content>");
                _html.AppendLine("        </mat-card>");
            }
            else if (framework == CSSFramework.Bootstrap)
            {
                _html.AppendLine("        <div class=\"col-12 text-center text-muted py-5\">");
                _html.AppendLine("          <p>No data available</p>");
                _html.AppendLine("        </div>");
            }
            else
            {
                _html.AppendLine("        <div class=\"empty-state\">");
                _html.AppendLine("          <p>No data available</p>");
                _html.AppendLine("        </div>");
            }
            
            _html.AppendLine("      }");
            _html.AppendLine();

            BuildCardItems(framework);

            _html.AppendLine("    </div>");
            _html.AppendLine("  }");
            _html.AppendLine("</div>");
            _html.AppendLine();
        }

        private void BuildCardItems(CSSFramework framework)
        {
            var primaryKey = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";

            _html.AppendLine($"      @for (item of dataList(); track trackByFn($index, item)) {{");
            
            if (framework == CSSFramework.Bootstrap)
            {
                _html.AppendLine("        <div class=\"col-md-4\">");
                _html.AppendLine("          <div class=\"card h-100\">");
                _html.AppendLine("            <div class=\"card-header\">");
                _html.AppendLine($"              <h5 class=\"card-title\">{{{{ item.{_definition.Fields.FirstOrDefault()?.FieldName ?? "name"} }}}}</h5>");
                _html.AppendLine("            </div>");
                _html.AppendLine("            <div class=\"card-body\">");
                
                foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey).Take(5))
                {
                    _html.AppendLine($"              <p><strong>{field.Label}:</strong> {{{{ item.{field.FieldName} }}}}</p>");
                }
                
                _html.AppendLine("            </div>");
                _html.AppendLine("            <div class=\"card-footer d-flex gap-2\">");
                
                if (_definition.IsUpdate)
                {
                    _html.AppendLine("              <button class=\"btn btn-sm btn-warning\" (click)=\"onEdit(item)\">");
                    _html.AppendLine("                Edit");
                    _html.AppendLine("              </button>");
                }
                
                if (_definition.IsDelete)
                {
                    _html.AppendLine($"              <button class=\"btn btn-sm btn-danger\" (click)=\"onDelete(item.{primaryKey})\">");
                    _html.AppendLine("                Delete");
                    _html.AppendLine("              </button>");
                }
                
                _html.AppendLine("            </div>");
                _html.AppendLine("          </div>");
                _html.AppendLine("        </div>");
            }
            else if (framework == CSSFramework.AngularMaterial)
            {
                _html.AppendLine("        <mat-card class=\"card-item\">");
                _html.AppendLine("          <mat-card-header>");
                _html.AppendLine($"            <mat-card-title>{{{{ item.{_definition.Fields.FirstOrDefault()?.FieldName ?? "name"} }}}}</mat-card-title>");
                _html.AppendLine("          </mat-card-header>");
                _html.AppendLine("          <mat-card-content>");
                
                foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey).Take(5))
                {
                    _html.AppendLine("            <div class=\"field-row\">");
                    _html.AppendLine($"              <strong>{field.Label}:</strong> {{{{ item.{field.FieldName} }}}}");
                    _html.AppendLine("            </div>");
                }
                
                _html.AppendLine("          </mat-card-content>");
                _html.AppendLine("          <mat-card-actions>");
                
                if (_definition.IsUpdate)
                {
                    _html.AppendLine("            <button mat-button color=\"primary\" (click)=\"onEdit(item)\">Edit</button>");
                }
                
                if (_definition.IsDelete)
                {
                    _html.AppendLine($"            <button mat-button color=\"warn\" (click)=\"onDelete(item.{primaryKey})\">Delete</button>");
                }
                
                _html.AppendLine("          </mat-card-actions>");
                _html.AppendLine("        </mat-card>");
            }
            else // BasicCSS
            {
                _html.AppendLine("        <div class=\"card-item\">");
                _html.AppendLine("          <div class=\"card-header\">");
                _html.AppendLine($"            <h3 class=\"card-title\">{{{{ item.{_definition.Fields.FirstOrDefault()?.FieldName ?? "name"} }}}}</h3>");
                _html.AppendLine("          </div>");
                _html.AppendLine("          <div class=\"card-body\">");
                
                foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey).Take(5))
                {
                    _html.AppendLine("            <div class=\"card-field\">");
                    _html.AppendLine($"              <span class=\"card-field-label\">{field.Label}:</span>");
                    _html.AppendLine($"              <span class=\"card-field-value\">{{{{ item.{field.FieldName} }}}}</span>");
                    _html.AppendLine("            </div>");
                }
                
                _html.AppendLine("          </div>");
                _html.AppendLine("          <div class=\"card-actions\">");
                
                if (_definition.IsUpdate)
                {
                    _html.AppendLine("            <button class=\"btn btn-warning btn-sm\" (click)=\"onEdit(item)\">Edit</button>");
                }
                
                if (_definition.IsDelete)
                {
                    _html.AppendLine($"            <button class=\"btn btn-danger btn-sm\" (click)=\"onDelete(item.{primaryKey})\">Delete</button>");
                }
                
                _html.AppendLine("          </div>");
                _html.AppendLine("        </div>");
            }
            
            _html.AppendLine("      }");
        }

        private void BuildModal(CSSFramework framework)
        {
            if (!_definition.IsPost && !_definition.IsUpdate) return;

            _html.AppendLine("  <!-- Modal for Create/Update -->");
            _html.AppendLine("  @if (showModal()) {");
            
            if (framework == CSSFramework.AngularMaterial)
            {
                BuildMaterialModal();
            }
            else if (framework == CSSFramework.Bootstrap)
            {
                BuildBootstrapModal();
            }
            else
            {
                BuildBasicModal();
            }
            
            _html.AppendLine("  }");
        }

        private void BuildBootstrapModal()
        {
            _html.AppendLine("    <div class=\"modal d-block\" tabindex=\"-1\" style=\"background-color: rgba(0,0,0,0.5);\">");
            _html.AppendLine("      <div class=\"modal-dialog\">");
            _html.AppendLine("        <div class=\"modal-content\">");
            _html.AppendLine("          <div class=\"modal-header\">");
            _html.AppendLine("            <h5 class=\"modal-title\">{{ isEditMode() ? 'Edit' : 'Create' }} {_definition.EntityName}</h5>");
            _html.AppendLine("            <button type=\"button\" class=\"btn-close\" (click)=\"closeModal()\"></button>");
            _html.AppendLine("          </div>");
            _html.AppendLine("          <form [formGroup]=\"form\" (ngSubmit)=\"onSubmit()\">");
            _html.AppendLine("            <div class=\"modal-body\">");
            
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey))
            {
                _html.AppendLine("              <div class=\"mb-3\">");
                _html.AppendLine($"                <label class=\"form-label\">{field.Label}</label>");
                _html.AppendLine($"                <input formControlName=\"{field.FieldName}\" class=\"form-control\" />");
                _html.AppendLine("              </div>");
            }
            
            _html.AppendLine("            </div>");
            _html.AppendLine("            <div class=\"modal-footer\">");
            _html.AppendLine("              <button type=\"button\" class=\"btn btn-secondary\" (click)=\"closeModal()\">Cancel</button>");
            _html.AppendLine("              <button type=\"submit\" class=\"btn btn-primary\" [disabled]=\"form.invalid\">Save</button>");
            _html.AppendLine("            </div>");
            _html.AppendLine("          </form>");
            _html.AppendLine("        </div>");
            _html.AppendLine("      </div>");
            _html.AppendLine("    </div>");
        }

        private void BuildMaterialModal()
        {
            _html.AppendLine("    <div class=\"modal-overlay\">");
            _html.AppendLine("      <mat-card class=\"modal-card\">");
            _html.AppendLine("        <mat-card-header>");
            _html.AppendLine("          <mat-card-title>{{ isEditMode() ? 'Edit' : 'Create' }} {_definition.EntityName}</mat-card-title>");
            _html.AppendLine("        </mat-card-header>");
            _html.AppendLine("        <mat-card-content>");
            _html.AppendLine("          <form [formGroup]=\"form\" (ngSubmit)=\"onSubmit()\">");
            
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey))
            {
                _html.AppendLine("            <mat-form-field appearance=\"outline\" class=\"w-100\">");
                _html.AppendLine($"              <mat-label>{field.Label}</mat-label>");
                _html.AppendLine($"              <input matInput formControlName=\"{field.FieldName}\" />");
                _html.AppendLine("            </mat-form-field>");
            }
            
            _html.AppendLine("          </form>");
            _html.AppendLine("        </mat-card-content>");
            _html.AppendLine("        <mat-card-actions>");
            _html.AppendLine("          <button mat-button (click)=\"closeModal()\">Cancel</button>");
            _html.AppendLine("          <button mat-raised-button color=\"primary\" (click)=\"onSubmit()\" [disabled]=\"form.invalid\">Save</button>");
            _html.AppendLine("        </mat-card-actions>");
            _html.AppendLine("      </mat-card>");
            _html.AppendLine("    </div>");
        }

        private void BuildBasicModal()
        {
            _html.AppendLine("    <div class=\"modal-overlay\">");
            _html.AppendLine("      <div class=\"modal-content\">");
            _html.AppendLine("        <div class=\"modal-header\">");
            _html.AppendLine("          <h3 class=\"modal-title\">{{ isEditMode() ? 'Edit' : 'Create' }} {_definition.EntityName}</h3>");
            _html.AppendLine("          <button class=\"modal-close\" (click)=\"closeModal()\">&times;</button>");
            _html.AppendLine("        </div>");
            _html.AppendLine("        <form [formGroup]=\"form\" (ngSubmit)=\"onSubmit()\">");
            _html.AppendLine("          <div class=\"modal-body\">");
            
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey))
            {
                _html.AppendLine("            <div class=\"form-group\">");
                _html.AppendLine($"              <label class=\"form-label\">{field.Label}</label>");
                _html.AppendLine($"              <input formControlName=\"{field.FieldName}\" class=\"form-control\" />");
                _html.AppendLine("            </div>");
            }
            
            _html.AppendLine("          </div>");
            _html.AppendLine("          <div class=\"modal-footer\">");
            _html.AppendLine("            <button type=\"button\" class=\"btn\" (click)=\"closeModal()\">Cancel</button>");
            _html.AppendLine("            <button type=\"submit\" class=\"btn btn-primary\" [disabled]=\"form.invalid\">Save</button>");
            _html.AppendLine("          </div>");
            _html.AppendLine("        </form>");
            _html.AppendLine("      </div>");
            _html.AppendLine("    </div>");
        }
    }
}
