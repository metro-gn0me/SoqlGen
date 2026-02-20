using Microsoft.CodeAnalysis;

namespace SoqlGen.Models;

internal record struct FieldInfo(
    string FieldName,
    string Key,
    string PropertyName,
    string ClassName,
    string TargetSymbolKey,
    int TypeHandling)
{
    // FieldInfo stores only simple, stable primitives (names/keys) and avoids storing Roslyn symbols or Locations
    // which become invalid across generator/compilation runs. Use ResolvePropertySymbol(compilation) to re-resolve
    // the live symbol and SymbolHelpers to locate the attribute syntax for diagnostics when needed.

    public static FieldInfo FromSymbol(string fieldName, string key, int typeHandling, IPropertySymbol property)
    {
        // Store a stable identifier we can use to re-resolve the property later: "<containingTypeFullName>#<propertyName>"
        var symbolKey = property.ContainingType.ToDisplayString() + "#" + property.Name;
        return new FieldInfo(
                fieldName,
                key,
                property.Name,
                property.ContainingType.ToDisplayString(),
                symbolKey,
                typeHandling);
    }

    public readonly IPropertySymbol? ResolvePropertySymbol(Compilation compilation)
    {
        if (string.IsNullOrEmpty(ClassName) || string.IsNullOrEmpty(PropertyName))
        {
            return null;
        }

        var typeSymbol = compilation.GetTypeByMetadataName(ClassName);
        if (typeSymbol is null)
        {
            return null;
        }

        return typeSymbol.GetMembers(PropertyName).FirstOrDefault() as IPropertySymbol;
    }

    public override readonly int GetHashCode()
        => HashCode.Combine(FieldName, Key, PropertyName, ClassName, TypeHandling);

    public readonly bool Equals(FieldInfo other)
        => FieldName == other.FieldName
            && Key == other.Key
            && PropertyName == other.PropertyName
            && ClassName == other.ClassName
            && TypeHandling == other.TypeHandling;
}
