using Microsoft.CodeAnalysis;

namespace SoqlGen.Models;

internal readonly struct QueryField
{
    public string FieldName { get; }
    public string PropertyName { get; }
    public string TypeName { get; }
    public bool IsCollection { get; }
    public string? CollectionBaseType { get; }

    public QueryField(string fieldName, string propertyName, string typeName, bool isCollection, string? collectionBaseType)
    {
        FieldName = fieldName;
        PropertyName = propertyName;
        TypeName = typeName;
        IsCollection = isCollection;
        CollectionBaseType = collectionBaseType;
    }

    public static QueryField FromResolved(FieldInfo fieldInfo, IPropertySymbol propertySymbol)
    {
        var type = propertySymbol.Type;
        var typeName = type.ToDisplayString();
        var isCollection = type.AllInterfaces.Any(static i => i.ToDisplayString() == "System.Collections.IEnumerable")
            && type.SpecialType != SpecialType.System_String;

        string? collectionBase = null;
        if (isCollection)
        {
            if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
            {
                collectionBase = namedType.TypeArguments[0].ToDisplayString();
            }
            else if (type is IArrayTypeSymbol arrayType)
            {
                collectionBase = arrayType.ElementType.ToDisplayString();
            }
            else
            {
                collectionBase = "object";
            }
        }

        return new QueryField(fieldInfo.FieldName, fieldInfo.PropertyName, typeName, isCollection, collectionBase);
    }

    public static QueryField FromUnresolved(FieldInfo fieldInfo)
        => new(fieldInfo.FieldName, fieldInfo.PropertyName, "object", false, null);
}
