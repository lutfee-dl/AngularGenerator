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
        private string _baseUrl = "/api";
        
        public ServiceBuilder(ComponentDefinition definition)
        {
            _definition = definition;
            InitializeDefaults();
        }
        
        private void InitializeDefaults()
        {
            AddImport(new[] { "Injectable", "inject" }, "@angular/core");
            AddImport(new[] { "HttpClient" }, "@angular/common/http");
            AddImport(new[] { "Observable" }, "rxjs");
        }
        
        public ServiceBuilder AddImport(string[] items, string from)
        {
            _imports.Add(new ImportSegment { Items = items.ToList(), From = from });
            return this;
        }
        
        public ServiceBuilder WithBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
            return this;
        }
        
        public ServiceBuilder WithGetAll()
        {
            _methods.Add(new MethodSegment
            {
                Name = "getAll",
                ReturnType = $"Observable<{_definition.EntityName}Model[]>",
                BodyLines = new List<string>
                {
                    $"return this.http.get<{_definition.EntityName}Model[]>(`${{this.baseUrl}}/{_definition.EntityName.ToLower()}`);"
                }
            });
            return this;
        }
        
        public ServiceBuilder WithGetById()
        {
            _methods.Add(new MethodSegment
            {
                Name = "getById",
                Parameters = new List<string> { "id: any" },
                ReturnType = $"Observable<{_definition.EntityName}Model>",
                BodyLines = new List<string>
                {
                    $"return this.http.get<{_definition.EntityName}Model>(`${{this.baseUrl}}/{_definition.EntityName.ToLower()}/${{id}}`);"
                }
            });
            return this;
        }
        
        public ServiceBuilder WithCreate()
        {
            _methods.Add(new MethodSegment
            {
                Name = "create",
                Parameters = new List<string> { $"data: {_definition.EntityName}Model" },
                ReturnType = $"Observable<{_definition.EntityName}Model>",
                BodyLines = new List<string>
                {
                    $"return this.http.post<{_definition.EntityName}Model>(`${{this.baseUrl}}/{_definition.EntityName.ToLower()}`, data);"
                }
            });
            return this;
        }
        
        public ServiceBuilder WithUpdate()
        {
            _methods.Add(new MethodSegment
            {
                Name = "update",
                Parameters = new List<string> { "id: any", $"data: {_definition.EntityName}Model" },
                ReturnType = $"Observable<{_definition.EntityName}Model>",
                BodyLines = new List<string>
                {
                    $"return this.http.put<{_definition.EntityName}Model>(`${{this.baseUrl}}/{_definition.EntityName.ToLower()}/${{id}}`, data);"
                }
            });
            return this;
        }
        
        public ServiceBuilder WithDelete()
        {
            _methods.Add(new MethodSegment
            {
                Name = "delete",
                Parameters = new List<string> { "id: any" },
                ReturnType = "Observable<void>",
                BodyLines = new List<string>
                {
                    $"return this.http.delete<void>(`${{this.baseUrl}}/{_definition.EntityName.ToLower()}/${{id}}`);"
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
            
            // Model interface
            sb.AppendLine($"export interface {_definition.EntityName}Model {{");
            foreach (var field in _definition.Fields)
            {
                var optional = field.IsRequired ? "" : "?";
                sb.AppendLine($"  {field.FieldName}{optional}: {field.TsType};");
            }
            sb.AppendLine("}");
            sb.AppendLine();
            
            // Service class
            sb.AppendLine("@Injectable({");
            sb.AppendLine("  providedIn: 'root'");
            sb.AppendLine("})");
            sb.AppendLine($"export class {_definition.EntityName}Service {{");
            sb.AppendLine();
            sb.AppendLine("  private http = inject(HttpClient);");
            sb.AppendLine($"  private baseUrl = '{_baseUrl}';");
            sb.AppendLine();
            
            // Methods
            foreach (var method in _methods)
            {
                sb.AppendLine(method.Build(1));
                sb.AppendLine();
            }
            
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
