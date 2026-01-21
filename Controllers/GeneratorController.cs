using AngularGenerator.Services;
using AngularGenerator.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AngularGenerator.Controllers
{
    public class GeneratorController : Controller
    {
        private readonly FullStackGenerator _generator;
        private readonly DbSchemaService _dbService;
        private readonly AngularComponentFactory _factory;

        public GeneratorController(FullStackGenerator generator, DbSchemaService dbService, AngularComponentFactory factory)
        {
            _generator = generator;
            _dbService = dbService;
            _factory = factory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var defaultModel = new ComponentDefinition
            {
                IsGet = true,
                IsGetById = true,
                IsPost = true,
                IsUpdate = true,
                IsDelete = true
            };

            return View(defaultModel);
        }

        [HttpGet]
        public IActionResult GetColumns(string tableName)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName)) 
                    return Json(new { success = false, message = "Table name is empty" });
                
                var columns = _dbService.GetSchema(tableName);
                if (columns == null || !columns.Any()) 
                    return Json(new { success = false, message = "Table not found." });

                var componentDef = _factory.Create(tableName, columns);
                return Json(new { success = true, fields = componentDef.Fields });
            }
            catch (Exception ex) 
            { 
                return Json(new { success = false, message = ex.Message }); 
            }
        }

        [HttpGet]
        public IActionResult CheckDbConnection()
        {
            try 
            { 
                return Json(new { success = _dbService.TestConnection() }); 
            }
            catch (Exception ex) 
            { 
                return Json(new { success = false, message = ex.Message }); 
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(
            string tableName,
            string componentName,
            string apiBaseUrl,
            string generationMode,
            string layoutType,
            string cssFramework,
            bool separateInterface,
            List<string> selectedFields,
            bool IsGet, bool IsGetById, bool IsPost, bool IsUpdate, bool IsDelete)
        {
            ViewBag.LastTableName = tableName;
            ViewBag.SelectedFields = selectedFields;
            ViewBag.ApiBaseUrl = apiBaseUrl ?? "/api";
            ViewBag.GenerationMode = generationMode ?? "Report";
            ViewBag.LayoutType = layoutType;
            ViewBag.CssFramework = cssFramework;
            ViewBag.SeparateInterface = separateInterface;

            try
            {
                if (string.IsNullOrEmpty(tableName)) 
                    throw new Exception("Please enter a table name.");

                // ถ้าเลือก Report Mode ให้ตั้งค่า CRUD เป็น Get อย่างเดียว
                if (generationMode == "Report")
                {
                    IsGet = true;
                    IsGetById = false;
                    IsPost = false;
                    IsUpdate = false;
                    IsDelete = false;
                }

                // Parse Layout Type (default: TableView)
                var selectedLayoutType = Enum.TryParse<UILayoutType>(layoutType, out var parsedLayout) 
                    ? parsedLayout 
                    : UILayoutType.TableView;

                // Parse CSS Framework (default: Bootstrap)
                var selectedCssFramework = Enum.TryParse<CSSFramework>(cssFramework, out var parsedCss) 
                    ? parsedCss 
                    : CSSFramework.Bootstrap;

                var result = await _generator.GenerateAsync(
                    tableName, selectedFields,
                    IsGet, IsGetById, IsPost, IsUpdate, IsDelete,
                    layoutType: selectedLayoutType,
                    cssFramework: selectedCssFramework,
                    apiBaseUrl: apiBaseUrl ?? "/api",
                    componentName: componentName ?? "",
                    separateInterface: separateInterface
                );

                // Validate: ต้องมี PK ถ้าเลือก GetById, Update, หรือ Delete
                if ((IsGetById || IsUpdate || IsDelete) && string.IsNullOrEmpty(result.PrimaryKeyName))
                {
                    throw new Exception("⚠️ Table ไม่มี Primary Key!\n\nไม่สามารถใช้ GetById, Update หรือ Delete ได้ เนื่องจากต้องใช้ Primary Key ในการระบุข้อมูล");
                }

                result.IsGet = IsGet;
                result.IsGetById = IsGetById;
                result.IsPost = IsPost;
                result.IsUpdate = IsUpdate;
                result.IsDelete = IsDelete;

                return View(result);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View();
            }
        }
    }
}
