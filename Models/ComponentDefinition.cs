namespace AngularGenerator.Core.Models
{
    public enum ControlType { Text, Number, DatePicker, Checkbox, TextArea }

    // UI Layout Types
    public enum UILayoutType
    {
        TableView,  
        CardView
    }

    // CSS Framework Types
    public enum CSSFramework
    {
        Bootstrap = 0,
        BasicCSS = 1,
        AngularMaterial = 2 
    }

    public class AngularField
    {
        public string FieldName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string TsType { get; set; } = "string"; 
        public ControlType UIControl { get; set; }
        public bool IsRequired { get; set; }
        public bool IsPrimaryKey { get; set; }
    }

    public class ComponentDefinition
    {
        public string EntityName { get; set; } = string.Empty;
        public string Selector { get; set; } = string.Empty;
        public string PrimaryKeyName { get; set; } = "id";
        public List<AngularField> Fields { get; set; } = new List<AngularField>();

        public bool IsGet { get; set; }
        public bool IsGetById { get; set; }
        public bool IsPost { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }

        public UILayoutType LayoutType { get; set; } = UILayoutType.TableView;
        public CSSFramework CssFramework { get; set; } = CSSFramework.BasicCSS; 

        public string GeneratedHtml { get; set; } = string.Empty;
        public string GeneratedTs { get; set; } = string.Empty;
        public string GeneratedService { get; set; } = string.Empty;
        public string GeneratedCss { get; set; } = string.Empty;
    }
}