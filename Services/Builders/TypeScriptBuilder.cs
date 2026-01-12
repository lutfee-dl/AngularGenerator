using System.Text;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder for TypeScript component code using fluent API
    /// </summary>
    public class TypeScriptBuilder : ICodeBuilder<TypeScriptBuilder>
    {
        private readonly ComponentDefinition _definition;
        private readonly List<ImportSegment> _imports = new();
        private readonly List<PropertySegment> _properties = new();
        private readonly List<MethodSegment> _methods = new();
        private readonly List<string> _componentDecorator = new();
        
        public TypeScriptBuilder(ComponentDefinition definition)
        {
            _definition = definition;
            InitializeDefaults();
        }
        
        private void InitializeDefaults()
        {
            // Default Angular imports (v20+)
            AddImport(new[] { "Component", "inject", "signal" }, "@angular/core");
            
            // Default component decorator
            _componentDecorator.Add($"selector: '{_definition.Selector}'");
            _componentDecorator.Add("standalone: true");
            _componentDecorator.Add("imports: []");
            _componentDecorator.Add($"templateUrl: './{_definition.EntityName.ToLower()}.html'");
            _componentDecorator.Add($"styleUrls: ['./{_definition.EntityName.ToLower()}.css']");            
            // Add Material imports if using Angular Material
            if (_definition.CssFramework == CSSFramework.AngularMaterial)
            {
                AddImport(new[] { "MatTableModule", "MatButtonModule", "MatIconModule", "MatFormFieldModule", "MatInputModule" }, "@angular/material");
                
                var importsLine = _componentDecorator.FirstOrDefault(x => x.StartsWith("imports:"));
                if (importsLine != null)
                {
                    _componentDecorator.Remove(importsLine);
                    _componentDecorator.Add("imports: [MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule]");
                }
            }            
            // Add Material imports if using Angular Material
            if (_definition.CssFramework == CSSFramework.AngularMaterial)
            {
                AddImport(new[] { "MatTableModule", "MatButtonModule", "MatIconModule", "MatFormFieldModule", "MatInputModule" }, "@angular/material");
                
                var importsLine = _componentDecorator.FirstOrDefault(x => x.StartsWith("imports:"));
                if (importsLine != null)
                {
                    _componentDecorator.Remove(importsLine);
                    _componentDecorator.Add("imports: [MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule]");
                }
            }
        }
        
        public TypeScriptBuilder AddImport(string[] items, string from)
        {
            _imports.Add(new ImportSegment { Items = items.ToList(), From = from });
            return this;
        }
        
        public TypeScriptBuilder AddProperty(PropertySegment property)
        {
            _properties.Add(property);
            return this;
        }
        
        public TypeScriptBuilder AddMethod(MethodSegment method)
        {
            _methods.Add(method);
            return this;
        }
        
        public TypeScriptBuilder WithReactiveForms()
        {
            AddImport(new[] { "ReactiveFormsModule", "FormBuilder", "FormGroup", "Validators" }, "@angular/forms");
            
            // Update component decorator imports
            var importsLine = _componentDecorator.FirstOrDefault(x => x.StartsWith("imports:"));
            if (importsLine != null)
            {
                _componentDecorator.Remove(importsLine);
                
                if (_definition.CssFramework == CSSFramework.AngularMaterial)
                {
                    _componentDecorator.Add("imports: [ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule]");
                }
                else
                {
                    _componentDecorator.Add("imports: [ReactiveFormsModule]");
                }
            }
            
            // Add FormBuilder injection
            AddProperty(new PropertySegment 
            { 
                Name = "fb", 
                Type = "FormBuilder", 
                InitialValue = "inject(FormBuilder)",
                AccessModifier = "private"
            });
            
            AddProperty(new PropertySegment 
            { 
                Name = "form", 
                Type = "FormGroup",
                AccessModifier = "public"
            });
            
            return this;
        }
        
        public TypeScriptBuilder WithService()
        {
            AddImport(new[] { $"{_definition.EntityName}Service", $"{_definition.EntityName}Model" }, 
                      $"./{_definition.EntityName.ToLower()}.service");
            
            AddProperty(new PropertySegment 
            { 
                Name = "service", 
                Type = $"{_definition.EntityName}Service",
                InitialValue = $"inject({_definition.EntityName}Service)",
                AccessModifier = "private"
            });
            
            return this;
        }
        
        public TypeScriptBuilder WithGetAll()
        {
            AddProperty(new PropertySegment 
            { 
                Name = "dataList", 
                Type = $"{_definition.EntityName}Model[]",
                InitialValue = $"signal<{_definition.EntityName}Model[]>([])",
                IsSignal = false // already in InitialValue
            });
            
            AddProperty(new PropertySegment 
            { 
                Name = "isLoading", 
                Type = "boolean",
                InitialValue = "signal<boolean>(false)",
                IsSignal = false
            });
            
            // Detail Modal properties
            AddProperty(new PropertySegment
            {
                Name = "showDetailModal",
                Type = "boolean",
                InitialValue = "signal<boolean>(false)",
                IsSignal = false
            });
            
            AddProperty(new PropertySegment
            {
                Name = "selectedItem",
                Type = $"{_definition.EntityName}Model | null",
                InitialValue = $"signal<{_definition.EntityName}Model | null>(null)",
                IsSignal = false
            });
            
            var loadMethod = new MethodSegment
            {
                Name = "loadData",
                BodyLines = new List<string>
                {
                    "this.isLoading.set(true);",
                    "this.service.getAll().subscribe({",
                    "  next: (data) => {",
                    "    this.dataList.set(data);",
                    "    this.isLoading.set(false);",
                    "  },",
                    "  error: (err) => {",
                    "    console.error(err);",
                    "    this.isLoading.set(false);",
                    "  }",
                    "});"
                }
            };
            
            AddMethod(loadMethod);
            
            // View Detail method
            AddMethod(new MethodSegment
            {
                Name = "viewDetail",
                Parameters = new List<string> { $"item: {_definition.EntityName}Model" },
                BodyLines = new List<string>
                {
                    "this.selectedItem.set(item);",
                    "this.showDetailModal.set(true);"
                }
            });
            
            // Close Detail method
            AddMethod(new MethodSegment
            {
                Name = "closeDetail",
                BodyLines = new List<string>
                {
                    "this.showDetailModal.set(false);",
                    "this.selectedItem.set(null);"
                }
            });
            
            return this;
        }
        
        public TypeScriptBuilder WithCreate()
        {
            AddProperty(new PropertySegment 
            { 
                Name = "showModal", 
                Type = "boolean",
                InitialValue = "signal<boolean>(false)",
                IsSignal = false
            });
            
            AddProperty(new PropertySegment 
            { 
                Name = "isSaving", 
                Type = "boolean",
                InitialValue = "signal<boolean>(false)",
                IsSignal = false
            });
            
            var openCreateMethod = new MethodSegment
            {
                Name = "openCreate",
                BodyLines = new List<string>
                {
                    "this.form.reset();",
                    "this.showModal.set(true);"
                }
            };
            
            AddMethod(openCreateMethod);
            return this;
        }
        
        public TypeScriptBuilder WithUpdate()
        {
            if (!_properties.Any(p => p.Name == "showModal"))
            {
                AddProperty(new PropertySegment 
                { 
                    Name = "showModal", 
                    Type = "boolean",
                    InitialValue = "signal<boolean>(false)",
                    IsSignal = false
                });
            }
            
            AddProperty(new PropertySegment 
            { 
                Name = "isEditMode", 
                Type = "boolean",
                InitialValue = "signal<boolean>(false)",
                IsSignal = false
            });
            
            var editMethod = new MethodSegment
            {
                Name = "onEdit",
                Parameters = new List<string> { $"item: {_definition.EntityName}Model" },
                BodyLines = new List<string>
                {
                    "this.isEditMode.set(true);",
                    "this.form.patchValue(item);",
                    "this.showModal.set(true);"
                }
            };
            
            AddMethod(editMethod);
            return this;
        }
        
        public TypeScriptBuilder WithFormSubmit()
        {
            var submitLines = new List<string>
            {
                "if (this.form.invalid) return;",
                "this.isSaving.set(true);",
                ""
            };
            
            if (_definition.IsUpdate && _definition.IsPost)
            {
                submitLines.Add($"const request$ = this.isEditMode()");
                submitLines.Add($"  ? this.service.update(this.form.value.{_definition.PrimaryKeyName}, this.form.value)");
                submitLines.Add($"  : this.service.create(this.form.value);");
            }
            else if (_definition.IsPost)
            {
                submitLines.Add("const request$ = this.service.create(this.form.value);");
            }
            else if (_definition.IsUpdate)
            {
                submitLines.Add($"const request$ = this.service.update(this.form.value.{_definition.PrimaryKeyName}, this.form.value);");
            }
            
            submitLines.Add("");
            submitLines.Add("request$.subscribe({");
            submitLines.Add("  next: () => {");
            
            if (_definition.IsGet)
                submitLines.Add("    this.loadData();");
            
            submitLines.Add("    this.isSaving.set(false);");
            submitLines.Add("    this.closeModal();");
            submitLines.Add("  },");
            submitLines.Add("  error: (err) => {");
            submitLines.Add("    console.error(err);");
            submitLines.Add("    this.isSaving.set(false);");
            submitLines.Add("  }");
            submitLines.Add("});");
            
            AddMethod(new MethodSegment
            {
                Name = "onSubmit",
                BodyLines = submitLines
            });
            
            AddMethod(new MethodSegment
            {
                Name = "closeModal",
                BodyLines = new List<string> { "this.showModal.set(false);" }
            });
            
            return this;
        }
        
        public TypeScriptBuilder WithDelete()
        {
            var deleteMethod = new MethodSegment
            {
                Name = "onDelete",
                Parameters = new List<string> { "id: any" },
                BodyLines = new List<string>
                {
                    "if (!confirm('Confirm delete?')) return;",
                    "this.service.delete(id).subscribe(() => {"
                }
            };
            
            if (_definition.IsGet)
                deleteMethod.BodyLines.Add("  this.loadData();");
            
            deleteMethod.BodyLines.Add("});");
            
            AddMethod(deleteMethod);
            return this;
        }
        
        public TypeScriptBuilder WithFormInit()
        {
            var formControls = new List<string>();
            
            if (!string.IsNullOrEmpty(_definition.PrimaryKeyName))
            {
                formControls.Add($"      {_definition.PrimaryKeyName}: [null]");
            }
            
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey))
            {
                var validators = field.IsRequired ? ", [Validators.required]" : "";
                formControls.Add($"      {field.FieldName}: [null{validators}]");
            }
            
            var initFormMethod = new MethodSegment
            {
                Name = "initForm",
                BodyLines = new List<string>
                {
                    "this.form = this.fb.group({",
                    string.Join(",\n", formControls),
                    "    });"
                }
            };
            
            AddMethod(initFormMethod);
            return this;
        }
        
        public TypeScriptBuilder WithColumnConfig()
        {
            var configLines = new List<string>();
            configLines.Add($"columnConfig = [");
            
            foreach (var field in _definition.Fields)
            {
                configLines.Add($"    {{ field: '{field.FieldName}', label: '{field.Label}', type: '{field.TsType}' }},");
            }
            
            configLines.Add("  ];");
            
            AddProperty(new PropertySegment
            {
                Name = "columnConfig",
                Type = "any[]",
                InitialValue = "[\n" + string.Join(",\n", _definition.Fields.Select(f => 
                    $"    {{ field: '{f.FieldName}', label: '{f.Label}', type: '{f.TsType}' }}")) + "\n  ]"
            });
            
            // เพิ่ม helper method สำหรับ type-safe field access (รองรับทั้ง camelCase และ PascalCase)
            AddMethod(new MethodSegment
            {
                Name = "getFieldValue",
                Parameters = new List<string> { $"item: {_definition.EntityName}Model", "fieldName: string" },
                ReturnType = "any",
                BodyLines = new List<string>
                {
                    "// ลองหา field ตามชื่อที่ระบุก่อน (camelCase)",
                    "if (fieldName in item) {",
                    "  return (item as any)[fieldName];",
                    "}",
                    "",
                    "// ถ้าไม่เจอ ลอง PascalCase (ตัวแรกเป็นตัวใหญ่)",
                    "const pascalCase = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);",
                    "if (pascalCase in item) {",
                    "  return (item as any)[pascalCase];",
                    "}",
                    "",
                    "// ไม่เจอเลย ลอง lowercase ทั้งหมด",
                    "const lowerCase = fieldName.toLowerCase();",
                    "const found = Object.keys(item).find(key => key.toLowerCase() === lowerCase);",
                    "return found ? (item as any)[found] : undefined;"
                }
            });
            
            // เพิ่ม trackBy function สำหรับ @for loop (รองรับทั้ง camelCase และ PascalCase)
            var pkField = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
            var pkFieldPascal = char.ToUpper(pkField[0]) + pkField.Substring(1);
            
            AddMethod(new MethodSegment
            {
                Name = "trackByFn",
                Parameters = new List<string> { "index: number", $"item: {_definition.EntityName}Model" },
                ReturnType = "any",
                BodyLines = new List<string>
                {
                    $"// ลองทั้ง camelCase และ PascalCase",
                    $"const pkValue = (item as any)['{pkField}'] ?? (item as any)['{pkFieldPascal}'];",
                    "return pkValue ?? index;"
                }
            });
            
            return this;
        }
        
        public TypeScriptBuilder WithNgOnInit()
        {
            var initLines = new List<string>();
            
            if (_definition.IsPost || _definition.IsUpdate)
                initLines.Add("this.initForm();");
            
            if (_definition.IsGet)
                initLines.Add("this.loadData();");
            
            AddMethod(new MethodSegment
            {
                Name = "constructor",
                BodyLines = initLines
            });
            
            return this;
        }
        
        public string Build()
        {
            var sb = new StringBuilder();
            
            foreach (var import in _imports.DistinctBy(x => x.From))
            {
                var allItems = _imports.Where(x => x.From == import.From)
                                      .SelectMany(x => x.Items)
                                      .Distinct();
                sb.AppendLine($"import {{ {string.Join(", ", allItems)} }} from '{import.From}';");
            }
            
            sb.AppendLine();
            
            // Component decorator
            sb.AppendLine("@Component({");
            foreach (var line in _componentDecorator)
            {
                sb.AppendLine($"  {line},");
            }
            sb.AppendLine("})");
            
            sb.AppendLine($"export class {_definition.EntityName}Component {{");
            sb.AppendLine();
            
            // Properties
            foreach (var prop in _properties)
            {
                sb.AppendLine($"  {prop.Build()}");
            }
            
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
        
        public TypeScriptBuilder Reset()
        {
            _imports.Clear();
            _properties.Clear();
            _methods.Clear();
            _componentDecorator.Clear();
            InitializeDefaults();
            return this;
        }
    }
}
