using System.Text.Json;
using System.Text.RegularExpressions;
using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    public class JsonSchemaService
    {
        private readonly HttpClient _httpClient;

        public JsonSchemaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JsonSchemaResponse> ParseFromApiAsync(string apiUrl, string httpMethod, Dictionary<string, string>? headers)
        {
            try
            {
                var request = new HttpRequestMessage(new HttpMethod(httpMethod), apiUrl);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                return ParseFromJson(jsonContent);
            }
            catch (Exception ex)
            {
                return new JsonSchemaResponse
                {
                    Success = false,
                    ErrorMessage = $"API Error: {ex.Message}"
                };
            }
        }

        public JsonSchemaResponse ParseFromJson(string jsonContent)
        {
            try
            {
                // ตรวจสอบว่า JSON ว่างหรือไม่
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return new JsonSchemaResponse
                    {
                        Success = false,
                        ErrorMessage = "JSON ว่างเปล่า กรุณาป้อน JSON ที่ถูกต้อง"
                    };
                }

                // ลอง parse JSON
                using var document = JsonDocument.Parse(jsonContent, new JsonDocumentOptions
                {
                    AllowTrailingCommas = true, // อนุญาตให้มี comma ต่อท้าย
                    CommentHandling = JsonCommentHandling.Skip // ข้าม comment
                });
                
                var root = document.RootElement;

                // ตรวจสอบว่าเป็น array หรือ object
                JsonElement sampleObject;
                
                if (root.ValueKind == JsonValueKind.Array)
                {
                    if (root.GetArrayLength() == 0)
                    {
                        return new JsonSchemaResponse
                        {
                            Success = false,
                            ErrorMessage = "JSON array ว่างเปล่า ต้องมีอย่างน้อย 1 object"
                        };
                    }
                    sampleObject = root[0];
                }
                else
                {
                    sampleObject = root;
                }

                if (sampleObject.ValueKind != JsonValueKind.Object)
                {
                    return new JsonSchemaResponse
                    {
                        Success = false,
                        ErrorMessage = "JSON ต้องเป็น object หรือ array of objects เท่านั้น"
                    };
                }

                var fields = new List<AngularField>();
                string? primaryKeyField = null;

                foreach (var property in sampleObject.EnumerateObject())
                {
                    var field = InferFieldFromProperty(property);
                    fields.Add(field);

                    // Detect primary key
                    if (primaryKeyField == null && IsPrimaryKeyName(property.Name))
                    {
                        field.IsPrimaryKey = true;
                        primaryKeyField = property.Name;
                    }
                }

                // ถ้าไม่เจอ PK ให้ใช้ field แรก
                if (primaryKeyField == null && fields.Count > 0)
                {
                    fields[0].IsPrimaryKey = true;
                    primaryKeyField = fields[0].FieldName;
                }

                return new JsonSchemaResponse
                {
                    Success = true,
                    Fields = fields,
                    PrimaryKeyField = primaryKeyField
                };
            }
            catch (JsonException ex)
            {
                // แสดง error message ที่เข้าใจง่าย
                var errorMsg = ex.Message;
                if (errorMsg.Contains("depth to be zero"))
                {
                    errorMsg = "JSON ไม่สมบูรณ์: มี { หรือ [ ที่ยังไม่ได้ปิดด้วย } หรือ ] กรุณาตรวจสอบ JSON ให้ครบถ้วน";
                }
                else if (errorMsg.Contains("invalid"))
                {
                    errorMsg = $"JSON ไม่ถูกต้อง: {errorMsg}";
                }
                
                return new JsonSchemaResponse
                {
                    Success = false,
                    ErrorMessage = errorMsg
                };
            }
            catch (Exception ex)
            {
                return new JsonSchemaResponse
                {
                    Success = false,
                    ErrorMessage = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        private AngularField InferFieldFromProperty(JsonProperty property)
        {
            var field = new AngularField
            {
                FieldName = property.Name,
                Label = FormatLabel(property.Name),
                IsRequired = false,
                IsPrimaryKey = false
            };

            // Infer type from value
            var (tsType, controlType) = InferTypeFromValue(property.Value);
            field.TsType = tsType;
            field.UIControl = controlType;

            return field;
        }

        private (string TsType, ControlType ControlType) InferTypeFromValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Number => value.TryGetInt64(out _)
                    ? ("number", ControlType.Number)
                    : ("number", ControlType.Number),
                    
                JsonValueKind.True or JsonValueKind.False => ("boolean", ControlType.Checkbox),
                
                JsonValueKind.String => InferStringType(value.GetString() ?? ""),
                
                JsonValueKind.Null => ("string", ControlType.Text), // ถ้าเป็น null ให้เป็น string
                
                JsonValueKind.Array => ("any[]", ControlType.Text),
                
                JsonValueKind.Object => ("any", ControlType.TextArea), // nested object ให้เป็น TextArea
                
                _ => ("string", ControlType.Text)
            };
        }

        private (string TsType, ControlType ControlType) InferStringType(string value)
        {
            // Check if it's a date
            if (IsDateString(value))
            {
                return ("Date", ControlType.DatePicker);
            }

            // Check if long text
            if (value.Length > 100)
            {
                return ("string", ControlType.TextArea);
            }

            return ("string", ControlType.Text);
        }

        private bool IsDateString(string value)
        {
            // ISO 8601 date pattern
            var isoDatePattern = @"^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2})?";
            if (Regex.IsMatch(value, isoDatePattern))
            {
                return DateTime.TryParse(value, out _);
            }

            return false;
        }

        private bool IsPrimaryKeyName(string propertyName)
        {
            var lower = propertyName.ToLower();
            return lower == "id" 
                || lower.EndsWith("id") 
                || lower == "key" 
                || lower.EndsWith("key");
        }

        private string FormatLabel(string fieldName)
        {
            // Convert camelCase/PascalCase to Title Case with spaces
            var result = Regex.Replace(fieldName, "([A-Z])", " $1").Trim();
            
            // Convert snake_case to spaces
            result = result.Replace("_", " ");
            
            // Capitalize first letter
            if (result.Length > 0)
            {
                result = char.ToUpper(result[0]) + result.Substring(1);
            }

            return result;
        }
    }
}
