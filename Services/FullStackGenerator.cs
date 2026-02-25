using AngularGenerator.Core.Models;
using AngularGenerator.Services.Builders;

namespace AngularGenerator.Services
{
    /// <summary>
    /// FullStackGenerator - Now using Builder Pattern instead of Razor Templates
    /// </summary>
    public class FullStackGenerator
    {
        private readonly DbSchemaServiceFactory _dbFactory;
        private readonly AngularComponentFactory _factory;

        public FullStackGenerator(DbSchemaServiceFactory dbFactory, AngularComponentFactory factory)
        {
            _dbFactory = dbFactory; 
            _factory = factory;
        }

        /// <summary>
        /// Generate Angular component using Builder Pattern (OOP approach)
        /// Now supports UI Layout and CSS Framework selection
        /// </summary>
        public async Task<ComponentDefinition> GenerateAsync(
            string tableName,
            List<string> selectedFields,
            bool isGet, bool isGetById, bool isPost, bool isUpdate, bool isDelete,
            UILayoutType layoutType = UILayoutType.TableView,
            CSSFramework cssFramework = CSSFramework.BasicCSS,
            string apiBaseUrl = "",
            string componentName = "",
            bool separateInterface = false)
        {
            // 1. Get schema from database
            var columns = _dbFactory.GetCurrentService().GetSchema(tableName);
            var fullModel = _factory.Create(tableName, columns);

            if (!string.IsNullOrEmpty(componentName))
            {
                var formattedName = char.ToUpper(componentName[0]) + componentName.Substring(1);
                fullModel.EntityName = formattedName;
                fullModel.Selector = componentName.ToLower();
            }

            // 2. หา Primary Key field จาก fullModel
            var pkField = fullModel.Fields.FirstOrDefault(f => f.IsPrimaryKey);
            
            // 3. Create model for generation (apply field selection)
            var generationModel = new ComponentDefinition
            {
                EntityName = fullModel.EntityName,
                Selector = fullModel.Selector,
                PrimaryKeyName = fullModel.PrimaryKeyName,
                Fields = (selectedFields != null && selectedFields.Count > 0)
                    ? fullModel.Fields.Where(f => selectedFields.Contains(f.FieldName)).ToList()
                    : fullModel.Fields,

                // Apply CRUD options
                IsGet = isGet,
                IsGetById = isGetById,
                IsPost = isPost,
                IsUpdate = isUpdate,
                IsDelete = isDelete,

                LayoutType = layoutType,
                CssFramework = cssFramework,
                SeparateInterface = separateInterface
            };

            // 4. ตรวจสอบว่า PK field ถูกรวมอยู่ใน selectedFields หรือไม่
            // ถ้าไม่มี ให้เพิ่มเข้าไปเพราะ PK จำเป็นสำหรับ CRUD operations
            if (pkField != null && !generationModel.Fields.Any(f => f.IsPrimaryKey))
            {
                generationModel.Fields.Insert(0, pkField);
            }

            var builder = new ComponentBuilder(generationModel);

            generationModel.GeneratedTs = builder.BuildTypeScript();
            generationModel.GeneratedService = builder.BuildService(apiBaseUrl);
            generationModel.GeneratedHtml = builder.BuildHtml();
            
            // Generate separate interface file if requested
            if (separateInterface)
            {
                generationModel.GeneratedInterface = builder.BuildInterface();
            }
            generationModel.GeneratedCss = builder.BuildCss();

            // Return to controller for display
            return await Task.FromResult(generationModel);
        }
    }
}