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
            AddImport(new[] { "Component", "inject", "signal", "computed", "OnInit" }, "@angular/core");
            AddImport(new[] { "CommonModule" }, "@angular/common");
            AddImport(new[] { "FormsModule" }, "@angular/forms");
            
            // Import interface if separated
            if (_definition.SeparateInterface)
            {
                AddImport(new[] { $"{_definition.EntityName}Model" }, $"./{_definition.EntityName.ToLower()}.interface");
            }
            
            // Default component decorator
            _componentDecorator.Add($"selector: '{_definition.Selector}'");
            _componentDecorator.Add("standalone: true");
            _componentDecorator.Add("imports: [CommonModule, FormsModule]");
            _componentDecorator.Add($"templateUrl: './{_definition.EntityName.ToLower()}.html'");
            _componentDecorator.Add($"styleUrls: ['./{_definition.EntityName.ToLower()}.css']");            
            // Add Material imports if using Angular Material
            if (_definition.CssFramework == CSSFramework.AngularMaterial)
            {
                AddImport(new[] { "MatTableModule" }, "@angular/material/table");
                AddImport(new[] { "MatButtonModule" }, "@angular/material/button");
                AddImport(new[] { "MatIconModule" }, "@angular/material/icon");
                AddImport(new[] { "MatFormFieldModule" }, "@angular/material/form-field");
                AddImport(new[] { "MatInputModule" }, "@angular/material/input");
                AddImport(new[] { "MatChipsModule" }, "@angular/material/chips");
                AddImport(new[] { "MatTooltipModule" }, "@angular/material/tooltip");
                AddImport(new[] { "MatSortModule" }, "@angular/material/sort");
                AddImport(new[] { "MatCardModule" }, "@angular/material/card");
                
                var importsLine = _componentDecorator.FirstOrDefault(x => x.StartsWith("imports:"));
                if (importsLine != null)
                {
                    _componentDecorator.Remove(importsLine);
                    _componentDecorator.Add("imports: [CommonModule, FormsModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatChipsModule, MatTooltipModule, MatSortModule, MatCardModule]");
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
                    _componentDecorator.Add("imports: [CommonModule, ReactiveFormsModule, FormsModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule]");
                }
                else
                {
                    _componentDecorator.Add("imports: [CommonModule, ReactiveFormsModule, FormsModule]");
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
            
            return this;
        }
        
        public TypeScriptBuilder WithService()
        {   
            if (!_definition.SeparateInterface)
            {
                AddImport(new[] { $"{_definition.EntityName}Service", $"{_definition.EntityName}Model" }, 
                    $"./{_definition.EntityName.ToLower()}.service");
            }
            else
            {
                AddImport(new[] { $"{_definition.EntityName}Service" }, 
                    $"./{_definition.EntityName.ToLower()}.service");
            }
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
            // Data signals
            AddProperty(new PropertySegment 
            { 
                Name = "dataList", 
                Type = $"{_definition.EntityName}Model[]",
                InitialValue = $"signal<{_definition.EntityName}Model[]>([])",
                IsSignal = false,
                AccessModifier = "public"
            });
            
            // Add displayedColumns for Material Table
            if (_definition.CssFramework == CSSFramework.AngularMaterial)
            {
                var pkField = _definition.Fields.FirstOrDefault(f => f.IsPrimaryKey)?.FieldName ?? "id";
                var hasCheckbox = _definition.Fields.Any(f => f.UIControl == ControlType.Checkbox);
                var columns = new List<string> { $"'{pkField}'" };
                
                // Add other fields (exclude PK and checkbox)
                columns.AddRange(_definition.Fields
                    .Where(f => !f.IsPrimaryKey && f.UIControl != ControlType.Checkbox)
                    .Select(f => $"'{f.FieldName}'"));
                
                if (hasCheckbox)
                {
                    columns.Add("'status'");
                }
                
                if (_definition.IsUpdate || _definition.IsDelete || _definition.IsGetById)
                {
                    columns.Add("'actions'");
                }
                
                var columnsArray = string.Join(", ", columns);
                
                AddProperty(new PropertySegment
                {
                    Name = "displayedColumns",
                    Type = "string[]",
                    InitialValue = $"[{columnsArray}]",
                    AccessModifier = "public"
                });
                
                // Add cardFields for Material Card view
                var cardFieldsList = _definition.Fields
                    .Where(f => !f.IsPrimaryKey && f.UIControl != ControlType.Checkbox)
                    .Select(f => $"{{ key: '{f.FieldName}', label: '{f.Label}', type: '{GetFieldType(f)}' }}")
                    .ToList();
                
                if (cardFieldsList.Any())
                {
                    var cardFieldsValue = $"[{string.Join(", ", cardFieldsList)}]";
                    
                    AddProperty(new PropertySegment
                    {
                        Name = "cardFields",
                        Type = "Array<{key: string, label: string, type: string}>",
                        InitialValue = cardFieldsValue,
                        AccessModifier = "public"
                    });
                }
            }
            
            AddProperty(new PropertySegment 
            { 
                Name = "isLoading", 
                Type = "boolean",
                InitialValue = "signal<boolean>(false)",
                IsSignal = false,
                AccessModifier = "public"
            });
            
            // Search & Sort signals
            AddProperty(new PropertySegment
            {
                Name = "searchTerm",
                Type = "string",
                InitialValue = "signal<string>('')",
                IsSignal = false,
                AccessModifier = "public"
            });
            
            AddProperty(new PropertySegment
            {
                Name = "sortColumn",
                Type = "string",
                InitialValue = "signal<string>('')",
                IsSignal = false,
                AccessModifier = "public"
            });
            
            AddProperty(new PropertySegment
            {
                Name = "sortDirection",
                Type = "'asc' | 'desc'",
                InitialValue = "signal<'asc' | 'desc'>('asc')",
                IsSignal = false,
                AccessModifier = "public"
            });
            
            // Computed filtered list
            var filterLogic = new List<string>
            {
                "let data = [...this.dataList()];",
                "const term = this.searchTerm().toLowerCase();",
                "const col = this.sortColumn();",
                "const dir = this.sortDirection();",
                "",
                "if (term) {",
                $"  const firstFieldKey = this.formFields[0].key as keyof {_definition.EntityName}Model;",
                "  data = data.filter(item => {",
                "      const val = item[firstFieldKey];",
                "      return val ? String(val).toLowerCase().includes(term) : false;",
                "    });",
                "}",
                "",
                "// 2. Sort",
                "if (col) {",
                "  data.sort((a, b) => {",
                "    const valA = (a as any)[col];",
                "    const valB = (b as any)[col];",
                "    if (valA == null) return 1;",
                "    if (valB == null) return -1;",
                "    if (valA < valB) return dir === 'asc' ? -1 : 1;",
                "    if (valA > valB) return dir === 'asc' ? 1 : -1;",
                "    return 0;",
                "  });",
                "}",
                "return data;"
            };
            
            AddProperty(new PropertySegment
            {
                Name = "filteredList",
                Type = "",
                InitialValue = "computed(() => {\n    " + string.Join("\n    ", filterLogic) + "\n  })",
                AccessModifier = "public"
            });
            
            // Load data method
            var loadMethod = new MethodSegment
            {
                Name = "loadData",
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    "this.isLoading.set(true);",
                    "this.service.getAll().subscribe({",
                    "  next: (res) => { this.dataList.set(res); this.isLoading.set(false); },",
                    "  error: (err) => { console.error(err); this.isLoading.set(false); }",
                    "});"
                },
                AccessModifier = "public"
            };
            
            AddMethod(loadMethod);
            
            // Sort method
            AddMethod(new MethodSegment
            {
                Name = "onSort",
                Parameters = new List<string> { "columnKey: string" },
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    "if (this.sortColumn() === columnKey) {",
                    "  this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');",
                    "} else {",
                    "  this.sortColumn.set(columnKey);",
                    "  this.sortDirection.set('asc');",
                    "}"
                },
                AccessModifier = "public"
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
                IsSignal = false,
                AccessModifier = "public"
            });
            
            AddProperty(new PropertySegment 
            { 
                Name = "isEditMode", 
                Type = "boolean",
                InitialValue = "signal<boolean>(false)",
                IsSignal = false,
                AccessModifier = "public"
            });
            
            AddProperty(new PropertySegment 
            { 
                Name = "isViewMode", 
                Type = "boolean",
                InitialValue = "signal<boolean>(false)",
                IsSignal = false,
                AccessModifier = "public"
            });
            
            var openCreateMethod = new MethodSegment
            {
                Name = "openCreate",
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    "this.isViewMode.set(false);",
                    "this.isEditMode.set(false);",
                    $"this.{_definition.EntityName.ToLower()}Form.enable();",
                    "",
                    "// Reset Form",
                    $"const resetValues: any = {{ {_definition.PrimaryKeyName}: 0 }};",
                    "this.formFields.forEach(f => resetValues[f.key] = f.type === 'checkbox' ? false : '');",
                    $"this.{_definition.EntityName.ToLower()}Form.reset(resetValues);",
                    "",
                    "this.showModal.set(true);"
                },
                AccessModifier = "public"
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
                    IsSignal = false,
                    AccessModifier = "public"
                });
            }
            
            if (!_properties.Any(p => p.Name == "isEditMode"))
            {
                AddProperty(new PropertySegment 
                { 
                    Name = "isEditMode", 
                    Type = "boolean",
                    InitialValue = "signal<boolean>(false)",
                    IsSignal = false,
                    AccessModifier = "public"
                });
            }
            
            if (!_properties.Any(p => p.Name == "isViewMode"))
            {
                AddProperty(new PropertySegment 
                { 
                    Name = "isViewMode", 
                    Type = "boolean",
                    InitialValue = "signal<boolean>(false)",
                    IsSignal = false,
                    AccessModifier = "public"
                });
            }
            
            // openEdit method - รับ item มาเลย
            AddMethod(new MethodSegment
            {
                Name = "openEdit",
                Parameters = new List<string> { $"item: {_definition.EntityName}Model" },
                ReturnType = "void",
                AccessModifier = "public",
                BodyLines = new List<string>
                {
                    "this.isLoading.set(true);",
                    $"this.service.getById(item.{_definition.PrimaryKeyName}).subscribe({{", 
                    "  next: (data) => {",
                    "    this.isViewMode.set(false);",
                    "    this.isEditMode.set(true);",
                    $"    this.{_definition.EntityName.ToLower()}Form.enable();",
                    $"    this.{_definition.EntityName.ToLower()}Form.patchValue(data);",
                    "    this.showModal.set(true);",
                    "    this.isLoading.set(false);",
                    "  },",
                    "  error: (err) => {",
                    "    alert(err.message);",
                    "    this.isLoading.set(false);",
                    "  }",
                    "});"
                }
            });
            
            // openDetail method - รับ item มาเลย สำหรับ Card View
            AddMethod(new MethodSegment
            {
                 Name = "openDetail",
                Parameters = new List<string> { $"item: {_definition.EntityName}Model" },
                ReturnType = "void",
                AccessModifier = "public",
                BodyLines = new List<string>
                {
                    "this.isLoading.set(true);",
                    $"this.service.getById(item.{_definition.PrimaryKeyName}).subscribe({{", 
                    "  next: (data) => {",
                    "    this.isViewMode.set(true);",
                    "    this.isEditMode.set(false);",
                    $"    this.{_definition.EntityName.ToLower()}Form.disable();",
                    $"    this.{_definition.EntityName.ToLower()}Form.patchValue(data);",
                    "    this.showModal.set(true);",
                    "    this.isLoading.set(false);",
                    "  },",
                    "  error: (err) => {",
                    "    alert(err.message);",
                    "    this.isLoading.set(false);",
                    "  }",
                    "});"
                }
            });
            
            // enableEditMode method - สลับจาก View เป็น Edit Mode
            AddMethod(new MethodSegment
            {
                Name = "enableEditMode",
                Parameters = new List<string>(),
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    "this.isViewMode.set(false);",
                    "this.isEditMode.set(true);",
                    $"this.{_definition.EntityName.ToLower()}Form.enable();"
                },
                AccessModifier = "public"
            });
            
            return this;
        }
        
        public TypeScriptBuilder WithFormSubmit()
        {
            var submitLines = new List<string>
            {
                $"if (this.{_definition.EntityName.ToLower()}Form.invalid) return;",
                $"const formData = this.{_definition.EntityName.ToLower()}Form.getRawValue();",
                "this.isLoading.set(true);",
                ""
            };
            
            if (_definition.IsUpdate && _definition.IsPost)
            {
                submitLines.Add("const action$ = this.isEditMode()");
                submitLines.Add($"  ? this.service.update(formData.{_definition.PrimaryKeyName}, formData)");
                submitLines.Add("  : this.service.create(formData);");
            }
            else if (_definition.IsPost)
            {
                submitLines.Add("const action$ = this.service.create(formData);");
            }
            else if (_definition.IsUpdate)
            {
                submitLines.Add($"const action$ = this.service.update(formData.{_definition.PrimaryKeyName}, formData);");
            }
            
            submitLines.Add("");
            submitLines.Add("action$.subscribe({");
            submitLines.Add("  next: () => { this.loadData(); this.onClose(); },");
            submitLines.Add("  error: (err) => { ");
            submitLines.Add("    alert('Failed: ' + (err.error?.message || err.message)); ");
            submitLines.Add("    this.isLoading.set(false); ");
            submitLines.Add("  }");
            submitLines.Add("});");
            
            AddMethod(new MethodSegment
            {
                Name = "onSubmit",
                ReturnType = "void",
                BodyLines = submitLines,
                AccessModifier = "public"
            });
            
            AddMethod(new MethodSegment
            {
                Name = "onClose",
                ReturnType = "void",
                BodyLines = new List<string> { "this.showModal.set(false);" },
                AccessModifier = "public"
            });
            
            return this;
        }
        
        public TypeScriptBuilder WithDelete()
        {
            var deleteMethod = new MethodSegment
            {
                Name = "onDelete",
                Parameters = new List<string> { $"item: {_definition.EntityName}Model" },
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    $"if (confirm(`ต้องการลบ ID : ${{item.{_definition.PrimaryKeyName}}} ใช่หรือไม่`)) {{",
                    $"  this.service.delete(item.{_definition.PrimaryKeyName}).subscribe({{",
                    "    next: () => this.loadData(),",
                    "    error: (err) => alert('Cannot delete: ' + err.message)",
                    "  });",
                    "}"
                },
                AccessModifier = "public"
            };
            
            AddMethod(deleteMethod);
            return this;
        }
        
        public TypeScriptBuilder WithFormInit()
        {
            // สร้าง formFields array (เสมอ)
            var formFieldsItems = new List<string>();
            foreach (var field in _definition.Fields.Where(f => !f.IsPrimaryKey))
            {
                var maxLength = field.TsType == "string" ? "50" : "null";
                var fieldType = field.UIControl == ControlType.Checkbox ? "checkbox" : 
                               field.UIControl == ControlType.Number ? "number" : 
                               field.UIControl == ControlType.DatePicker ? "date" : "text";
                
                formFieldsItems.Add($"{{ key: '{field.FieldName}', label: '{field.Label}', type: '{fieldType}', required: {field.IsRequired.ToString().ToLower()}, maxLength: {maxLength} }}");
            }
            
            var formFieldsArrayValue = "[\n    " + string.Join(",\n    ", formFieldsItems) + "\n  ]";
            
            AddProperty(new PropertySegment
            {
                Name = "formFields",
                Type = "",
                InitialValue = formFieldsArrayValue,
                AccessModifier = "public readonly"
            });
            
            return this;
        }
        
        public TypeScriptBuilder WithFormGroup()
        {
            // สร้าง FormGroup property
            AddProperty(new PropertySegment
            {
                Name = $"{_definition.EntityName.ToLower()}Form",
                Type = "FormGroup",
                InitialValue = "null!",
                AccessModifier = "public"
            });
            
            var formControls = new List<string>();
            formControls.Add($"const group: any = {{ {_definition.PrimaryKeyName}: [0] }};");
            formControls.Add("this.formFields.forEach(field => {");
            formControls.Add("  const validators = [];");
            formControls.Add("  if (field.required) validators.push(Validators.required);");
            formControls.Add("  if (field.maxLength) validators.push(Validators.maxLength(field.maxLength));");
            formControls.Add("  group[field.key] = [field.type === 'checkbox' ? false : '', validators];");
            formControls.Add("});");
            formControls.Add($"this.{_definition.EntityName.ToLower()}Form = this.fb.group(group);");
            
            var initFormMethod = new MethodSegment
            {
                Name = "initForm",
                ReturnType = "void",
                BodyLines = formControls,
                AccessModifier = "private"
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
                    "if (fieldName in item) {",
                    "  return (item as any)[fieldName];",
                    "}",
                    "",
                    "const pascalCase = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);",
                    "if (pascalCase in item) {",
                    "  return (item as any)[pascalCase];",
                    "}",
                    "",
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
            
            var constructorMethod = new MethodSegment
            {
                Name = "constructor",
                BodyLines = initLines
            };
            
            AddMethod(constructorMethod);
            
            // Add ngOnInit
            AddMethod(new MethodSegment
            {
                Name = "ngOnInit",
                ReturnType = "void",
                BodyLines = new List<string>(),
                AccessModifier = "public"
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
            
            sb.AppendLine($"export class {_definition.EntityName}Component implements OnInit {{");
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
        
        private string GetFieldType(AngularField field)
        {
            return field.UIControl switch
            {
                ControlType.Text => "text",
                ControlType.Number => "number",
                ControlType.DatePicker => "date",
                ControlType.Checkbox => "checkbox",
                ControlType.TextArea => "textarea",
                _ => "text"
            };
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
