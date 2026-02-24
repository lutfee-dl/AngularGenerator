using AngularGenerator.Services;
using AngularGenerator.Core.Models;
using AngularGenerator.Services.Builders;
using Microsoft.AspNetCore.Mvc;

namespace AngularGenerator.Controllers
{
    public class GeneratorController : Controller
    {
        private readonly FullStackGenerator _generator;
        private readonly DbSchemaServiceFactory _dbFactory;
        private readonly AngularComponentFactory _factory;
        private readonly JsonSchemaService _jsonSchemaService;
        private readonly DatabaseConfigService _configService;

        public GeneratorController(
            FullStackGenerator generator, 
            DbSchemaServiceFactory dbFactory, 
            AngularComponentFactory factory, 
            JsonSchemaService jsonSchemaService,
            DatabaseConfigService configService)
        {
            _generator = generator;
            _dbFactory = dbFactory;
            _factory = factory;
            _jsonSchemaService = jsonSchemaService;
            _configService = configService;
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
                
                
                var service = _dbFactory.GetCurrentService();
                
                var columns = service.GetSchema(tableName);
                
                if (columns == null || !columns.Any()) 
                    return Json(new { success = false, message = "Table not found or has no columns." });

                var componentDef = _factory.Create(tableName, columns);
                
                return Json(new { success = true, fields = componentDef.Fields });
            }
            catch (Exception ex) 
            {
                return Json(new { success = false, message = ex.Message }); 
            }
        }

        [HttpGet]
        public IActionResult GetTables()
        {
            try
            {
                var dbService = _dbFactory.GetCurrentService();
                var tables = dbService.GetTables().ToList();
                
                return Json(new 
                { 
                    success = true, 
                    tables = tables,
                    databaseName = dbService.GetDatabaseName(),
                    dbType = dbService.DbType.ToString()
                });
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
                var dbService = _dbFactory.GetCurrentService();
                return Json(new { success = dbService.TestConnection(), dbType = dbService.DbType.ToString(), databaseName = dbService.GetDatabaseName() }); 
            }
            catch (Exception ex) 
            { 
                return Json(new { success = false, message = ex.Message }); 
            }
        }

        [HttpPost]
        public IActionResult TestConnection([FromBody] TestConnectionRequest request)
        {
            try
            {
                // Log for debugging
                Console.WriteLine($"TestConnection: DbType={request.DbType}, ConnString={request.ConnectionString?.Substring(0, Math.Min(50, request.ConnectionString?.Length ?? 0))}...");

                if (string.IsNullOrEmpty(request.ConnectionString))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Connection string is empty"
                    });
                }

                // ใช้ TestConnectionOnly เพื่อไม่ให้เปลี่ยน database ปัจจุบัน
                var service = _dbFactory.TestConnectionOnly(request.DbType, request.ConnectionString);
                bool isConnected = service.TestConnection();

                if (isConnected)
                {
                    var dbName = service.GetDatabaseName();
                    Console.WriteLine($"Connection successful: {dbName}");
                    return Json(new
                    {
                        success = true,
                        message = "Connection successful",
                        databaseName = dbName
                    });
                }
                else
                {
                    Console.WriteLine("Connection failed: TestConnection returned false");
                    return Json(new
                    {
                        success = false,
                        message = "Connection failed - Unable to open connection"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestConnection Error: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new
                {
                    success = false,
                    message = $"{ex.GetType().Name}: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public IActionResult SwitchDatabase([FromBody] TestConnectionRequest request)
        {
            try
            {
                // Create and test new service
                var service = _dbFactory.CreateService(request.DbType, request.ConnectionString);
                bool isConnected = service.TestConnection();

                if (!isConnected)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot connect to database"
                    });
                }

                // Service is already switched in factory
                return Json(new
                {
                    success = true,
                    message = "Database switched successfully",
                    databaseName = service.GetDatabaseName(),
                    dbType = service.DbType.ToString()
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ParseApiSchema([FromBody] JsonSchemaRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ApiUrl))
                    return Json(new { success = false, errorMessage = "API URL is required" });

                var result = await _jsonSchemaService.ParseFromApiAsync(
                    request.ApiUrl, 
                    request.HttpMethod ?? "GET", 
                    request.Headers
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ParseJsonSchema([FromBody] JsonSchemaRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.JsonContent))
                    return Json(new { success = false, errorMessage = "JSON content is required" });

                var result = _jsonSchemaService.ParseFromJson(request.JsonContent);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateFromParsedFields(
            [FromBody] GenerateFromFieldsRequest request)
        {
            try
            {
                if (request.Fields == null || !request.Fields.Any())
                    return Json(new { success = false, errorMessage = "No fields provided" });

                var entityName = request.EntityName ?? "Item";
                
                // Convert to DbColumnInfo for factory
                var columns = request.Fields.Select(f => new DbColumnInfo
                {
                    ColumnName = f.FieldName,
                    DataType = MapTsTypeToSqlType(f.TsType, f.UIControl),
                    IsNullable = f.IsRequired ? "NO" : "YES",
                    IsPrimaryKey = f.IsPrimaryKey
                }).ToList();

                // Create component definition
                var fullModel = _factory.Create(entityName, columns);

                // Apply CRUD settings
                if (request.GenerationMode == "Report")
                {
                    request.IsGet = true;
                    request.IsGetById = false;
                    request.IsPost = false;
                    request.IsUpdate = false;
                    request.IsDelete = false;
                }

                // Filter selected fields
                var selectedFieldNames = request.SelectedFields ?? request.Fields.Select(f => f.FieldName).ToList();
                var pkField = fullModel.Fields.FirstOrDefault(f => f.IsPrimaryKey);
                
                var generationModel = new ComponentDefinition
                {
                    EntityName = fullModel.EntityName,
                    Selector = fullModel.Selector,
                    PrimaryKeyName = fullModel.PrimaryKeyName,
                    Fields = fullModel.Fields.Where(f => selectedFieldNames.Contains(f.FieldName)).ToList(),
                    IsGet = request.IsGet,
                    IsGetById = request.IsGetById,
                    IsPost = request.IsPost,
                    IsUpdate = request.IsUpdate,
                    IsDelete = request.IsDelete,
                    LayoutType = Enum.TryParse<UILayoutType>(request.LayoutType, out var lt) ? lt : UILayoutType.TableView,
                    CssFramework = Enum.TryParse<CSSFramework>(request.CssFramework, out var cf) ? cf : CSSFramework.BasicCSS,
                    SeparateInterface = request.SeparateInterface
                };

                // Ensure PK is included
                if (pkField != null && !generationModel.Fields.Any(f => f.IsPrimaryKey))
                {
                    generationModel.Fields.Insert(0, pkField);
                }

                var builder = new ComponentBuilder(generationModel);

                generationModel.GeneratedTs = builder.BuildTypeScript();
                generationModel.GeneratedService = builder.BuildService(request.ApiBaseUrl ?? "http://localhost:3000/api");
                generationModel.GeneratedHtml = builder.BuildHtml();
                
                if (request.SeparateInterface)
                {
                    generationModel.GeneratedInterface = builder.BuildInterface();
                }
                generationModel.GeneratedCss = builder.BuildCss();

                return Json(new { 
                    success = true, 
                    html = generationModel.GeneratedHtml,
                    ts = generationModel.GeneratedTs,
                    service = generationModel.GeneratedService,
                    css = generationModel.GeneratedCss,
                    interfaceCode = generationModel.GeneratedInterface,
                    selector = generationModel.Selector,
                    entityName = generationModel.EntityName,
                    isGet = generationModel.IsGet
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        private string MapTsTypeToSqlType(string tsType, ControlType controlType)
        {
            return tsType.ToLower() switch
            {
                "number" => controlType == ControlType.Number ? "int" : "decimal",
                "boolean" => "bit",
                "date" => "datetime",
                _ => "nvarchar"
            };
        }

        [HttpPost]
        public async Task<IActionResult> GenerateFromSql(
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
            try
            {
                if (string.IsNullOrEmpty(tableName))
                    return Json(new { success = false, errorMessage = "Please enter a table name." });

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
                    apiBaseUrl: apiBaseUrl ?? "http://localhost:3000/api/exemples",
                    componentName: componentName ?? "",
                    separateInterface: separateInterface
                );

                // Validate: ต้องมี PK ถ้าเลือก GetById, Update, หรือ Delete
                if ((IsGetById || IsUpdate || IsDelete) && string.IsNullOrEmpty(result.PrimaryKeyName))
                {
                    return Json(new { success = false, errorMessage = "⚠️ Table ไม่มี Primary Key! ไม่สามารถใช้ GetById, Update หรือ Delete ได้" });
                }

                return Json(new
                {
                    success = true,
                    html = result.GeneratedHtml,
                    ts = result.GeneratedTs,
                    service = result.GeneratedService,
                    css = result.GeneratedCss,
                    interfaceCode = result.GeneratedInterface,
                    selector = result.Selector,
                    entityName = result.EntityName,
                    layoutType = result.LayoutType.ToString(),
                    cssFramework = result.CssFramework.ToString(),
                    isGet = result.IsGet
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
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
            ViewBag.ApiBaseUrl = apiBaseUrl ?? "http://localhost:3000/api/exemples";
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
                    apiBaseUrl: apiBaseUrl ?? "http://localhost:3000/api/exemples",
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

        // ==========================================
        // Database Configuration Management APIs
        // ==========================================

        [HttpGet]
        public IActionResult GetSavedConfigurations()
        {
            try
            {
                var configs = _configService.GetAllConfigurations();
                
                // Mask passwords for security
                var maskedConfigs = configs.Select(c => new
                {
                    c.Name,
                    c.DbType,
                    ConnectionString = MaskConnectionString(c.ConnectionString),
                    c.IsDefault,
                    c.Description,
                    c.LastUsed
                });

                return Json(new { success = true, configurations = maskedConfigs });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetSavedConfigurations Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveDatabaseConfig([FromBody] SaveConfigRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return Json(new { success = false, message = "Configuration name is required" });
                }

                if (string.IsNullOrEmpty(request.ConnectionString))
                {
                    return Json(new { success = false, message = "Connection string is required" });
                }

                var config = new DatabaseConfig
                {
                    Name = request.Name,
                    DbType = request.DbType,
                    ConnectionString = request.ConnectionString,
                    IsDefault = request.IsDefault,
                    Description = request.Description
                };

                bool saved = _configService.SaveConfiguration(config);

                if (saved)
                {
                    return Json(new { success = true, message = $"Configuration '{request.Name}' saved successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save configuration" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveDatabaseConfig Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult LoadDatabaseConfig([FromBody] Dictionary<string, string> data)
        {
            try
            {
                if (!data.ContainsKey("name"))
                {
                    return Json(new { success = false, message = "Configuration name is required" });
                }

                var configName = data["name"];
                var config = _configService.GetConfiguration(configName);

                if (config == null)
                {
                    return Json(new { success = false, message = "Configuration not found" });
                }

                // Update last used
                _configService.UpdateLastUsed(configName);

                return Json(new
                {
                    success = true,
                    config = new
                    {
                        config.Name,
                        config.DbType,
                        config.ConnectionString,
                        config.Description
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadDatabaseConfig Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteDatabaseConfig([FromBody] DeleteConfigRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return Json(new { success = false, message = "Configuration name is required" });
                }

                bool deleted = _configService.DeleteConfiguration(request.Name);

                if (deleted)
                {
                    return Json(new { success = true, message = $"Configuration '{request.Name}' deleted" });
                }
                else
                {
                    return Json(new { success = false, message = "Configuration not found" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteDatabaseConfig Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ExportConfigurations(bool includePasswords = false)
        {
            try
            {
                var json = _configService.ExportConfigurations(includePasswords);
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExportConfigurations Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to mask passwords in connection strings
        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            var patterns = new[] { 
                ("Password=", ";"), 
                ("Pwd=", ";"), 
                ("password=", ";"), 
                ("pwd=", ";"),
                ("User Id=", ";"),
                ("Uid=", ";")
            };

            var masked = connectionString;
            foreach (var (start, end) in patterns)
            {
                var index = masked.IndexOf(start, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var valueStart = index + start.Length;
                    var endIndex = masked.IndexOf(end, valueStart);
                    if (endIndex < 0) endIndex = masked.Length;

                    var before = masked.Substring(0, valueStart);
                    var after = masked.Substring(endIndex);
                    masked = before + "****" + after;
                }
            }

            return masked;
        }

    }
}
