using AngularGenerator.Core.Models;
using System.Text;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder สำหรับสร้างไฟล์ CSS พื้นฐาน
    /// ใช้เมื่อเลือก CSSFramework = BasicCSS
    /// </summary>
    public class CssBuilder
    {
        private readonly ComponentDefinition _definition;
        private readonly StringBuilder _css = new();

        public CssBuilder(ComponentDefinition definition)
        {
            _definition = definition;
        }

        /// <summary>
        /// Build CSS สำหรับ Table View
        /// </summary>
        public CssBuilder WithTableStyles()
        {
            _css.AppendLine("/* ========== Table View Styles ========== */");
            _css.AppendLine(".container {");
            _css.AppendLine("  padding: 10px;");
            _css.AppendLine("  background-color: #ffffff;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".header {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  justify-content: space-between;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("  margin-bottom: 20px;");
            _css.AppendLine("  padding-bottom: 15px;");
            _css.AppendLine("  border-bottom: 2px solid #e0e0e0;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".header h2 {");
            _css.AppendLine("  color: #333;");
            _css.AppendLine("  margin: 0;");
            _css.AppendLine("  font-size: 24px;");
            _css.AppendLine("  font-weight: 600;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine("/* Data Table */");
            _css.AppendLine(".data-table {");
            _css.AppendLine("  width: 100%;");
            _css.AppendLine("  border-collapse: collapse;");
            _css.AppendLine("  margin-top: 20px;");
            _css.AppendLine("  overflow: hidden;");
            _css.AppendLine("  border-radius: 8px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".data-table thead {");
            _css.AppendLine("  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);");
            _css.AppendLine("  color: white;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".data-table th {");
            _css.AppendLine("  padding: 15px;");
            _css.AppendLine("  text-align: left;");
            _css.AppendLine("  font-weight: 600;");
            _css.AppendLine("  font-size: 14px;");
            _css.AppendLine("  text-transform: uppercase;");
            _css.AppendLine("  letter-spacing: 0.5px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".data-table td {");
            _css.AppendLine("  padding: 12px 15px;");
            _css.AppendLine("  border-bottom: 1px solid #f0f0f0;");
            _css.AppendLine("  color: #555;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".data-table tbody tr:hover {");
            _css.AppendLine("  background-color: #f8f9fa;");
            _css.AppendLine("  transition: background-color 0.3s ease;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".data-table tbody tr:last-child td {");
            _css.AppendLine("  border-bottom: none;");
            _css.AppendLine("}");
            _css.AppendLine(".data-table tbody tr:nth-child(even){background-color: #f2f2f2;}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Card View
        /// </summary>
        public CssBuilder WithCardStyles()
        {
            _css.AppendLine("/* ========== Card View Styles ========== */");
            _css.AppendLine(".card-container {");
            _css.AppendLine("  max-width: 1200px;");
            _css.AppendLine("  margin: 20px auto;");
            _css.AppendLine("  padding: 20px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-grid {");
            _css.AppendLine("  display: grid;");
            _css.AppendLine("  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));");
            _css.AppendLine("  gap: 20px;");
            _css.AppendLine("  margin-top: 20px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-item {");
            _css.AppendLine("  background: white;");
            _css.AppendLine("  border-radius: 12px;");
            _css.AppendLine("  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);");
            _css.AppendLine("  padding: 20px;");
            _css.AppendLine("  transition: transform 0.3s ease, box-shadow 0.3s ease;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-item:hover {");
            _css.AppendLine("  transform: translateY(-5px);");
            _css.AppendLine("  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-header {");
            _css.AppendLine("  border-bottom: 2px solid #e0e0e0;");
            _css.AppendLine("  padding-bottom: 12px;");
            _css.AppendLine("  margin-bottom: 15px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-title {");
            _css.AppendLine("  font-size: 18px;");
            _css.AppendLine("  font-weight: 600;");
            _css.AppendLine("  color: #333;");
            _css.AppendLine("  margin: 0;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-body {");
            _css.AppendLine("  margin-bottom: 15px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-field {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  margin-bottom: 10px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-field-label {");
            _css.AppendLine("  font-weight: 600;");
            _css.AppendLine("  color: #666;");
            _css.AppendLine("  min-width: 120px;");
            _css.AppendLine("  font-size: 14px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-field-value {");
            _css.AppendLine("  color: #333;");
            _css.AppendLine("  font-size: 14px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".card-actions {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  gap: 10px;");
            _css.AppendLine("  justify-content: flex-end;");
            _css.AppendLine("  margin-top: 15px;");
            _css.AppendLine("  padding-top: 15px;");
            _css.AppendLine("  border-top: 1px solid #f0f0f0;");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Buttons
        /// </summary>
        public CssBuilder WithButtonStyles()
        {
            _css.AppendLine("/* ========== Button Styles ========== */");
            _css.AppendLine(".btn {");
            _css.AppendLine("  padding: 10px 20px;");
            _css.AppendLine("  border: none;");
            _css.AppendLine("  border-radius: 6px;");
            _css.AppendLine("  font-size: 14px;");
            _css.AppendLine("  font-weight: 500;");
            _css.AppendLine("  cursor: pointer;");
            _css.AppendLine("  transition: all 0.3s ease;");
            _css.AppendLine("  text-decoration: none;");
            _css.AppendLine("  display: inline-block;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".btn:hover {");
            _css.AppendLine("  transform: translateY(-2px);");
            _css.AppendLine("  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".btn-primary {");
            _css.AppendLine("  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);");
            _css.AppendLine("  color: white;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".btn-success {");
            _css.AppendLine("  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);");
            _css.AppendLine("  color: white;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".btn-warning {");
            _css.AppendLine("  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);");
            _css.AppendLine("  color: white;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".btn-danger {");
            _css.AppendLine("  background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);");
            _css.AppendLine("  color: white;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".btn-sm {");
            _css.AppendLine("  padding: 6px 12px;");
            _css.AppendLine("  font-size: 12px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".btn:disabled {");
            _css.AppendLine("  opacity: 0.6;");
            _css.AppendLine("  cursor: not-allowed;");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Form
        /// </summary>
        public CssBuilder WithFormStyles()
        {
            _css.AppendLine("/* ========== Form Styles ========== */");
            _css.AppendLine(".form-group {");
            _css.AppendLine("  margin-bottom: 20px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".form-label {");
            _css.AppendLine("  display: block;");
            _css.AppendLine("  margin-bottom: 8px;");
            _css.AppendLine("  font-weight: 600;");
            _css.AppendLine("  color: #333;");
            _css.AppendLine("  font-size: 14px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".form-control {");
            _css.AppendLine("  width: 100%;");
            _css.AppendLine("  padding: 10px 12px;");
            _css.AppendLine("  border: 2px solid #e0e0e0;");
            _css.AppendLine("  border-radius: 6px;");
            _css.AppendLine("  font-size: 14px;");
            _css.AppendLine("  transition: border-color 0.3s ease;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".form-control:focus {");
            _css.AppendLine("  outline: none;");
            _css.AppendLine("  border-color: #667eea;");
            _css.AppendLine("  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".form-control.is-invalid {");
            _css.AppendLine("  border-color: #dc3545;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".invalid-feedback {");
            _css.AppendLine("  color: #dc3545;");
            _css.AppendLine("  font-size: 12px;");
            _css.AppendLine("  margin-top: 5px;");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Modal
        /// </summary>
        public CssBuilder WithModalStyles()
        {
            _css.AppendLine("/* ========== Modal Styles ========== */");
            _css.AppendLine(".modal-overlay {");
            _css.AppendLine("  position: fixed;");
            _css.AppendLine("  top: 0;");
            _css.AppendLine("  left: 0;");
            _css.AppendLine("  right: 0;");
            _css.AppendLine("  bottom: 0;");
            _css.AppendLine("  background-color: rgba(0, 0, 0, 0.5);");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  justify-content: center;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("  z-index: 1000;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".modal-content {");
            _css.AppendLine("  background: white;");
            _css.AppendLine("  border-radius: 12px;");
            _css.AppendLine("  width: 90%;");
            _css.AppendLine("  max-width: 600px;");
            _css.AppendLine("  max-height: 90vh;");
            _css.AppendLine("  overflow-y: auto;");
            _css.AppendLine("  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".modal-header {");
            _css.AppendLine("  padding: 20px;");
            _css.AppendLine("  border-bottom: 2px solid #e0e0e0;");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  justify-content: space-between;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".modal-title {");
            _css.AppendLine("  font-size: 20px;");
            _css.AppendLine("  font-weight: 600;");
            _css.AppendLine("  color: #333;");
            _css.AppendLine("  margin: 0;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".modal-close {");
            _css.AppendLine("  background: none;");
            _css.AppendLine("  border: none;");
            _css.AppendLine("  font-size: 24px;");
            _css.AppendLine("  cursor: pointer;");
            _css.AppendLine("  color: #999;");
            _css.AppendLine("  transition: color 0.3s ease;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".modal-close:hover {");
            _css.AppendLine("  color: #333;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".modal-body {");
            _css.AppendLine("  padding: 20px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".modal-footer {");
            _css.AppendLine("  padding: 20px;");
            _css.AppendLine("  border-top: 2px solid #e0e0e0;");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  justify-content: flex-end;");
            _css.AppendLine("  gap: 10px;");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Loading Spinner
        /// </summary>
        public CssBuilder WithLoadingStyles()
        {
            _css.AppendLine("/* ========== Loading Spinner ========== */");
            _css.AppendLine(".loading-container {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  justify-content: center;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("  padding: 40px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".spinner {");
            _css.AppendLine("  border: 4px solid #f3f3f3;");
            _css.AppendLine("  border-top: 4px solid #667eea;");
            _css.AppendLine("  border-radius: 50%;");
            _css.AppendLine("  width: 40px;");
            _css.AppendLine("  height: 40px;");
            _css.AppendLine("  animation: spin 1s linear infinite;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine("@keyframes spin {");
            _css.AppendLine("  0% { transform: rotate(0deg); }");
            _css.AppendLine("  100% { transform: rotate(360deg); }");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Responsive Design
        /// </summary>
        public CssBuilder WithResponsiveStyles()
        {
            _css.AppendLine("/* ========== Responsive Design ========== */");
            _css.AppendLine("@media (max-width: 768px) {");
            _css.AppendLine("  .container, .card-container {");
            _css.AppendLine("    padding: 10px;");
            _css.AppendLine("  }");
            _css.AppendLine();

            _css.AppendLine("  .card-grid {");
            _css.AppendLine("    grid-template-columns: 1fr;");
            _css.AppendLine("  }");
            _css.AppendLine();

            _css.AppendLine("  .data-table {");
            _css.AppendLine("    font-size: 12px;");
            _css.AppendLine("  }");
            _css.AppendLine();

            _css.AppendLine("  .data-table th,");
            _css.AppendLine("  .data-table td {");
            _css.AppendLine("    padding: 8px;");
            _css.AppendLine("  }");
            _css.AppendLine();

            _css.AppendLine("  .modal-content {");
            _css.AppendLine("    width: 95%;");
            _css.AppendLine("  }");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        public string Build()
        {
            var finalCss = new StringBuilder();
            
            finalCss.AppendLine($"/* Generated CSS for {_definition.EntityName} Component */");
            finalCss.AppendLine($"/* Generated by Angular CRUD Generator - {DateTime.Now:yyyy-MM-dd HH:mm:ss} */");
            finalCss.AppendLine();

            // Reset styles
            finalCss.AppendLine("/* ========== Reset Styles ========== */");
            finalCss.AppendLine("* {");
            finalCss.AppendLine("  box-sizing: border-box;");
            finalCss.AppendLine("  margin: 0;");
            finalCss.AppendLine("  padding: 0;");
            finalCss.AppendLine("}");
            finalCss.AppendLine();

            // Add generated CSS
            finalCss.Append(_css);

            return finalCss.ToString();
        }
    }
}
