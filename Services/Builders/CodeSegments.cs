using System.Text;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Code segment models for composable code generation
    /// </summary>
    public class CodeSegment
    {
        public string Code { get; set; } = string.Empty;
        public int IndentLevel { get; set; } = 0;
        
        public string GetIndented()
        {
            var indent = new string(' ', IndentLevel * 2);
            return string.Join("\n", Code.Split('\n').Select(line => 
                string.IsNullOrWhiteSpace(line) ? line : indent + line));
        }
    }
    
    public class ImportSegment
    {
        public List<string> Items { get; set; } = new();
        public string From { get; set; } = string.Empty;
        
        public string Build()
        {
            if (!Items.Any()) return string.Empty;
            return $"import {{ {string.Join(", ", Items)} }} from '{From}';";
        }
    }
    
    public class PropertySegment
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? InitialValue { get; set; }
        public string AccessModifier { get; set; } = "public";
        public bool IsSignal { get; set; } = false;
        
        public string Build()
        {
            if (InitialValue != null && InitialValue.Contains("signal<"))
            {
                return $"{AccessModifier} {Name} = {InitialValue};";
            }
            
            var value = InitialValue != null ? $" = {InitialValue}" : "";
            var typeAnnotation = !string.IsNullOrEmpty(Type) ? $": {Type}" : "";
            return $"{AccessModifier} {Name}{typeAnnotation}{value};";
        }
    }
    
    public class MethodSegment
    {
        public string Name { get; set; } = string.Empty;
        public string ReturnType { get; set; } = "void";
        public List<string> Parameters { get; set; } = new();
        public List<string> BodyLines { get; set; } = new();
        public string AccessModifier { get; set; } = "public";
        
        public string Build(int indentLevel = 1)
        {
            var sb = new StringBuilder();
            var indent = new string(' ', indentLevel * 2);
            
            var paramStr = string.Join(", ", Parameters);
            
            // Constructor ไม่มี return type ใน TypeScript
            var returnTypeAnnotation = Name == "constructor" ? "" : $": {ReturnType}";
            sb.AppendLine($"{indent}{AccessModifier} {Name}({paramStr}){returnTypeAnnotation} {{");
            
            foreach (var line in BodyLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    sb.AppendLine();
                else
                    sb.AppendLine($"{indent}  {line}");
            }
            
            sb.AppendLine($"{indent}}}");
            return sb.ToString().TrimEnd('\r', '\n');
        }
    }
}
