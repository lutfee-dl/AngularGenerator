namespace AngularGenerator.Core.Models
{
    public enum DataSourceType
    {
        SqlServer = 0,
        RestApi = 1,
        JsonFile = 2,
        JsonText = 3
    }

    public class JsonSchemaRequest
    {
        public DataSourceType SourceType { get; set; }
        
        // For REST API
        public string? ApiUrl { get; set; }
        public string? HttpMethod { get; set; } = "GET";
        public Dictionary<string, string>? Headers { get; set; }
        
        // For JSON File Upload or JSON Text
        public string? JsonContent { get; set; }
        
        // Entity naming
        public string EntityName { get; set; } = "Item";
    }

    public class ApiHeader
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class JsonSchemaResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<AngularField>? Fields { get; set; }
        public string? PrimaryKeyField { get; set; }
    }

    public class GenerateFromFieldsRequest
    {
        public string EntityName { get; set; } = "Item";
        public List<AngularField> Fields { get; set; } = new();
        public List<string>? SelectedFields { get; set; }
        public string ApiBaseUrl { get; set; } = "http://localhost:3000/api";
        public string GenerationMode { get; set; } = "CRUD";
        public string LayoutType { get; set; } = "TableView";
        public string CssFramework { get; set; } = "BasicCSS";
        public bool SeparateInterface { get; set; }
        public bool IsGet { get; set; } = true;
        public bool IsGetById { get; set; }
        public bool IsPost { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }
    }
}
