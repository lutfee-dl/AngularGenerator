using System.Text;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder for Angular Service (TypeScript)
    /// </summary>
    public class ServiceBuilder : ICodeBuilder<ServiceBuilder>
    {
        private readonly ComponentDefinition _definition;
        private readonly List<ImportSegment> _imports = new();
        private readonly List<MethodSegment> _methods = new();
        private string _baseUrl = "http://localhost:3000/api/products";
        private string _endpoint = "";
        
        public ServiceBuilder(ComponentDefinition definition)
        {
            _definition = definition;
            InitializeDefaults();
        }
        
        private void InitializeDefaults()
        {
            AddImport(new[] { "Injectable", "inject" }, "@angular/core");
            AddImport(new[] { "HttpClient", "HttpErrorResponse" }, "@angular/common/http");
            AddImport(new[] { "Observable", "throwError" }, "rxjs");
            AddImport(new[] { "map", "catchError" }, "rxjs/operators");
            
            if (_definition.SeparateInterface)
            {
                AddImport(new[] { $"{_definition.EntityName}Model" }, $"./{_definition.EntityName.ToLower()}.interface");
            }
        }
        
        public ServiceBuilder AddImport(string[] items, string from)
        {
            _imports.Add(new ImportSegment { Items = items.ToList(), From = from });
            return this;
        }
        
        public ServiceBuilder WithBaseUrl(string apiBaseUrl)
        {
            
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                _baseUrl = "http://localhost:3000/api/exemples";
                _endpoint = "";
                return this;
            }
            
            var startBrace = apiBaseUrl.IndexOf('{');
            var endBrace = apiBaseUrl.IndexOf('}');
            
            if (startBrace >= 0 && endBrace > startBrace)
            {
                _endpoint = apiBaseUrl.Substring(startBrace + 1, endBrace - startBrace - 1);
                _baseUrl = apiBaseUrl.Substring(0, startBrace).TrimEnd('/');
            }
            else
            {
                var trimmedUrl = apiBaseUrl.TrimEnd('/');
                var lastSlashIndex = trimmedUrl.LastIndexOf('/');
                
                if (lastSlashIndex > 0)
                {
                    _endpoint = trimmedUrl.Substring(lastSlashIndex + 1);
                    _baseUrl = trimmedUrl.Substring(0, lastSlashIndex);
                }
                else
                {
                    _baseUrl = trimmedUrl;
                    _endpoint = "";
                }
            }
            
            return this;
        }
        
        public ServiceBuilder WithGetAll()
        {
            var apiPath = !string.IsNullOrEmpty(_endpoint) ? _endpoint : _definition.EntityName.ToLower();
            
            _methods.Add(new MethodSegment
            {
                Name = "getAll",
                ReturnType = $"Observable<{_definition.EntityName}Model[]>",
                BodyLines = new List<string>
                {
                    $"return this.http.get<{_definition.EntityName}Model[]>(`${{this.baseUrl}}/{apiPath}`).pipe(",
                    $"  map((data) => data.map((item) => this.map{_definition.PrimaryKeyName}(item))),",
                    "  catchError(this.handleError),",
                    ");"
                }
            });
            return this;
        }
        
        public ServiceBuilder WithGetById()
        {
            var apiPath = !string.IsNullOrEmpty(_endpoint) ? _endpoint : _definition.EntityName.ToLower();
            
            _methods.Add(new MethodSegment
            {
                Name = "getById",
                Parameters = new List<string> { "id: string | number" },
                ReturnType = $"Observable<{_definition.EntityName}Model>",
                BodyLines = new List<string>
                {
                    $"return this.http.get<{_definition.EntityName}Model>(`${{this.baseUrl}}/{apiPath}/${{id}}`).pipe(",
                    $"  map((item) => this.map{_definition.PrimaryKeyName}(item)),",
                    "  catchError(this.handleError),",
                    ");"
                }
            });
            return this;
        }
        
        public ServiceBuilder WithCreate()
        {
            var apiPath = !string.IsNullOrEmpty(_endpoint) ? _endpoint : _definition.EntityName.ToLower();
            
            _methods.Add(new MethodSegment
            {
                Name = "create",
                Parameters = new List<string> { $"data: {_definition.EntityName}Model" },
                ReturnType = $"Observable<{_definition.EntityName}Model>",
                BodyLines = new List<string>
                {
                    "return this.http",
                    $"  .post<{_definition.EntityName}Model>(`${{this.baseUrl}}/{apiPath}`, data)",
                    "  .pipe(catchError(this.handleError));"
                }
            });
            return this;
        }
        
        public ServiceBuilder WithUpdate()
        {
            var apiPath = !string.IsNullOrEmpty(_endpoint) ? _endpoint : _definition.EntityName.ToLower();
            
            _methods.Add(new MethodSegment
            {
                Name = "update",
                Parameters = new List<string> { "id: string | number", $"data: {_definition.EntityName}Model" },
                ReturnType = $"Observable<{_definition.EntityName}Model>",
                BodyLines = new List<string>
                {
                    "return this.http",
                    $"  .put<{_definition.EntityName}Model>(`${{this.baseUrl}}/{apiPath}/${{id}}`, data)",
                    "  .pipe(catchError(this.handleError));"
                }
            });
            return this;
        }
        
        public ServiceBuilder WithDelete()
        {
            var apiPath = !string.IsNullOrEmpty(_endpoint) ? _endpoint : _definition.EntityName.ToLower();
            
            _methods.Add(new MethodSegment
            {
                Name = "delete",
                Parameters = new List<string> { "id: string | number" },
                ReturnType = "Observable<void>",
                BodyLines = new List<string>
                {
                    "return this.http",
                    $"  .delete<void>(`${{this.baseUrl}}/{apiPath}/${{id}}`)",
                    "  .pipe(catchError(this.handleError));"
                }
            });
            return this;
        }
        
        public string Build()
        {
            var sb = new StringBuilder();
            
            // Imports
            foreach (var import in _imports.DistinctBy(x => x.From))
            {
                var allItems = _imports.Where(x => x.From == import.From)
                                      .SelectMany(x => x.Items)
                                      .Distinct();
                sb.AppendLine($"import {{ {string.Join(", ", allItems)} }} from '{import.From}';");
            }
            
            sb.AppendLine();
            
            // Model interface (only if not separated)
            if (!_definition.SeparateInterface)
            {
                sb.AppendLine($"export interface {_definition.EntityName}Model {{");
                foreach (var field in _definition.Fields)
                {
                    var optional = field.IsRequired ? "" : "?";
                    sb.AppendLine($"  {field.FieldName}{optional}: {field.TsType};");
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }
            
            // Service class
            sb.AppendLine("@Injectable({");
            sb.AppendLine("  providedIn: 'root'");
            sb.AppendLine("})");
            sb.AppendLine($"export class {_definition.EntityName}Service {{");
            sb.AppendLine();
            sb.AppendLine("  private http = inject(HttpClient);");
            sb.AppendLine($"  private baseUrl = '{_baseUrl}';");
            sb.AppendLine();
            
            // Public Methods
            foreach (var method in _methods)
            {
                sb.AppendLine(method.Build(1));
                sb.AppendLine();
            }
            
            // Private Helper Methods
            sb.AppendLine($"  private map{_definition.PrimaryKeyName}(item: {_definition.EntityName}Model): {_definition.EntityName}Model {{");
            sb.AppendLine("    return {");
            sb.AppendLine("      ...item,");
            sb.AppendLine($"      {_definition.PrimaryKeyName}: item.{_definition.PrimaryKeyName},");
            sb.AppendLine("    };");
            sb.AppendLine("  }");
            sb.AppendLine();
            
            sb.AppendLine("  private handleError(error: HttpErrorResponse) {");
            sb.AppendLine("    let errorMessage = 'เกิดข้อผิดพลาดที่ไม่ทราบสาเหตุ กรุณาลองใหม่อีกครั้ง';");
            sb.AppendLine();
            sb.AppendLine("    if (error.status === 0) {");
            sb.AppendLine("      errorMessage = 'ไม่สามารถเชื่อมต่อเซิร์ฟเวอร์ได้ กรุณาตรวจสอบอินเทอร์เน็ต';");
            sb.AppendLine("    } else if (error.status === 404) {");
            sb.AppendLine("      errorMessage = 'ไม่พบข้อมูลที่ต้องการ';");
            sb.AppendLine("    } else if (error.status === 500) {");
            sb.AppendLine("      errorMessage = 'เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์ กรุณาลองใหม่อีกครั้งในภายหลัง';");
            sb.AppendLine("    }");
            sb.AppendLine("    return throwError(() => new Error(errorMessage));");
            sb.AppendLine("  }");
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        public ServiceBuilder Reset()
        {
            _imports.Clear();
            _methods.Clear();
            InitializeDefaults();
            return this;
        }
    }
}
