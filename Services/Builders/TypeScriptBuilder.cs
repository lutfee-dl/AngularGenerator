using System.Text;
using AngularGenerator.Core.Models;
using AngularGenerator.Services.Builders.Strategies;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder for TypeScript component code using fluent API
    /// Uses Strategy Pattern for framework-specific imports
    /// </summary>
    public class TypeScriptBuilder : ICodeBuilder<TypeScriptBuilder>
    {
        private readonly ComponentDefinition _definition;
        private readonly ICssFrameworkRenderer _renderer;
        private readonly List<ImportSegment> _imports = new();
        private readonly List<PropertySegment> _properties = new();
        private readonly List<MethodSegment> _methods = new();
        private readonly List<string> _componentDecorator = new();
        
        public TypeScriptBuilder(ComponentDefinition definition)
        {
            _definition = definition;
            _renderer = CssRendererFactory.Create(definition.CssFramework);
            InitializeDefaults();
        }
        
        private void InitializeDefaults()
        {
            // Default Angular imports
            if (_renderer.RequiresSpecialTableRendering())
            {
                // Material: needs AfterViewInit + ViewChild for MatPaginator
                AddImport(new[] { "Component", "inject", "signal", "computed", "OnInit", "ViewChild", "AfterViewInit" }, "@angular/core");
            }
            else
            {
                AddImport(new[] { "Component", "inject", "signal", "computed", "OnInit" }, "@angular/core");
            }
            AddImport(new[] { "CommonModule" }, "@angular/common");
            AddImport(new[] { "FormsModule" }, "@angular/forms");
            
            // Import interface if separated
            if (_definition.SeparateInterface)
            {
                AddImport(new[] { $"{_definition.EntityName}Model" }, $"./{_definition.EntityName.ToLower()}.interface");
            }
            
            // Default component decorator
            _componentDecorator.Add($"selector: 'app-{_definition.Selector}'");
            _componentDecorator.Add("standalone: true");
            _componentDecorator.Add($"templateUrl: './{_definition.EntityName.ToLower()}.html'");
            _componentDecorator.Add($"styleUrls: ['./{_definition.EntityName.ToLower()}.css']");            
            
            // Add Material imports if using Angular Material
            var frameworkImports = _renderer.GetRequiredImports();
            if (frameworkImports.Length > 0)
            {
                foreach (var import in frameworkImports)
                {
                    var module = import;
                    var from = $"@angular/material/{module.Replace("Module", "").Replace("Mat", "").ToLower()}";
                    if (module == "MatTableModule") from = "@angular/material/table";
                    else if (module == "MatButtonModule") from = "@angular/material/button";
                    else if (module == "MatIconModule") from = "@angular/material/icon";
                    else if (module == "MatFormFieldModule") from = "@angular/material/form-field";
                    else if (module == "MatInputModule") from = "@angular/material/input";
                    else if (module == "MatChipsModule") from = "@angular/material/chips";
                    else if (module == "MatTooltipModule") from = "@angular/material/tooltip";
                    else if (module == "MatSortModule") from = "@angular/material/sort";
                    else if (module == "MatCardModule") from = "@angular/material/card";
                    else if (module == "MatOptionModule") from = "@angular/material/core";
                    else if (module == "MatProgressSpinnerModule") from = "@angular/material/progress-spinner";
                    else if (module == "MatPaginatorModule")
                    {
                        // MatPaginator class needed for @ViewChild
                        AddImport(new[] { "MatPaginatorModule", "MatPaginator" }, "@angular/material/paginator");
                        continue;
                    }
                    AddImport(new[] { module }, from);
                }
                
                AddImport(new[] { "ReactiveFormsModule" }, "@angular/forms");
                
                _componentDecorator.Add(_renderer.GetImportsDeclaration());
            }
            else
            {
                _componentDecorator.Add("imports: [CommonModule, FormsModule]");
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
                
                // ตรวจสอบว่ามี ReactiveFormsModule อยู่แล้วหรือยัง
                var baseImports = _renderer.GetImportsDeclaration();
                if (!baseImports.Contains("ReactiveFormsModule"))
                {
                    // Replace "FormsModule" ด้วย "ReactiveFormsModule, FormsModule" เฉพาะตอนท้าย
                    baseImports = baseImports.Replace(", FormsModule]", ", ReactiveFormsModule, FormsModule]")
                                             .Replace("[FormsModule]", "[ReactiveFormsModule, FormsModule]")
                                             .Replace("[CommonModule, FormsModule", "[CommonModule, ReactiveFormsModule, FormsModule");
                }
                _componentDecorator.Add(baseImports);
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
            if (_renderer.RequiresSpecialTableRendering())
            {
                // New Dynamic Style for Material
                var hasActions = _definition.IsUpdate || _definition.IsDelete || _definition.IsGetById;
                var columnsValue = hasActions 
                    ? "[...this.formFields.map(f => f.key), 'actions']"
                    : "this.formFields.map(f => f.key)";
                
                AddProperty(new PropertySegment
                {
                    Name = "displayedColumns",
                    Type = "string[]",
                    InitialValue = columnsValue,
                    AccessModifier = "public"
                });
                
                AddProperty(new PropertySegment
                {
                    Name = "cardFields",
                    Type = "",
                    InitialValue = "this.formFields",
                    AccessModifier = "public readonly"
                });
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
                "  data = data.filter(item => ",
                "    Object.values(item).some(val => ",
                "      String(val).toLowerCase().includes(term)",
                "    )",
                "  );",
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
                ""
            };
            
            if (_renderer.RequiresSpecialTableRendering())
            {
                // Material: filteredList returns ALL (mat-paginator handles slicing)
                filterLogic.Add("return data; // mat-paginator handles pagination");
            }
            else
            {
                filterLogic.Add("const startIndex = (this.currentPage() - 1) * this.pageSize();");
                filterLogic.Add("return data.slice(startIndex, startIndex + this.pageSize());");
            }
            
            AddProperty(new PropertySegment
            {
                Name = "filteredList",
                Type = "",
                InitialValue = "computed(() => {\n    " + string.Join("\n    ", filterLogic) + "\n  })",
                AccessModifier = "public"
            });
            
            // Pagination properties
            if (_renderer.RequiresSpecialTableRendering())
            {
                // Material: 0-indexed (mat-paginator uses pageIndex)
                AddProperty(new PropertySegment
                {
                    Name = "currentPage",
                    Type = "number",
                    InitialValue = "signal<number>(0)",
                    IsSignal = false,
                    AccessModifier = "public"
                });
            }
            else
            {
                // Basic/Bootstrap: 1-indexed
                AddProperty(new PropertySegment
                {
                    Name = "currentPage",
                    Type = "number",
                    InitialValue = "signal<number>(1)",
                    IsSignal = false,
                    AccessModifier = "public"
                });
            }
            
            AddProperty(new PropertySegment
            {
                Name = "pageSize",
                Type = "number",
                InitialValue = "signal<number>(20)",
                IsSignal = false,
                AccessModifier = "public"
            });

            if (_renderer.RequiresSpecialTableRendering())
            {
                // Material: paginatedList for mat-table dataSource
                AddProperty(new PropertySegment
                {
                    Name = "paginatedList",
                    Type = "",
                    InitialValue = "computed(() => {\n    const data = this.filteredList();\n    const startIndex = this.currentPage() * this.pageSize();\n    return data.slice(startIndex, startIndex + this.pageSize());\n  })",
                    AccessModifier = "public"
                });
            }

            // Add setPageSize method
            AddMethod(new MethodSegment
            {
                Name = "setPageSize",
                Parameters = new List<string> { "size: number" },
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    "this.pageSize.set(size);",
                    _renderer.RequiresSpecialTableRendering()
                        ? "this.currentPage.set(0);"
                        : "this.currentPage.set(1);"
                },
                AccessModifier = "public"
            });
            
            // totalPages computed
            AddProperty(new PropertySegment
            {
                Name = "totalPages",
                Type = "",
                InitialValue = @"computed(() => {
  const count = this.searchTerm() 
    ? this.dataList().filter(item => Object.values(item).some(v => String(v).toLowerCase().includes(this.searchTerm().toLowerCase()))).length
    : this.dataList().length;
  return Math.ceil(count / this.pageSize());
  })",
                AccessModifier = "public"
            });
            
            // setPage method
            AddMethod(new MethodSegment
            {
                Name = "setPage",
                Parameters = new List<string> { "page: number" },
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    "this.currentPage.set(page);"
                },
                AccessModifier = "public"
            });

            if (_renderer.RequiresSpecialTableRendering())
            {
                // Material: onPageChange for mat-paginator (page) event
                AddMethod(new MethodSegment
                {
                    Name = "onPageChange",
                    Parameters = new List<string> { "event: any" },
                    ReturnType = "void",
                    BodyLines = new List<string>
                    {
                        "this.currentPage.set(event.pageIndex);",
                        "this.pageSize.set(event.pageSize);"
                    },
                    AccessModifier = "public"
                });
            }

            
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

        public TypeScriptBuilder WithExport()
        {
            // ─── Excel Export ─────────────────────────────────────────────────────
            AddMethod(new MethodSegment
            {
                Name = "exportToExcel",
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    "// Requires: npm install xlsx",
                    "import('xlsx').then(XLSX => {",
                    "  const data = this.dataList();",
                    "  const worksheet = XLSX.utils.json_to_sheet(data);",
                    "  const workbook = XLSX.utils.book_new();",
                    $"  XLSX.utils.book_append_sheet(workbook, worksheet, '{_definition.EntityName}');",
                    $"  XLSX.writeFile(workbook, '{_definition.EntityName.ToLower()}_export.xlsx');",
                    "});"
                },
                AccessModifier = "public"
            });

            // ─── PDF Export ───────────────────────────────────────────────────────
            // jspdf@^2.5.2 + jspdf-autotable@^3.8.3
            // async IIFE + sequential await — most reliable with Angular bundler
            AddMethod(new MethodSegment
            {
                Name = "exportToPdf",
                ReturnType = "void",
                BodyLines = new List<string>
                {
                    "// Requires: npm install jspdf@^2.5.2 jspdf-autotable@^3.8.3",
                    "(async () => {",
                    "  try {",
                    "    // Step 1: load jsPDF",
                    "    const jsPDFModule = await import('jspdf');",
                    "    const jsPDF = jsPDFModule.default ?? (jsPDFModule as any).jsPDF;",
                    "",
                    "    // Step 2: load autoTable (v3.x exports default function)",
                    "    const autoTableModule = await import('jspdf-autotable');",
                    "    const autoTable = autoTableModule.default;",
                    "",
                    "    // Step 3: build document (landscape fits more columns)",
                    "    const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' });",
                    "",
                    "    // All records (ignore pagination)",
                    "    const allData = this.dataList();",
                    "    const selectedFields = this.formFields;",
                    "",
                    "    // Build head and body as plain string[][]",
                    "    const head: string[][] = [selectedFields.map(f => f.label)];",
                    "    const body: string[][] = allData.map(item =>",
                    "      selectedFields.map(f => {",
                    "        const v = (item as any)[f.key];",
                    "        return (v === null || v === undefined) ? '-' : String(v);",
                    "      })",
                    "    );",
                    "",
                    "    // autoTable with optimized settings",
                    "    autoTable(doc, {",
                    "      head,",
                    "      body,",
                    "      startY: 20,",
                    "      theme: 'grid',",
                    "",
                    "      // Header styles — shown every page",
                    "      headStyles: {",
                    "        fillColor: [41, 128, 185],",
                    "        textColor: 255,",
                    "        fontStyle: 'bold',",
                    "        fontSize: 5,",
                    "        halign: 'center',",
                    "        cellPadding: 1,",
                    "        minCellWidth: 8",
                    "      },",
                    "",
                    "      // Body styles — tiny font to fit many columns",
                    "      bodyStyles: {",
                    "        fontSize: 4.5,",
                    "        textColor: [30, 30, 30],",
                    "        cellPadding: 1,",
                    "        minCellHeight: 4,",
                    "        minCellWidth: 8",
                    "      },",
                    "",
                    "      alternateRowStyles: { fillColor: [245, 248, 250] },",
                    "",
                    "      styles: {",
                    "        overflow: 'linebreak',",
                    "        cellWidth: 'auto',",
                    "        halign: 'left',",
                    "        valign: 'middle',",
                    "        fontSize: 4.5",
                    "      },",
                    "",
                    "      margin: { top: 20, left: 5, right: 5, bottom: 15 },",
                    "      showHead: 'everyPage',",
                    "      tableWidth: 'auto',",
                    "",
                    "      // Title + date + page number on every page",
                    "      didDrawPage: (data: any) => {",
                    "        const docAny = doc as any;",
                    "",
                    "        // Title",
                    "        docAny.setFontSize(12);",
                    "        docAny.setTextColor(40, 40, 40);",
                    $"        docAny.text('{_definition.EntityName} Export Report', data.settings.margin.left, 12);",
                    "",
                    "        // Date",
                    "        docAny.setFontSize(7);",
                    "        docAny.setTextColor(100, 100, 100);",
                    "        const now = new Date().toLocaleDateString('th-TH');",
                    "        docAny.text('Export: ' + now, data.settings.margin.left, 16);",
                    "",
                    "        // Page number",
                    "        const pageCount = docAny.internal.getNumberOfPages();",
                    "        docAny.setFontSize(7);",
                    "        docAny.setTextColor(128, 128, 128);",
                    "        const pageText = 'หน้า ' + data.pageNumber + '/' + pageCount;",
                    "        const pageWidth = docAny.internal.pageSize.getWidth();",
                    "        docAny.text(pageText, pageWidth - 20, 12);",
                    "      }",
                    "    });",
                    "",
                    $"    doc.save('{_definition.EntityName.ToLower()}_report_' + new Date().getTime() + '.pdf');",
                    "",
                    "  } catch (err: any) {",
                    "    console.error('[exportToPdf] error:', err);",
                    "    if (err?.message?.includes('Cannot find module')) {",
                    "      alert('กรุณาติดตั้ง:\\n npm install jspdf@^2.5.2 jspdf-autotable@^3.8.3');",
                    "    } else {",
                    "      alert('Export PDF ล้มเหลว: ' + (err?.message ?? err));",
                    "    }",
                    "  }",
                    "})();"
                },
                AccessModifier = "public"
            });

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
            var constructorLines = new List<string>();
            
            if (_definition.IsPost || _definition.IsUpdate)
                constructorLines.Add("this.initForm();");
            
            if (_definition.IsGet)
                constructorLines.Add("this.loadData();");
            
            if (constructorLines.Any())
            {
                AddMethod(new MethodSegment
                {
                    Name = "constructor",
                    BodyLines = constructorLines,
                    AccessModifier = "public"
                });
            }
            
            // ngAfterViewInit — Material only: wire paginator
            if (_renderer.RequiresSpecialTableRendering())
            {
                AddMethod(new MethodSegment
                {
                    Name = "ngAfterViewInit",
                    ReturnType = "void",
                    BodyLines = new List<string>
                    {
                        "// เชื่อมต่อ paginator กับ signals",
                        "if (this.paginator) {",
                        "  this.paginator.page.subscribe((event) => {",
                        "    this.onPageChange(event);",
                        "  });",
                        "}"
                    },
                    AccessModifier = "public"
                });
            }
            
            // ngOnInit
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
            
            var classImplements = _renderer.RequiresSpecialTableRendering()
                ? "implements OnInit, AfterViewInit"
                : "implements OnInit";
            sb.AppendLine($"export class {_definition.EntityName}Component {classImplements} {{");
            sb.AppendLine();
            
            // @ViewChild for MatPaginator (Material only)
            if (_renderer.RequiresSpecialTableRendering())
            {
                sb.AppendLine("  @ViewChild(MatPaginator) paginator!: MatPaginator;");
                sb.AppendLine();
            }
            
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
