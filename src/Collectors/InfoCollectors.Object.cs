using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SoqlGen.Models;
using SoqlGen.Sources;

namespace SoqlGen.Collectors;

internal static partial class InfoCollectors
{
    public static IncrementalValueProvider<ImmutableArray<ObjectInfo>> CreateObjectCollector(this SyntaxValueProvider provider)
    {
        return provider
            .ForAttributeWithMetadataName(
                $"{nameof(SoqlGen)}.{nameof(Texts.SoqlObjectAttribute)}",
                predicate: (_, _) => true,
                transform: (c, _) => ExtractObjectInfos(c))
            .SelectMany((infos, _) => infos)
            .Collect();
    }

    private static ImmutableArray<ObjectInfo> ExtractObjectInfos(GeneratorAttributeSyntaxContext context)
    {
        var objectAttributes = context.Attributes
            .Where(a => a.AttributeClass?.Name == nameof(Texts.SoqlObjectAttribute))
            .ToArray();

        var multiple = objectAttributes.Length > 1;
        return objectAttributes
            .Select(a => ExtractObjectInfo(context, a, multiple))
            .Where(info => info is not null)
            .Cast<ObjectInfo>()
            .ToImmutableArray();
    }

    private static ObjectInfo? ExtractObjectInfo(GeneratorAttributeSyntaxContext context, AttributeData attribute, bool keyRequired)
    {
        var @object = attribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
        var key = attribute.ConstructorArguments.Skip(1).FirstOrDefault().Value?.ToString();
        var targetSymbol = context.TargetSymbol;

        if (@object is null || key is null || targetSymbol is not INamedTypeSymbol modelSymbol)
        {
            return null;
        }

        return ObjectInfo.FromSymbol(@object, key, keyRequired, modelSymbol);
    }
}
