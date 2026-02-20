using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SoqlGen.Diagnostics;

namespace SoqlGen.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InvalidFieldKeyCodeFixProvider)), Shared]
    public class InvalidFieldKeyCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(DiagnosticDescriptors.InvalidFieldKey.Id); // SOQL003

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // Batch fixer works if there is only one replacement, but we might arguably be okay handling multiple ambiguities? 
            // Usually if there are multiple options, batch fix is tricky. 
            // For now, let's disable BatchFixer or leave it if we are confident the first option is reasonable default?
            // Actually, if we register multiple actions, GetFixAllProvider generally applies the "EquivalenceKey" matched one.
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null) return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            
            // The diagnostic is likely reported on the Attribute, or the specific Argument. 
            // Let's find the AttributeSyntax.
            var node = root.FindToken(diagnosticSpan.Start).Parent;
            var attribute = node?.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
            
            if (attribute == null) return;
            
            // Find containing class
            var classDecl = attribute.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDecl == null) return;

            // Get valid keys from [SoqlObject] attributes on the class
            var validKeys = classDecl.AttributeLists
                .SelectMany(a => a.Attributes)
                .Where(a => a.Name.ToString().Contains("SoqlObject") && a.ArgumentList != null && a.ArgumentList.Arguments.Count >= 2)
                .Select(a => a.ArgumentList!.Arguments[1].Expression) // Get expression of the Key argument
                .ToList();

            if (!validKeys.Any()) return;

            foreach (var keyExpr in validKeys)
            {
                // We need a readable string for the title. 
                // keyExpr is an ExpressionSyntax (e.g., Literal "MyKey" or Constant Reference).
                var keyTitle = keyExpr.ToString();

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: $"Change Key to {keyTitle}",
                        createChangedDocument: c => FixKeyAsync(context.Document, attribute, keyExpr, c),
                        equivalenceKey: $"FixKey_{keyTitle}"), // Unique ID per key choice
                    diagnostic);
            }
        }

        private async Task<Document> FixKeyAsync(Document document, AttributeSyntax attribute, ExpressionSyntax newKeyExpr, CancellationToken cancellationToken)
        {
            // We need to replace the 2nd argument of the [SoqlField] attribute.
            // Assuming positional arguments: [SoqlField("Name", "BadKey")]
            
            if (attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count < 2)
                return document;

            // Clone the argument list
            var oldArg = attribute.ArgumentList.Arguments[1];
            
            // Create new argument with the valid key expression
            var newArg = SyntaxFactory.AttributeArgument(newKeyExpr)
                .WithTriviaFrom(oldArg); // Preserve whitespace/comments

            // Replace in list
            var newArgumentList = attribute.ArgumentList.ReplaceNode(oldArg, newArg);
            var newAttribute = attribute.WithArgumentList(newArgumentList);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null) return document;

            var newRoot = root.ReplaceNode(attribute, newAttribute);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
