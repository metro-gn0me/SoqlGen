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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PartialClassCodeFixProvider)), Shared]
    public class PartialClassCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(DiagnosticDescriptors.ClassMustBePartial.Id);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the class declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
            if (declaration == null)
            {
                return;
            }

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Make class partial",
                    createChangedDocument: c => MakePartialAsync(context.Document, declaration, c),
                    equivalenceKey: "MakePartial"),
                diagnostic);
        }

        private async Task<Document> MakePartialAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken)
        {
            // Create the partial token
            var partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword);

            // Add the 'partial' modifier to the class
            // We need to place it correctly (usually before 'class' and after access modifiers)
            // But SyntaxFactory helpers or just inserting into Modifiers list is cleaner.
            
            // Standard order: public static partial class
            // Roslyn's WithModifiers handles this if we order them, but often just adding to the list works if users put it anywhere?
            // Actually, `partial` must be at the end of modifiers usually? No, `public partial class`.
            // Let's just add it.

            var newModifiers = classDecl.Modifiers.Add(partialToken);
            var newClassDecl = classDecl.WithModifiers(newModifiers);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null) return document; // Should not happen

            var newRoot = root.ReplaceNode(classDecl, newClassDecl);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
