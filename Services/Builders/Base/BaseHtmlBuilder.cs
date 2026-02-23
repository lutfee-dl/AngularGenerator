using System.Text;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services.Builders.Base
{
    /// <summary>
    /// Abstract Base Class for all HTML Builders
    /// Implements Template Method Pattern to reduce code duplication
    /// </summary>
    public abstract class BaseHtmlBuilder
    {
        protected readonly ComponentDefinition _definition;
        protected readonly StringBuilder _sb;

        protected BaseHtmlBuilder(ComponentDefinition definition)
        {
            _definition = definition;
            _sb = new StringBuilder();
        }

        /// <summary>
        /// Template Method - defines the skeleton of the build process
        /// </summary>
        public string Build()
        {
            _sb.Clear();
            
            BuildContainer();
            BuildHeader();
            BuildLoadingIndicator();
            BuildDataSection();
            BuildModal();
            CloseContainer();
            
            return _sb.ToString();
        }

        protected virtual void BuildContainer()
        {
            _sb.AppendLine("<div class=\"container-fluid px-2 py-2 bg-light min-vh-100\">");
        }

        protected virtual void CloseContainer()
        {
            _sb.AppendLine("</div>");
        }

        /// <summary>
        /// Builds the header section with title and actions
        /// Can be overridden if needed
        /// </summary>
        protected virtual void BuildHeader()
        {
            _sb.AppendLine("  <div class=\"card shadow-sm mb-2 border-0 bg-white\">");
            _sb.AppendLine("    <div class=\"card-body d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3\">");
            _sb.AppendLine($"      <h3 class=\"m-0 fw-bold text-dark\">{_definition.EntityName} Catalog</h3>");
            _sb.AppendLine("      <div class=\"d-flex gap-2\">");
            _sb.AppendLine("        <input type=\"text\" class=\"form-control\" style=\"width: 300px;\" placeholder=\"Search...\" ");
            _sb.AppendLine("               [ngModel]=\"searchTerm()\" (ngModelChange)=\"searchTerm.set($event)\">");
            
            if (_definition.IsPost)
            {
                _sb.AppendLine("        <button class=\"btn btn-primary d-flex align-items-center justify-content-center px-4 shadow-sm fw-bold\" (click)=\"openCreate()\">");
                _sb.AppendLine($"          <span class=\"me-2 fs-5\">&#10010;</span> Add {_definition.EntityName}");
                _sb.AppendLine("        </button>");
            }
            
            _sb.AppendLine("      </div>");
            _sb.AppendLine("    </div>");
            _sb.AppendLine("  </div>");
            _sb.AppendLine();
        }

        /// <summary>
        /// Builds the loading indicator
        /// Can be overridden if needed
        /// </summary>
        protected virtual void BuildLoadingIndicator()
        {
            if (_definition.IsPost || _definition.IsUpdate)
            {
                _sb.AppendLine("  @if (isLoading() && !showModal()) {");
            }
            else
            {
                _sb.AppendLine("  @if (isLoading()) {");
            }
            _sb.AppendLine("    <div class=\"loading\">Loading...</div>");
            _sb.AppendLine("  }");
            _sb.AppendLine();
        }

        /// <summary>
        /// Abstract method - each builder must implement its own data section
        /// (Table view, Card view, etc.)
        /// </summary>
        protected abstract void BuildDataSection();

        /// <summary>
        /// Builds the modal for Create/Edit/View
        /// Shared between all builders
        /// </summary>
        protected virtual void BuildModal()
        {
            if (!_definition.IsPost && !_definition.IsUpdate && !_definition.IsGetById) return;

            _sb.AppendLine();
            _sb.AppendLine("@if (showModal()) {");
            _sb.AppendLine("<div class=\"modal fade show\" tabindex=\"-1\" style=\"display: block; background: rgba(0,0,0,0.5); z-index: 1060;\">");
            _sb.AppendLine("  <div class=\"modal-dialog modal-lg modal-dialog-centered\">");
            _sb.AppendLine("    <div class=\"modal-content shadow-lg border-0\">");
            _sb.AppendLine("      ");
            _sb.AppendLine("      <div class=\"modal-header bg-light py-3\">");
            _sb.AppendLine("        <h5 class=\"modal-title fw-bold\">");
            _sb.AppendLine("          <i class=\"bi bi-box-seam me-2\"></i>");
            _sb.AppendLine($"          @if(isViewMode()) {{ View {_definition.EntityName} Detail }}");
            _sb.AppendLine($"          @else if(isEditMode()) {{ Edit {_definition.EntityName} Information }}");
            _sb.AppendLine($"          @else {{ Create New {_definition.EntityName} }}");
            _sb.AppendLine("        </h5>");
            _sb.AppendLine("        <button type=\"button\" class=\"btn-close\" (click)=\"onClose()\"></button>");
            _sb.AppendLine("      </div>");
            _sb.AppendLine();
            
            // Form
            _sb.AppendLine($"      <form [formGroup]=\"{_definition.EntityName.ToLower()}Form\" (ngSubmit)=\"onSubmit()\">");
            _sb.AppendLine("        <div class=\"modal-body p-4\">");
            _sb.AppendLine("          <div class=\"row g-3\"> @for (field of formFields; track field.key) {");
            _sb.AppendLine("              <div [class]=\"field.type === 'checkbox' ? 'col-12' : 'col-md-6'\">");
            _sb.AppendLine("                <label class=\"form-label fw-semibold mb-1\">");
            _sb.AppendLine("                  {{ field.label }}");
            _sb.AppendLine("                  @if(field.required && !isViewMode()) { <span class=\"text-danger\">*</span> }");
            _sb.AppendLine("                </label>");
            _sb.AppendLine();
            
            // Checkbox field
            _sb.AppendLine("                @if (field.type === 'checkbox') {");
            _sb.AppendLine("                  <div class=\"form-check form-switch border rounded p-2 ps-5\">");
            _sb.AppendLine("                    <input type=\"checkbox\" class=\"form-check-input\" [formControlName]=\"field.key\" [id]=\"field.key\">");
            _sb.AppendLine("                    <label class=\"form-check-label text-muted\" [for]=\"field.key\">Enable this option</label>");
            _sb.AppendLine("                  </div>");
            
            // Other input types
            _sb.AppendLine("                } @else {");
            _sb.AppendLine("                  <input [type]=\"field.type\"");
            _sb.AppendLine("                         [formControlName]=\"field.key\"");
            _sb.AppendLine("                         class=\"form-control\"");
            _sb.AppendLine("                         [class.bg-light]=\"isViewMode()\"");
            _sb.AppendLine("                         [readonly]=\"isViewMode()\"");
            _sb.AppendLine("                         [placeholder]=\"'Enter ' + field.label\"");
            _sb.AppendLine("                         [attr.maxlength]=\"field.maxLength\">");
            _sb.AppendLine("                }");
            _sb.AppendLine("                ");
            _sb.AppendLine($"                @if ({_definition.EntityName.ToLower()}Form.get(field.key)?.touched && {_definition.EntityName.ToLower()}Form.get(field.key)?.invalid) {{");
            _sb.AppendLine("                  <small class=\"text-danger\">This field is required.</small>");
            _sb.AppendLine("                }");
            _sb.AppendLine("              </div>");
            _sb.AppendLine("            }");
            _sb.AppendLine("          </div>");
            _sb.AppendLine("        </div>");
            _sb.AppendLine();
            
            // Modal Actions
            _sb.AppendLine("        <div class=\"modal-footer bg-light border-0 px-4\">");
            _sb.AppendLine("          <button type=\"button\" class=\"btn btn-outline-secondary px-4\" (click)=\"onClose()\">");
            _sb.AppendLine("            {{ isViewMode() ? 'Close' : 'Cancel' }}");
            _sb.AppendLine("          </button>");
            _sb.AppendLine();
            _sb.AppendLine("          @if(isViewMode()) {");
            _sb.AppendLine("            <button type=\"button\" class=\"btn btn-primary px-4\" (click)=\"enableEditMode()\">");
            _sb.AppendLine("              <i class=\"bi bi-pencil-square me-1\"></i> Edit");
            _sb.AppendLine("            </button>");
            _sb.AppendLine("          }");
            _sb.AppendLine();
            _sb.AppendLine("          @if(!isViewMode()) {");
            _sb.AppendLine($"            <button type=\"submit\" class=\"btn btn-success px-4\" [disabled]=\"{_definition.EntityName.ToLower()}Form.invalid\">");
            _sb.AppendLine("              <i class=\"bi bi-check-lg me-1\"></i>");
            _sb.AppendLine($"              {{{{ isEditMode() ? 'Update Changes' : 'Create {_definition.EntityName}' }}}}");
            _sb.AppendLine("            </button>");
            _sb.AppendLine("          }");
            _sb.AppendLine("        </div>");
            _sb.AppendLine("      </form>");
            _sb.AppendLine("    </div>");
            _sb.AppendLine("  </div>");
            _sb.AppendLine("</div>");
            _sb.AppendLine("}");
        }

        /// <summary>
        /// Builds pagination footer (commonly used in Table view)
        /// Can be called by derived classes
        /// </summary>
        protected virtual void BuildPaginationFooter()
        {
            _sb.AppendLine("    <div class=\"card-footer bg-white border-top py-3\">");
            _sb.AppendLine("      <div class=\"d-flex justify-content-between align-items-center flex-wrap gap-3\">");
            _sb.AppendLine("    ");
            _sb.AppendLine("    <div class=\"d-flex align-items-center gap-3\">");
            _sb.AppendLine("      <div class=\"d-flex align-items-center\">");
            _sb.AppendLine("        <span class=\"small text-muted me-2 text-nowrap\">Show</span>");
            _sb.AppendLine("        <select class=\"form-select form-select-sm shadow-none\" ");
            _sb.AppendLine("                style=\"width: 70px;\"");
            _sb.AppendLine("                [ngModel]=\"pageSize()\" ");
            _sb.AppendLine("                (ngModelChange)=\"setPageSize($any($event))\">");
            _sb.AppendLine("          <option [value]=\"10\">10</option>");
            _sb.AppendLine("          <option [value]=\"20\">20</option>");
            _sb.AppendLine("          <option [value]=\"50\">50</option>");
            _sb.AppendLine("          <option [value]=\"100\">100</option>");
            _sb.AppendLine("        </select>");
            _sb.AppendLine("        <span class=\"small text-muted ms-2 text-nowrap\">entries</span>");
            _sb.AppendLine("      </div>");
            _sb.AppendLine("      ");
            _sb.AppendLine("      <div class=\"small text-muted border-start ps-3 d-none d-md-block\">");
            _sb.AppendLine("        Showing page <span class=\"fw-bold text-primary\">{{ currentPage() }}</span> of {{ totalPages() }}");
            _sb.AppendLine("      </div>");
            _sb.AppendLine("    </div>");
            _sb.AppendLine("    ");
            _sb.AppendLine($"      <nav aria-label=\"{_definition.EntityName} Page Navigation\">");
            _sb.AppendLine("        <ul class=\"pagination pagination-sm m-0 shadow-sm\">");
            _sb.AppendLine("          <li class=\"page-item\" [class.disabled]=\"currentPage() === 1\">");
            _sb.AppendLine("            <a class=\"page-link\" role=\"button\" (click)=\"setPage(currentPage() - 1)\">Previous</a>");
            _sb.AppendLine("          </li>");
            _sb.AppendLine();
            _sb.AppendLine("          @for (p of [].constructor(totalPages()); track $index) {");
            _sb.AppendLine("            @if (($index + 1) >= (currentPage() - 1) && ($index + 1) <= (currentPage() + 1)) {");
            _sb.AppendLine("              <li class=\"page-item\" [class.active]=\"currentPage() === ($index + 1)\">");
            _sb.AppendLine("                <a class=\"page-link\" role=\"button\" (click)=\"setPage($index + 1)\">");
            _sb.AppendLine("                  {{ $index + 1 }}");
            _sb.AppendLine("                </a>");
            _sb.AppendLine("              </li>");
            _sb.AppendLine("            }");
            _sb.AppendLine("          }");
            _sb.AppendLine();
            _sb.AppendLine("          <li class=\"page-item\" [class.disabled]=\"currentPage() === totalPages() || totalPages() === 0\">");
            _sb.AppendLine("            <a class=\"page-link\" role=\"button\" (click)=\"setPage(currentPage() + 1)\">Next</a>");
            _sb.AppendLine("          </li>");
            _sb.AppendLine("        </ul>");
            _sb.AppendLine("      </nav>");
            _sb.AppendLine("    </div>");
            _sb.AppendLine("</div>");
        }
    }
}
