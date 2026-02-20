using Microsoft.CodeAnalysis;

namespace SoqlGen.Models;

internal record struct ObjectInfo(
    string ObjectName,
    string Key,
    bool KeyRequired,
    string ClassName,
    string TargetSymbolKey)
{
    // ObjectInfo stores stable strings rather than Roslyn symbols so we can re-resolve symbols on demand
    // against the compilation currently being analyzed. Use ResolveModelSymbol(compilation) to get the live symbol.

    public static ObjectInfo FromSymbol(string objectName, string key, bool keyRequired, INamedTypeSymbol model)
    {
        // Use the model's display string as a stable identifier to re-resolve later
        var symbolKey = model.ToDisplayString();
        return new ObjectInfo(objectName, key, keyRequired, model.ToDisplayString(), symbolKey);
    }

    public readonly INamedTypeSymbol? ResolveModelSymbol(Compilation compilation)
    {
        if (string.IsNullOrEmpty(ClassName))
        {
            return null;
        }

        return compilation.GetTypeByMetadataName(ClassName);
    }

    public readonly bool Equals(ObjectInfo other)
        => ObjectName == other.ObjectName
            && Key == other.Key
            && ClassName == other.ClassName;

    public override readonly int GetHashCode()
        => HashCode.Combine(ObjectName, Key, ClassName);
}
