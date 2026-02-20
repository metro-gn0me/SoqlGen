using Microsoft.CodeAnalysis;

namespace SoqlGen.Models;

internal readonly struct QueryField
{
    public string FieldName { get; }
    public string PropertyName { get; }
    public string TypeName { get; }
    public bool IsCollection { get; }
    public string? CollectionBaseType { get; }
    public bool IsValueType { get; }
    public bool IsNullable { get; }
    public int TypeHandling { get; }

    public QueryField(string fieldName, string propertyName, string typeName, bool isCollection, string? collectionBaseType, bool isValueType, bool isNullable, int typeHandling)
    {
        FieldName = fieldName;
        PropertyName = propertyName;
        TypeName = typeName;
        IsCollection = isCollection;
        CollectionBaseType = collectionBaseType;
        IsValueType = isValueType;
        IsNullable = isNullable;
        TypeHandling = typeHandling;
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
        
        var isValueType = type.IsValueType;
        var isNullable = !isValueType || (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T);

        return new QueryField(fieldInfo.FieldName, fieldInfo.PropertyName, typeName, isCollection, collectionBase, isValueType, isNullable, fieldInfo.TypeHandling);
    }

    public static QueryField FromUnresolved(FieldInfo fieldInfo)
        => new(fieldInfo.FieldName, fieldInfo.PropertyName, "object", false, null, false, true, fieldInfo.TypeHandling);
}
