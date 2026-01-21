using System.Text;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Builder for generating separate TypeScript Interface file
    /// </summary>
    public class InterfaceBuilder
    {
        private readonly ComponentDefinition _definition;
        
        public InterfaceBuilder(ComponentDefinition definition)
        {
            _definition = definition;
        }
        
        public string Build()
        {
            var sb = new StringBuilder();
            
            // Generate Interface
            sb.AppendLine($"export interface {_definition.EntityName}Model {{");
            
            foreach (var field in _definition.Fields)
            {
                var optional = field.IsRequired ? "" : "?";
                sb.AppendLine($"  {field.FieldName}{optional}: {field.TsType};");
            }
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }
    }
}
