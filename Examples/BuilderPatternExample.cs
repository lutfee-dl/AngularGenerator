using AngularGenerator.Core.Models;
using AngularGenerator.Services.Builders;

namespace AngularGenerator.Examples
{
    /// <summary>
    /// Example: How to use Builder Pattern for code generation
    /// </summary>
    public class BuilderPatternExample
    {
        public static void Main()
        {
            // Example 1: Full CRUD with all features
            var fullCrudExample = CreateFullCrudExample();
            
            // Example 2: Read-only (Get only)
            var readOnlyExample = CreateReadOnlyExample();
            
            // Example 3: Create & Update only (no delete)
            var createUpdateExample = CreateUpdateExample();
        }
        
        /// <summary>
        /// Example: Full CRUD (Get, Create, Update, Delete)
        /// </summary>
        public static ComponentDefinition CreateFullCrudExample()
        {
            var definition = new ComponentDefinition
            {
                EntityName = "Product",
                Selector = "app-product",
                PrimaryKeyName = "id",
                IsGet = true,
                IsGetById = true,
                IsPost = true,
                IsUpdate = true,
                IsDelete = true,
                Fields = new List<AngularField>
                {
                    new AngularField 
                    { 
                        FieldName = "id", 
                        Label = "ID", 
                        TsType = "number", 
                        IsPrimaryKey = true,
                        UIControl = ControlType.Number
                    },
                    new AngularField 
                    { 
                        FieldName = "name", 
                        Label = "Product Name", 
                        TsType = "string", 
                        IsRequired = true,
                        UIControl = ControlType.Text
                    },
                    new AngularField 
                    { 
                        FieldName = "price", 
                        Label = "Price", 
                        TsType = "number", 
                        IsRequired = true,
                        UIControl = ControlType.Number
                    },
                    new AngularField 
                    { 
                        FieldName = "description", 
                        Label = "Description", 
                        TsType = "string",
                        UIControl = ControlType.TextArea
                    },
                    new AngularField 
                    { 
                        FieldName = "inStock", 
                        Label = "In Stock", 
                        TsType = "boolean",
                        UIControl = ControlType.Checkbox
                    },
                    new AngularField 
                    { 
                        FieldName = "createdDate", 
                        Label = "Created Date", 
                        TsType = "Date",
                        UIControl = ControlType.DatePicker
                    }
                }
            };
            
            // Use Builder Pattern
            var builder = new ComponentBuilder(definition);
            
            string tsCode = builder.BuildTypeScript();
            string serviceCode = builder.BuildService();
            string htmlCode = builder.BuildHtml();
            
            // Output (for demo)
            Console.WriteLine("=== TypeScript Component ===");
            Console.WriteLine(tsCode);
            Console.WriteLine("\n=== Service ===");
            Console.WriteLine(serviceCode);
            Console.WriteLine("\n=== HTML ===");
            Console.WriteLine(htmlCode);
            
            return definition;
        }
        
        /// <summary>
        /// Example: Read-only component (Get only, no forms)
        /// </summary>
        public static ComponentDefinition CreateReadOnlyExample()
        {
            var definition = new ComponentDefinition
            {
                EntityName = "Report",
                Selector = "app-report",
                PrimaryKeyName = "id",
                IsGet = true,    
                IsGetById = false,
                IsPost = false,
                IsUpdate = false,
                IsDelete = false,
                Fields = new List<AngularField>
                {
                    new AngularField { FieldName = "id", Label = "ID", TsType = "number", IsPrimaryKey = true },
                    new AngularField { FieldName = "title", Label = "Title", TsType = "string" },
                    new AngularField { FieldName = "totalSales", Label = "Total Sales", TsType = "number" },
                    new AngularField { FieldName = "reportDate", Label = "Date", TsType = "Date" }
                }
            };
            
            var builder = new ComponentBuilder(definition);
            
            // Generated code will NOT include:
            // - FormBuilder, FormGroup
            // - openCreate(), onEdit(), onDelete()
            // - Modal form
            
            string tsCode = builder.BuildTypeScript();
            string htmlCode = builder.BuildHtml();
            
            Console.WriteLine("=== Read-Only Component ===");
            Console.WriteLine(tsCode);
            
            return definition;
        }
        
        /// <summary>
        /// Example: Create & Update (no delete, no list)
        /// </summary>
        public static ComponentDefinition CreateUpdateExample()
        {
            var definition = new ComponentDefinition
            {
                EntityName = "UserProfile",
                Selector = "app-user-profile",
                PrimaryKeyName = "userId",
                IsGet = false,       // No list
                IsGetById = true,    // Can load for edit
                IsPost = true,       // Can create
                IsUpdate = true,     // Can update
                IsDelete = false,    // No delete
                Fields = new List<AngularField>
                {
                    new AngularField { FieldName = "userId", Label = "User ID", TsType = "number", IsPrimaryKey = true },
                    new AngularField { FieldName = "email", Label = "Email", TsType = "string", IsRequired = true },
                    new AngularField { FieldName = "firstName", Label = "First Name", TsType = "string", IsRequired = true },
                    new AngularField { FieldName = "lastName", Label = "Last Name", TsType = "string", IsRequired = true },
                    new AngularField { FieldName = "bio", Label = "Bio", TsType = "string", UIControl = ControlType.TextArea }
                }
            };
            
            var builder = new ComponentBuilder(definition);
            
            // Generated code will include:
            // - FormBuilder, FormGroup
            // - onSubmit() with create/update logic
            // - NOT include: dataList, loadData(), onDelete()
            
            string tsCode = builder.BuildTypeScript();
            string serviceCode = builder.BuildService();
            
            Console.WriteLine("=== Form-Only Component ===");
            Console.WriteLine(tsCode);
            
            return definition;
        }
        
        /// <summary>
        /// Example: Advanced - Custom builder with extra features
        /// </summary>
        public static void AdvancedBuilderExample()
        {
            var definition = new ComponentDefinition
            {
                EntityName = "Order",
                Selector = "app-order",
                PrimaryKeyName = "orderId",
                IsGet = true,
                IsPost = true,
                Fields = new List<AngularField>
                {
                    new AngularField { FieldName = "orderId", Label = "Order ID", TsType = "number", IsPrimaryKey = true },
                    new AngularField { FieldName = "customerName", Label = "Customer", TsType = "string", IsRequired = true },
                    new AngularField { FieldName = "totalAmount", Label = "Total", TsType = "number", IsRequired = true }
                }
            };
            
            // Use TypeScriptBuilder directly for more control
            var tsBuilder = new TypeScriptBuilder(definition);
            
            tsBuilder
                .WithService()
                .WithGetAll()
                .WithColumnConfig()
                .WithReactiveForms()
                .WithFormInit()
                .WithCreate()
                .WithFormSubmit()
                .WithNgOnInit();
            
            // Add custom method
            tsBuilder.AddMethod(new MethodSegment
            {
                Name = "exportToExcel",
                BodyLines = new List<string>
                {
                    "const data = this.dataList();",
                    "// Export logic here",
                    "console.log('Exporting...', data);"
                }
            });
            
            string tsCode = tsBuilder.Build();
            Console.WriteLine("=== Custom Component ===");
            Console.WriteLine(tsCode);
        }
    }
}
