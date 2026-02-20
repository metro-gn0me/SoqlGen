using Microsoft.CodeAnalysis;
using SoqlGen.Models;

namespace SoqlGen.Diagnostics;

internal static class DiagnosticPresets
{
    public static Diagnostic DuplicateObject(ObjectInfo obj) => Diagnostic.Create(
        DiagnosticDescriptors.DuplicateObject,
        Location.None,
        obj.ClassName,
        obj.Key);

    public static Diagnostic MissingObject(FieldInfo field, Compilation compilation)
    {
        var loc = SymbolHelpers.GetAttributeLocationForField(field, compilation) ?? Location.None;
        return Diagnostic.Create(DiagnosticDescriptors.MissingObject, loc, field.Key, field.ClassName);
    }

    public static Diagnostic InvalidFieldKey(FieldInfo field, Compilation compilation)
    {
        var loc = SymbolHelpers.GetAttributeLocationForField(field, compilation) ?? Location.None;
        return Diagnostic.Create(DiagnosticDescriptors.InvalidFieldKey, loc, field.Key, field.PropertyName, field.ClassName);
    }

    public static Diagnostic DuplicateField(FieldInfo field, Compilation compilation)
    {
        var loc = SymbolHelpers.GetAttributeLocationForField(field, compilation) ?? Location.None;
        return Diagnostic.Create(DiagnosticDescriptors.DuplicateField, loc, field.PropertyName, field.FieldName, field.Key);
    }

    public static Diagnostic ObjectWithNoFields(ObjectInfo obj, Compilation compilation)
    {
        var loc = SymbolHelpers.GetAttributeLocationForObject(obj, compilation) ?? Location.None;
        return Diagnostic.Create(DiagnosticDescriptors.ObjectWithNoFields, loc, obj.ClassName, obj.Key);
    }

    public static Diagnostic CyclicDependency(QueryObject obj) => Diagnostic.Create(
        DiagnosticDescriptors.CyclicDependency,
        Location.None,
        obj.ClassName,
        obj.Key);

    public static Diagnostic MissingParameterlessConstructor(ObjectInfo obj, Compilation compilation)
    {
        var loc = SymbolHelpers.GetAttributeLocationForObject(obj, compilation) ?? Location.None;
        return Diagnostic.Create(DiagnosticDescriptors.MissingParameterlessConstructor, loc, obj.ClassName);
    }

    public static Diagnostic InvalidPropertyAccessor(FieldInfo field, Compilation compilation)
    {
        var loc = SymbolHelpers.GetAttributeLocationForField(field, compilation) ?? Location.None;
        return Diagnostic.Create(DiagnosticDescriptors.InvalidPropertyAccessor, loc, field.PropertyName, field.ClassName);
    }
}
