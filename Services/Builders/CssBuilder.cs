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
            _css.AppendLine("@import url('https://fonts.googleapis.com/css2?family=Sarabun:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;0,800;1,100;1,200;1,300;1,400;1,500;1,600;1,700;1,800&display=swap');");
            _css.AppendLine();
            _css.AppendLine("body {");
            _css.AppendLine("  margin: 0;");
            _css.AppendLine("  padding: 0;");
            _css.AppendLine("  font-family: \"Sarabun\", sans-serif;");
            _css.AppendLine("  font-weight: 800;");
            _css.AppendLine("  font-style: normal;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".container {");
            _css.AppendLine("  padding: 20px;");
            _css.AppendLine("  font-family: \"Sarabun\", sans-serif;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".header-section {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  justify-content: space-between;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("  margin-bottom: 20px;");
            _css.AppendLine("  border-bottom: 2px solid #f0f0f0;");
            _css.AppendLine("  padding-bottom: 15px;");
            _css.AppendLine("}");
            _css.AppendLine(".header-actions {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  gap: 10px;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".search-box {");
            _css.AppendLine("  padding: 8px 12px;");
            _css.AppendLine("  border: 1px solid #ddd;");
            _css.AppendLine("  border-radius: 4px;");
            _css.AppendLine("  width: 220px;");
            _css.AppendLine("}");
            _css.AppendLine(".search-box:focus {");
            _css.AppendLine("  border-color: #007bff;");
            _css.AppendLine("  outline: none;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".table-responsive {");
            _css.AppendLine("  overflow-x: auto;");
            _css.AppendLine("}");
            _css.AppendLine(".table {");
            _css.AppendLine("  width: 100%;");
            _css.AppendLine("  border-collapse: collapse;");
            _css.AppendLine("  min-width: 800px;");
            _css.AppendLine("}");
            _css.AppendLine(".table th,");
            _css.AppendLine(".table td {");
            _css.AppendLine("  padding: 12px 15px;");
            _css.AppendLine("  border-bottom: 1px solid #eee;");
            _css.AppendLine("  text-align: left;");
            _css.AppendLine("}");
            _css.AppendLine(".table th {");
            _css.AppendLine("  background: #f8f9fa;");
            _css.AppendLine("  font-weight: 600;");
            _css.AppendLine("  color: #444;");
            _css.AppendLine("  text-transform: uppercase;");
            _css.AppendLine("  font-size: 0.85rem;");
            _css.AppendLine("}");
            _css.AppendLine(".table tbody tr:nth-child(even){background-color: #f2f2f2;}");
            _css.AppendLine();

            _css.AppendLine(".text-center {");
            _css.AppendLine("  text-align: center;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".sortable {");
            _css.AppendLine("  cursor: pointer;");
            _css.AppendLine("  user-select: none;");
            _css.AppendLine("}");
            _css.AppendLine(".sortable:hover {");
            _css.AppendLine("  background-color: #e9ecef;");
            _css.AppendLine("}");
            _css.AppendLine(".sort-arrow {");
            _css.AppendLine("  color: #007bff;");
            _css.AppendLine("  margin-left: 5px;");
            _css.AppendLine("  font-size: 0.8em;");
            _css.AppendLine("}");
            _css.AppendLine();

            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                _css.AppendLine(".table th:last-child {");
                _css.AppendLine(" position: sticky;");
                _css.AppendLine(" right: 0;");
                _css.AppendLine(" background-color: #f8f9fa;");
                _css.AppendLine(" z-index: 2;");
                _css.AppendLine(" box-shadow: -2px 0 5px rgba(0,0,0,0.05);");
                _css.AppendLine("}");
                _css.AppendLine();
                _css.AppendLine(".table td:last-child {");
                _css.AppendLine(" position: sticky;");
                _css.AppendLine(" right: 0;");
                _css.AppendLine(" background-color: #fff;");
                _css.AppendLine(" z-index: 1;");
                _css.AppendLine(" box-shadow: -2px 0 5px rgba(0,0,0,0.05);");
                _css.AppendLine("}");
                _css.AppendLine();
            }

            _css.AppendLine(".table tbody tr:nth-child(even) td:last-child {");
            _css.AppendLine(" background-color: #f2f2f2;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".action-col{");
            _css.AppendLine("  min-width: 180px;");
            _css.AppendLine("  white-space: nowrap;");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Buttons
        /// </summary>
        public CssBuilder WithButtonStyles()
        {
            _css.AppendLine(".btn {");
            _css.AppendLine("  border: none;");
            _css.AppendLine("  padding: 6px 12px;");
            _css.AppendLine("  border-radius: 4px;");
            _css.AppendLine("  cursor: pointer;");
            _css.AppendLine("  font-size: 0.85rem;");
            _css.AppendLine("  color: white;");
            _css.AppendLine("  margin: 0 2px;");
            _css.AppendLine("}");
            _css.AppendLine(".btn-add {");
            _css.AppendLine("  background: #28a745;");
            _css.AppendLine("  padding: 8px 16px;");
            _css.AppendLine("  font-size: 0.9rem;");
            _css.AppendLine("}");
            _css.AppendLine(".btn-info {");
            _css.AppendLine("  background: #17a2b8;");
            _css.AppendLine("}");
            _css.AppendLine(".btn-edit {");
            _css.AppendLine("  background: #007bff;");
            _css.AppendLine("}");
            _css.AppendLine(".btn-delete {");
            _css.AppendLine("  background: #dc3545;");
            _css.AppendLine("}");
            _css.AppendLine(".btn-secondary {");
            _css.AppendLine("  background: #6c757d;");
            _css.AppendLine("}");
            _css.AppendLine(".btn-success {");
            _css.AppendLine("  background: #28a745;");
            _css.AppendLine("}");
            _css.AppendLine(".btn:disabled {");
            _css.AppendLine("  background: #ccc;");
            _css.AppendLine("  cursor: not-allowed;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".badge {");
            _css.AppendLine("  padding: 4px 8px;");
            _css.AppendLine("  border-radius: 10px;");
            _css.AppendLine("  font-size: 0.75rem;");
            _css.AppendLine("  font-weight: bold;");
            _css.AppendLine("}");
            _css.AppendLine(".badge-active {");
            _css.AppendLine("  background: #d4edda;");
            _css.AppendLine("  color: #155724;");
            _css.AppendLine("}");
            _css.AppendLine(".badge-inactive {");
            _css.AppendLine("  background: #f8d7da;");
            _css.AppendLine("  color: #721c24;");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Modal
        /// </summary>
        public CssBuilder WithModalStyles()
        {
            if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
            {
                _css.AppendLine(".modal-actions {");
                _css.AppendLine("  margin-top: 20px;");
                _css.AppendLine("  display: flex;");
                _css.AppendLine("  justify-content: flex-end;");
                _css.AppendLine("  gap: 10px;");
                _css.AppendLine("  padding-top: 15px;");
                _css.AppendLine("  border-top: 1px solid #eee;");
                _css.AppendLine("}");
                _css.AppendLine();
                _css.AppendLine(".modal-backdrop {");
                _css.AppendLine("  position: fixed;");
                _css.AppendLine("  top: 0;");
                _css.AppendLine("  left: 0;");
                _css.AppendLine("  width: 100%;");
                _css.AppendLine("  height: 100%;");
                _css.AppendLine("  background: rgba(0, 0, 0, 0.5);");
                _css.AppendLine("  display: flex;");
                _css.AppendLine("  justify-content: center;");
                _css.AppendLine("  align-items: center;");
                _css.AppendLine("  z-index: 1000;");
                _css.AppendLine("  backdrop-filter: blur(2px);");
                _css.AppendLine("}");
                _css.AppendLine(".modal-content {");
                _css.AppendLine("  background: white;");
                _css.AppendLine("  padding: 25px;");
                _css.AppendLine("  border-radius: 8px;");
                _css.AppendLine("  width: 600px;");
                _css.AppendLine("  max-width: 90%;");
                _css.AppendLine("  max-height: 90vh;");
                _css.AppendLine("  overflow-y: auto;");
                _css.AppendLine("  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);");
                _css.AppendLine("}");
                _css.AppendLine();
            }
            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Form
        /// </summary>
        public CssBuilder WithFormStyles()
        {
            _css.AppendLine(".form-grid {");
            _css.AppendLine("  display: grid;");
            _css.AppendLine("  grid-template-columns: 1fr 1fr;");
            _css.AppendLine("  gap: 15px;");
            _css.AppendLine("}");
            _css.AppendLine(".full-width {");
            _css.AppendLine("  grid-column: span 2;");
            _css.AppendLine("}");
            _css.AppendLine(".form-group {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  flex-direction: column;");
            _css.AppendLine("}");
            _css.AppendLine(".form-group label {");
            _css.AppendLine("  margin-bottom: 5px;");
            _css.AppendLine("  font-size: 0.9rem;");
            _css.AppendLine("  font-weight: 500;");
            _css.AppendLine("  color: #555;");
            _css.AppendLine("}");
            _css.AppendLine(".required {");
            _css.AppendLine("  color: red;");
            _css.AppendLine("}");
            _css.AppendLine(".form-control {");
            _css.AppendLine("  padding: 8px;");
            _css.AppendLine("  border: 1px solid #ced4da;");
            _css.AppendLine("  border-radius: 4px;");
            _css.AppendLine("}");
            _css.AppendLine(".form-control:disabled {");
            _css.AppendLine("  background: #e9ecef;");
            _css.AppendLine("  color: #6c757d;");
            _css.AppendLine("}");
            _css.AppendLine(".checkbox-wrapper {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("  gap: 8px;");
            _css.AppendLine("  height: 100%;");
            _css.AppendLine("}");
            _css.AppendLine(".checkbox-wrapper input {");
            _css.AppendLine("  width: 18px;");
            _css.AppendLine("  height: 18px;");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        /// <summary>
        /// Build CSS สำหรับ Card View
        /// </summary>
        public CssBuilder WithCardStyles()
        {
            // Import Google Fonts (same as table view)
            _css.AppendLine("@import url('https://fonts.googleapis.com/css2?family=Sarabun:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;0,800;1,100;1,200;1,300;1,400;1,500;1,600;1,700;1,800&display=swap');");
            _css.AppendLine();
            _css.AppendLine("body {");
            _css.AppendLine("  margin: 0;");
            _css.AppendLine("  padding: 0;");
            _css.AppendLine("  font-family: \"Sarabun\", sans-serif;");
            _css.AppendLine("  font-weight: 800;");
            _css.AppendLine("  font-style: normal;");
            _css.AppendLine("}");
            _css.AppendLine();

            _css.AppendLine(".container {");
            _css.AppendLine("  padding: 20px;");
            _css.AppendLine("  font-family: \"Sarabun\", sans-serif;");
            _css.AppendLine("}");
            _css.AppendLine();

            // Header (same as table)
            _css.AppendLine(".header-section {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  justify-content: space-between;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("  margin-bottom: 20px;");
            _css.AppendLine("  border-bottom: 2px solid #f0f0f0;");
            _css.AppendLine("  padding-bottom: 15px;");
            _css.AppendLine("}");
            _css.AppendLine(".header-actions {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  gap: 10px;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("}");
            _css.AppendLine(".search-wrapper {");
            _css.AppendLine("  position: relative;");
            _css.AppendLine("}");
            _css.AppendLine(".search-box {");
            _css.AppendLine("  padding: 8px 12px;");
            _css.AppendLine("  border: 1px solid #ddd;");
            _css.AppendLine("  border-radius: 4px;");
            _css.AppendLine("  width: 250px;");
            _css.AppendLine("}");
            _css.AppendLine();

            // Product Grid Layout
            _css.AppendLine(".product-grid {");
            _css.AppendLine("  display: grid;");
            _css.AppendLine("  grid-template-columns: repeat(4, 1fr);");
            _css.AppendLine("  gap: 20px;");
            _css.AppendLine("  margin-top: 20px;");
            _css.AppendLine("}");
            _css.AppendLine();

            // Card Styles
            _css.AppendLine(".card {");
            _css.AppendLine("  border: 1px solid #ddd;");
            _css.AppendLine("  border-radius: 8px;");
            _css.AppendLine("  overflow: hidden;");
            _css.AppendLine("  transition: transform 0.2s, box-shadow 0.2s;");
            _css.AppendLine("  background: white;");
            _css.AppendLine("}");
            _css.AppendLine(".card:hover {");
            _css.AppendLine("  transform: translateY(-4px);");
            _css.AppendLine("  box-shadow: 0 4px 12px rgba(0,0,0,0.15);");
            _css.AppendLine("}");
            _css.AppendLine();

            // Card Image
            _css.AppendLine(".card-image-wrapper {");
            _css.AppendLine("  position: relative;");
            _css.AppendLine("  width: 100%;");
            _css.AppendLine("  height: 200px;");
            _css.AppendLine("  overflow: hidden;");
            _css.AppendLine("  background: #f5f5f5;");
            _css.AppendLine("  cursor: pointer;");
            _css.AppendLine("}");
            _css.AppendLine(".card-image-wrapper img {");
            _css.AppendLine("  width: 100%;");
            _css.AppendLine("  height: 100%;");
            _css.AppendLine("  object-fit: cover;");
            _css.AppendLine("}");
            _css.AppendLine(".badge-color {");
            _css.AppendLine("  position: absolute;");
            _css.AppendLine("  top: 10px;");
            _css.AppendLine("  right: 10px;");
            _css.AppendLine("  background: #28a745;");
            _css.AppendLine("  color: white;");
            _css.AppendLine("  padding: 4px 12px;");
            _css.AppendLine("  border-radius: 12px;");
            _css.AppendLine("  font-size: 12px;");
            _css.AppendLine("  font-weight: bold;");
            _css.AppendLine("}");
            _css.AppendLine();

            // Card Body
            _css.AppendLine(".card-body {");
            _css.AppendLine("  padding: 15px;");
            _css.AppendLine("}");
            _css.AppendLine(".card-title {");
            _css.AppendLine("  margin: 0 0 10px 0;");
            _css.AppendLine("  font-size: 18px;");
            _css.AppendLine("  font-weight: 600;");
            _css.AppendLine("  color: #333;");
            _css.AppendLine("  white-space: nowrap;");
            _css.AppendLine("  overflow: hidden;");
            _css.AppendLine("  text-overflow: ellipsis;");
            _css.AppendLine("}");
            _css.AppendLine(".card-info {");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  justify-content: space-between;");
            _css.AppendLine("  align-items: center;");
            _css.AppendLine("  margin-top: 10px;");
            _css.AppendLine("}");
            _css.AppendLine(".price {");
            _css.AppendLine("  font-size: 20px;");
            _css.AppendLine("  font-weight: bold;");
            _css.AppendLine("  color: #e74c3c;");
            _css.AppendLine("}");
            _css.AppendLine(".stock {");
            _css.AppendLine("  color: #666;");
            _css.AppendLine("  font-size: 14px;");
            _css.AppendLine("}");
            _css.AppendLine();

            // Card Footer
            _css.AppendLine(".card-footer {");
            _css.AppendLine("  padding: 12px 15px;");
            _css.AppendLine("  background: #f8f9fa;");
            _css.AppendLine("  border-top: 1px solid #eee;");
            _css.AppendLine("  display: flex;");
            _css.AppendLine("  gap: 8px;");
            _css.AppendLine("  justify-content: center;");
            _css.AppendLine("}");
            _css.AppendLine(".card-footer .btn {");
            _css.AppendLine("  flex: 1;");
            _css.AppendLine("  padding: 6px 12px;");
            _css.AppendLine("  font-size: 13px;");
            _css.AppendLine("}");
            _css.AppendLine();

            // Empty State
            _css.AppendLine(".empty-state {");
            _css.AppendLine("  text-align: center;");
            _css.AppendLine("  padding: 40px;");
            _css.AppendLine("  color: #999;");
            _css.AppendLine("}");
            _css.AppendLine();

            // Loading State
            _css.AppendLine(".loading-state {");
            _css.AppendLine("  text-align: center;");
            _css.AppendLine("  padding: 40px;");
            _css.AppendLine("  color: #666;");
            _css.AppendLine("}");
            _css.AppendLine(".spinner {");
            _css.AppendLine("  display: inline-block;");
            _css.AppendLine("  width: 30px;");
            _css.AppendLine("  height: 30px;");
            _css.AppendLine("  border: 3px solid #f3f3f3;");
            _css.AppendLine("  border-top: 3px solid #007bff;");
            _css.AppendLine("  border-radius: 50%;");
            _css.AppendLine("  animation: spin 1s linear infinite;");
            _css.AppendLine("}");
            _css.AppendLine("@keyframes spin {");
            _css.AppendLine("  0% { transform: rotate(0deg); }");
            _css.AppendLine("  100% { transform: rotate(360deg); }");
            _css.AppendLine("}");
            _css.AppendLine();

            // Responsive
            _css.AppendLine("@media (max-width: 1200px) {");
            _css.AppendLine("  .product-grid { grid-template-columns: repeat(3, 1fr); }");
            _css.AppendLine("}");
            _css.AppendLine("@media (max-width: 768px) {");
            _css.AppendLine("  .product-grid { grid-template-columns: repeat(2, 1fr); }");
            _css.AppendLine("}");
            _css.AppendLine("@media (max-width: 480px) {");
            _css.AppendLine("  .product-grid { grid-template-columns: 1fr; }");
            _css.AppendLine("}");
            _css.AppendLine();

            return this;
        }

        public string Build()
        {
            return _css.ToString();
        }
    }
}