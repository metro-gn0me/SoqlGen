using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SoqlGen.Models;
using SoqlGen.Sources;

namespace SoqlGen.Collectors;

internal static partial class InfoCollectors
{
    public static IncrementalValueProvider<ImmutableArray<FieldInfo>> CreateFieldCollector(this SyntaxValueProvider provider)
    {
        return provider
            .ForAttributeWithMetadataName(
                $"{nameof(SoqlGen)}.{nameof(Texts.SoqlFieldAttribute)}",
                predicate: (_, _) => true,
                transform: (c, _) => ExtractFieldInfo(c))
            .Where(info => info is not null)
            .Select((info, _) => (FieldInfo)info!)
            .Collect();
    }

    private static FieldInfo? ExtractFieldInfo(GeneratorAttributeSyntaxContext context)
    {
        var attribute = context.Attributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(Texts.SoqlFieldAttribute));
        if (attribute is null)
        {
            return null;
        }

        var fieldName = attribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
        var key = attribute.ConstructorArguments.Skip(1).FirstOrDefault().Value?.ToString();
        var targetSymbol = context.TargetSymbol;

        if (fieldName is null || key is null || targetSymbol is not IPropertySymbol propSymbol)
        {
            return null;
        }

        return FieldInfo.FromSymbol(fieldName, key, propSymbol);
    }
}
