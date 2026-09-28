using System;
using System.Globalization;
using System.Numerics;
using NPOI.SS.UserModel;

namespace core.filed_type
{
    public class Vector2Type : IFiledType
    {
        public EFiledType TypeEnum => EFiledType.CustomType;

        public bool TryDeduceType(ICell cell)
        {
            return false;
        }

        public object ParseValue(ICell cell)
        {
            if (cell == null || cell.CellType == CellType.Blank)
                return DefaultValue;

            if (cell.CellType == CellType.String)
                return ParseValue(cell.StringCellValue);

            if (cell.CellType == CellType.Formula && cell.CachedFormulaResultType == CellType.String)
                return ParseValue(cell.StringCellValue);

            throw new FormatException($"Cannot parse Vector2 from cell {cell.Address} with cell type {cell.CellType}; expected a string in 'x,y' format.");
        }

        public object ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DefaultValue;

            var components = value.Split(',');
            if (components.Length != 2)
                throw new FormatException($"Invalid Vector2 '{value}'; expected exactly two comma-separated numbers in 'x,y' format.");

            if (!float.TryParse(components[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(components[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                throw new FormatException($"Invalid Vector2 '{value}'; both components must be valid invariant-culture floating-point numbers.");

            return new Vector2(x, y);
        }

        public bool IsInternalType => true;
        public bool IsArray => false;
        public bool IsDictionary => false;

        public bool IsMatch(string typeName, Func<string, IFiledType> matchOther = null)
        {
            return string.Equals(typeName, nameof(Vector2), StringComparison.Ordinal) ||
                   string.Equals(typeName, typeof(Vector2).FullName, StringComparison.Ordinal);
        }

        public string FullTypeName => typeof(Vector2).FullName;
        public object DefaultValue => Vector2.Zero;
        public Type Type => typeof(Vector2);
        public string[] PluginFiles => null;
        public bool NeedInit => false;

        public string GenInitCode(string objName, bool semicolon)
        {
            throw new NotImplementedException();
        }
    }
}
