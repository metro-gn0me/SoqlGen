using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SoqlGen.Analyzers;
using SoqlGen.Collectors;
using SoqlGen.Models;
using SoqlGen.Sources;

namespace SoqlGen;

[Generator]
public class SoqlGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx
            .AddSoqlObjectAttribute()
            .AddSoqlFieldAttributeSource()
            .AddSoqlCollectionSource());

        var objects = context.SyntaxProvider.CreateObjectCollector();
        var fields = context.SyntaxProvider.CreateFieldCollector();

        var info = objects.Combine(fields);
        var infoWithCompilation = info.Combine(context.CompilationProvider);
        context.RegisterSourceOutput(infoWithCompilation, GenerateSourceAndDiagnostics);
    }

    private static void GenerateSourceAndDiagnostics(
        SourceProductionContext ctx,
        ((ImmutableArray<ObjectInfo> Left, ImmutableArray<FieldInfo> Right) Combined, Compilation Compilation) input)
    {
        var combined = input.Combined;
        var compilation = input.Compilation;

        var (queryObjects, diagnostics) = combined.ExtractAndValidateInfo(compilation);

        foreach (var diagnostic in diagnostics)
        {
            ctx.ReportDiagnostic(diagnostic);
        }

        foreach (var queryPair in queryObjects)
        {
            var identifier = queryPair.Key;
            var queryObj = queryPair.Value;
            var (source, diagnostic) = Texts.GenerateSoqlCollectionSourceAndDiagnostic(queryObj, queryObjects);
            if (diagnostic is not null)
            {
                ctx.ReportDiagnostic(diagnostic);
            }

            if (source is not null)
            {
                var filename = $"{nameof(Texts.SoqlCollection)}.{identifier.Key}.{identifier.ClassName}.g.cs";
                ctx.AddSource(filename, SourceText.From(source, Encoding.UTF8));
            }
        }
    }
}
