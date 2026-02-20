using Microsoft.CodeAnalysis;

namespace SoqlGen.Diagnostics;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor DuplicateObject = new(
        "SOQL001",
        "Duplicate object definition",
        "Class '{0}' has multiple SoqlObject attributes with the same Key '{1}'",
        "SoqlGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingObject = new(
        "SOQL002",
        "Missing object definition",
        "No SoqlObject defined for key '{0}' in class '{1}'",
        "SoqlGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidFieldKey = new(
        "SOQL003",
        "Invalid field key",
        "Field key '{0}' in property '{1}' doesn't match any SoqlObject key in class '{2}'",
        "SoqlGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateField = new(
        "SOQL004",
        "Duplicate field definition",
        "Property '{0}' has multiple SoqlField attributes with the same FieldName '{1}' and Key '{2}'",
        "SoqlGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ObjectWithNoFields = new(
        "SOQL005",
        "Object with no fields",
        "Class '{0}' with Key '{1}' has no associated SoqlField attributes",
        "SoqlGen",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CyclicDependency = new(
        "SOQL006",
        "Cyclic dependency detected",
        "Cyclic dependency detected in object '{0}' with Key '{1}'",
        "SoqlGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingParameterlessConstructor = new(
        "SOQL007",
        "Missing parameterless constructor",
        "Class '{0}' must have a parameterless constructor to be used as a SoqlObject",
        "SoqlGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidPropertyAccessor = new(
        "SOQL008",
        "Invalid property accessor",
        "Property '{0}' in class '{1}' must have at least an init accessor or a non-private setter to be used as a SoqlField",
        "SoqlGen",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
