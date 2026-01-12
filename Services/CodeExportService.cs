// using System.IO.Compression;
// using System.Text;
// using AngularGenerator.Core.Models;

// namespace AngularGenerator.Services
// {
//     /// <summary>
//     /// Service for exporting generated code as downloadable files
//     /// </summary>
//     public class CodeExportService
//     {
//         /// <summary>
//         /// Create a ZIP file containing all generated code
//         /// </summary>
//         public byte[] CreateZipFile(ComponentDefinition definition)
//         {
//             using var memoryStream = new MemoryStream();
//             using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
//             {
//                 // Add TypeScript Component file
//                 if (!string.IsNullOrEmpty(definition.GeneratedTs))
//                 {
//                     var tsFileName = $"{definition.EntityName.ToLower()}.component.ts";
//                     var tsEntry = archive.CreateEntry(tsFileName);
//                     using (var entryStream = tsEntry.Open())
//                     using (var writer = new StreamWriter(entryStream))
//                     {
//                         writer.Write(definition.GeneratedTs);
//                     }
//                 }
                
//                 // Add Service file
//                 if (!string.IsNullOrEmpty(definition.GeneratedService))
//                 {
//                     var serviceFileName = $"{definition.EntityName.ToLower()}.service.ts";
//                     var serviceEntry = archive.CreateEntry(serviceFileName);
//                     using (var entryStream = serviceEntry.Open())
//                     using (var writer = new StreamWriter(entryStream))
//                     {
//                         writer.Write(definition.GeneratedService);
//                     }
//                 }
                
//                 // Add HTML file
//                 if (!string.IsNullOrEmpty(definition.GeneratedHtml))
//                 {
//                     var htmlFileName = $"{definition.EntityName.ToLower()}.component.html";
//                     var htmlEntry = archive.CreateEntry(htmlFileName);
//                     using (var entryStream = htmlEntry.Open())
//                     using (var writer = new StreamWriter(entryStream))
//                     {
//                         writer.Write(definition.GeneratedHtml);
//                     }
//                 }
                
//                 // Add README with instructions
//                 var readmeEntry = archive.CreateEntry("README.md");
//                 using (var entryStream = readmeEntry.Open())
//                 using (var writer = new StreamWriter(entryStream))
//                 {
//                     writer.Write(GenerateReadme(definition));
//                 }
//             }
            
//             return memoryStream.ToArray();
//         }
        
//         /// <summary>
//         /// Generate README content with usage instructions
//         /// </summary>
//         private string GenerateReadme(ComponentDefinition definition)
//         {
//             var sb = new StringBuilder();
//             sb.AppendLine($"# {definition.EntityName} Component");
//             sb.AppendLine();
//             sb.AppendLine("## Files Generated");
//             sb.AppendLine($"- `{definition.EntityName.ToLower()}.component.ts` - TypeScript Component");
//             sb.AppendLine($"- `{definition.EntityName.ToLower()}.service.ts` - Angular Service");
//             sb.AppendLine($"- `{definition.EntityName.ToLower()}.component.html` - HTML Template");
//             sb.AppendLine();
//             sb.AppendLine("## How to Use");
//             sb.AppendLine();
//             sb.AppendLine("1. **Copy files to your Angular project:**");
//             sb.AppendLine($"   ```bash");
//             sb.AppendLine($"   # Create component directory");
//             sb.AppendLine($"   mkdir src/app/components/{definition.EntityName.ToLower()}");
//             sb.AppendLine();
//             sb.AppendLine($"   # Copy all files");
//             sb.AppendLine($"   cp *.ts *.html src/app/components/{definition.EntityName.ToLower()}/");
//             sb.AppendLine("   ```");
//             sb.AppendLine();
//             sb.AppendLine("2. **Update your API base URL:**");
//             sb.AppendLine($"   - Open `{definition.EntityName.ToLower()}.service.ts`");
//             sb.AppendLine("   - Change `private baseUrl = '/api';` to your API endpoint");
//             sb.AppendLine();
//             sb.AppendLine("3. **Import the component:**");
//             sb.AppendLine("   ```typescript");
//             sb.AppendLine($"   import {{ {definition.EntityName}Component }} from './components/{definition.EntityName.ToLower()}/{definition.EntityName.ToLower()}.component';");
//             sb.AppendLine("   ```");
//             sb.AppendLine();
//             sb.AppendLine("4. **Add to your routes (if needed):**");
//             sb.AppendLine("   ```typescript");
//             sb.AppendLine("   {");
//             sb.AppendLine($"     path: '{definition.EntityName.ToLower()}',");
//             sb.AppendLine($"     component: {definition.EntityName}Component");
//             sb.AppendLine("   }");
//             sb.AppendLine("   ```");
//             sb.AppendLine();
//             sb.AppendLine("## Features");
//             sb.AppendLine();
//             if (definition.IsGet)
//                 sb.AppendLine("- ✅ List all records");
//             if (definition.IsGetById)
//                 sb.AppendLine("- ✅ Get record by ID");
//             if (definition.IsPost)
//                 sb.AppendLine("- ✅ Create new record");
//             if (definition.IsUpdate)
//                 sb.AppendLine("- ✅ Update existing record");
//             if (definition.IsDelete)
//                 sb.AppendLine("- ✅ Delete record");
//             sb.AppendLine();
//             sb.AppendLine("## Technologies");
//             sb.AppendLine();
//             sb.AppendLine("- Angular 16+ (Standalone Components)");
//             sb.AppendLine("- Reactive Forms");
//             sb.AppendLine("- Signals");
//             sb.AppendLine("- Bootstrap 5 (for styling)");
//             sb.AppendLine();
//             sb.AppendLine("---");
//             sb.AppendLine();
//             sb.AppendLine("*Generated by Angular CRUD Generator - Builder Pattern Edition*");
            
//             return sb.ToString();
//         }
//     }
// }
