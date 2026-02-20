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
using Microsoft.CodeAnalysis.Editing;
using SoqlGen.Diagnostics;

namespace SoqlGen.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddSoqlFieldsCodeFixProvider)), Shared]
    public class AddSoqlFieldsCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(DiagnosticDescriptors.ObjectWithNoFields.Id); // SOQL005

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
                    title: "Add [SoqlField] to public properties",
                    createChangedDocument: c => AddFieldsAsync(context.Document, declaration, c),
                    equivalenceKey: "AddSoqlFields"),
                diagnostic);
        }

        private async Task<Document> AddFieldsAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null) return document;

            // 1. Get the Query Key from [SoqlObject]
            var soqlObjectAttr = classDecl.AttributeLists
                .SelectMany(a => a.Attributes)
                .FirstOrDefault(a => a.Name.ToString().Contains("SoqlObject"));

            if (soqlObjectAttr == null || soqlObjectAttr.ArgumentList == null || soqlObjectAttr.ArgumentList.Arguments.Count < 2)
            {
                return document; // Can't determine key
            }

            // Assuming key is the 2nd argument: [SoqlObject("Name", "Key")]
            var keyArg = soqlObjectAttr.ArgumentList.Arguments[1];
            
            // 2. Find eligible properties
            var validProperties = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                .Where(p => 
                    // Public
                    p.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) &&
                    // Read/Write (basic check, analyzer does deeper check)
                    !p.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) &&
                    // Not already decorated
                    !p.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains("SoqlField"))
                );

            var editor = new SyntaxEditor(root, document.Project.Solution.Workspace.Services);

            foreach (var prop in validProperties)
            {
                 // Create [SoqlField("PropName", "Key")]
                 var propName = prop.Identifier.Text;
                 
                 // Reuse key argument syntax but we might need to be careful if it's strictly a string literal vs constant.
                 // Ideally we construct a new LiteralExpression using the Token from the existing argument if possible,
                 // or just use the raw text if it's a string literal.
                 
                 // Let's create a new attribute syntax:
                 var nameArg = SyntaxFactory.AttributeArgument(
                     SyntaxFactory.LiteralExpression(
                         SyntaxKind.StringLiteralExpression,
                         SyntaxFactory.Literal(propName)));

                 // Use the same expression for the key as provided in the Object attribute (could be a const or literal)
                 var keyExpr = keyArg.Expression; 
                 var keyAttributeArg = SyntaxFactory.AttributeArgument(keyExpr);

                 var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("SoqlField"))
                     .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                         SyntaxFactory.SeparatedList(new[] { nameArg, keyAttributeArg })));

                 var newAttributeList = SyntaxFactory.AttributeList(
                     SyntaxFactory.SingletonSeparatedList(attribute));

                 // Add attribute to property using SyntaxEditor
                 editor.AddAttribute(prop, newAttributeList);
            }

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }
    }
}
