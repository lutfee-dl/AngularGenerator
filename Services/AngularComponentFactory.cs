using AngularGenerator.Core.Models;

namespace AngularGenerator.Services
{
    public class AngularComponentFactory
    {
        public ComponentDefinition Create(string tableName, IEnumerable<DbColumnInfo> columns)
        {
            var def = new ComponentDefinition
            {
                EntityName = char.ToUpper(tableName[0]) + tableName.Substring(1),
                Selector = $"app-{tableName.ToLower()}"
            };

            foreach (var col in columns)
            {
                var field = new AngularField
                {
                    FieldName = col.ColumnName, // ใช้ PascalCase ตามชื่อคอลัมน์เดิม
                    Label = col.ColumnName,
                    IsRequired = col.IsNullable == "NO",
                    // ใช้ข้อมูล IsPrimaryKey จาก Database จริง (จาก INFORMATION_SCHEMA)
                    IsPrimaryKey = col.IsPrimaryKey
                };

                // Logic การเลือก UI Control ตาม Type (Strategy Logic)
                MapType(col.DataType, field);

                // ตั้งค่า Primary Key Name (เลือก PK แรกที่เจอ)
                if (field.IsPrimaryKey && string.IsNullOrEmpty(def.PrimaryKeyName))
                {
                    def.PrimaryKeyName = field.FieldName;
                }
                
                def.Fields.Add(field);
            }
            
            // Fallback: ถ้าไม่มี Primary Key ใน Database → หาคอลัมน์ที่มี "Key" หรือ "ID" ในชื่อ
            if (string.IsNullOrEmpty(def.PrimaryKeyName) || def.PrimaryKeyName == "id")
            {
                var keyField = def.Fields.FirstOrDefault(f => 
                    f.FieldName.EndsWith("Key", StringComparison.OrdinalIgnoreCase) || 
                    f.FieldName.EndsWith("ID", StringComparison.OrdinalIgnoreCase) ||
                    f.FieldName.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
                    
                if (keyField != null)
                {
                    def.PrimaryKeyName = keyField.FieldName;
                    keyField.IsPrimaryKey = true; // ตั้งเป็น PK ด้วย
                }
            }
            
            return def;
        }

        private void MapType(string sqlType, AngularField field)
        {
            switch (sqlType.ToLower())
            {
                // Numeric types → number
                case "int": case "bigint": case "smallint": case "tinyint":
                case "decimal": case "numeric": case "float": case "real":
                case "money": case "smallmoney":
                    field.TsType = "number";
                    field.UIControl = ControlType.Number;
                    break;
                
                // Boolean types → boolean
                case "bit": case "boolean":
                    field.TsType = "boolean";
                    field.UIControl = ControlType.Checkbox;
                    break;
                
                // Date/Time types → Date
                case "date": case "datetime": case "datetime2": 
                case "smalldatetime": case "datetimeoffset": case "time":
                    field.TsType = "Date";
                    field.UIControl = ControlType.DatePicker;
                    break;
                
                // String types (default) → string
                default:
                    field.TsType = "string";
                    field.UIControl = sqlType.Contains("max") ? ControlType.TextArea : ControlType.Text;
                    break;
            }
        }
    }
}