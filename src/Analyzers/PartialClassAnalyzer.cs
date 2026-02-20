using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SoqlGen.Diagnostics;
using SoqlGen.Sources;

namespace SoqlGen.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PartialClassAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(DiagnosticDescriptors.ClassMustBePartial);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDecl)
            {
                return;
            }

            var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
            if (symbol == null)
            {
                return;
            }

            if (IsPartial(classDecl))
            {
                return;
            }

            if (HasSoqlAttributes(symbol))
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ClassMustBePartial, classDecl.Identifier.GetLocation(), symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsPartial(ClassDeclarationSyntax classDecl)
        {
            return classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        }

        private static bool HasSoqlAttributes(INamedTypeSymbol symbol)
        {
            // Check for [SoqlObject] on the class
            if (symbol.GetAttributes().Any(attr => 
                attr.AttributeClass?.Name == nameof(Texts.SoqlObjectAttribute) || 
                attr.AttributeClass?.ToDisplayString() == "SoqlGen." + nameof(Texts.SoqlObjectAttribute)))
            {
                return true;
            }

            // Check for [SoqlField] on any property
            foreach (var member in symbol.GetMembers())
            {
                if (member is IPropertySymbol prop)
                {
                    if (prop.GetAttributes().Any(attr => 
                        attr.AttributeClass?.Name == nameof(Texts.SoqlFieldAttribute) || 
                        attr.AttributeClass?.ToDisplayString() == "SoqlGen." + nameof(Texts.SoqlFieldAttribute)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
