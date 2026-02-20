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
                transform: (c, _) => ExtractFieldInfos(c))
            .SelectMany((infos, _) => infos)
            .Collect();
    }

    private static ImmutableArray<FieldInfo> ExtractFieldInfos(GeneratorAttributeSyntaxContext context)
    {
        var fieldAttributes = context.Attributes
           .Where(a => a.AttributeClass?.Name == nameof(Texts.SoqlFieldAttribute))
           .ToArray();

        return fieldAttributes
            .Select(a => ExtractFieldInfo(context, a))
            .Where(info => info is not null)
            .Cast<FieldInfo>()
            .ToImmutableArray();
    }

    private static FieldInfo? ExtractFieldInfo(GeneratorAttributeSyntaxContext context, AttributeData attribute)
    {
        var fieldName = attribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
        var key = attribute.ConstructorArguments.Skip(1).FirstOrDefault().Value?.ToString();
        var typeHandling = attribute.NamedArguments
            .FirstOrDefault(na => na.Key == "TypeHandling").Value.Value;
            
        var typeHandlingValue = typeHandling is int val ? val : 0;
        var targetSymbol = context.TargetSymbol;

        if (fieldName is null || key is null || targetSymbol is not IPropertySymbol propSymbol)
        {
            return null;
        }

        return FieldInfo.FromSymbol(fieldName, key, typeHandlingValue, propSymbol);
    }
}
