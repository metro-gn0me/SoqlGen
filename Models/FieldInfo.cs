using Microsoft.CodeAnalysis;

namespace SoqlGen.Models;

internal record struct FieldInfo(
    string FieldName,
    string Key,
    string PropertyName,
    string ClassName,
    string TargetSymbolKey)
{
    public static FieldInfo FromSymbol(string fieldName, string key, IPropertySymbol property)
    {
        // Store a stable identifier we can use to re-resolve the property later: "<containingTypeFullName>#<propertyName>"
        var symbolKey = property.ContainingType.ToDisplayString() + "#" + property.Name;
        return new FieldInfo(
                fieldName,
                key,
                property.Name,
                property.ContainingType.ToDisplayString(),
                symbolKey);
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
        => HashCode.Combine(FieldName, Key, PropertyName, ClassName);

    public readonly bool Equals(FieldInfo other)
        => FieldName == other.FieldName
            && Key == other.Key
            && PropertyName == other.PropertyName
            && ClassName == other.ClassName;
}
