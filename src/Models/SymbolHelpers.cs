using Microsoft.CodeAnalysis;

namespace SoqlGen.Models;

internal static class SymbolHelpers
{
    public static IPropertySymbol? TryResolveProperty(string classMetadataName, string propertyName, Compilation compilation)
    {
        if (string.IsNullOrEmpty(classMetadataName) || string.IsNullOrEmpty(propertyName))
        {
            return null;
        }

        var type = compilation.GetTypeByMetadataName(classMetadataName);
        if (type is null)
        {
            return null;
        }

        return type.GetMembers(propertyName).FirstOrDefault() as IPropertySymbol;
    }

    public static Location? GetAttributeLocationForField(FieldInfo field, Compilation compilation)
    {
        // Re-resolve the property symbol from the current compilation to avoid stale syntax references
        var prop = TryResolveProperty(field.ClassName, field.PropertyName, compilation);
        if (prop is null)
        {
            return null;
        }

        // Get fresh syntax references from the current compilation
        foreach (var decl in prop.DeclaringSyntaxReferences)
        {
            // Ensure the syntax tree is part of the current compilation
            var syntaxTree = decl.SyntaxTree;
            if (!compilation.ContainsSyntaxTree(syntaxTree))
            {
                continue;
            }

            var syntax = decl.GetSyntax();
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            var attrLists = syntax.ChildNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>();
            foreach (var list in attrLists)
            {
                foreach (var attr in list.Attributes)
                {
                    // Resolve the attribute type symbol and compare by metadata name to avoid fragile string matching
                    var attrSymbol = semanticModel.GetSymbolInfo(attr).Symbol as IMethodSymbol;
                    var attrType = attrSymbol?.ContainingType;
                    if (attrType is null) continue;

                    if (attrType.Name is "SoqlFieldAttribute" or "SoqlField")
                    {
                        return attr.GetLocation();
                    }
                }
            }
        }

        return null;
    }

    public static Location? GetAttributeLocationForObject(ObjectInfo obj, Compilation compilation)
    {
        if (string.IsNullOrEmpty(obj.ClassName))
        {
            return null;
        }

        // Re-resolve the type from the current compilation
        var type = compilation.GetTypeByMetadataName(obj.ClassName);
        if (type is null)
        {
            return null;
        }

        foreach (var decl in type.DeclaringSyntaxReferences)
        {
            // Ensure the syntax tree is part of the current compilation
            var syntaxTree = decl.SyntaxTree;
            if (!compilation.ContainsSyntaxTree(syntaxTree))
            {
                continue;
            }

            var syntax = decl.GetSyntax();
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            var attrLists = syntax.ChildNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>();
            foreach (var list in attrLists)
            {
                foreach (var attr in list.Attributes)
                {
                    var attrSymbol = semanticModel.GetSymbolInfo(attr).Symbol as IMethodSymbol;
                    var attrType = attrSymbol?.ContainingType;
                    if (attrType is null) continue;

                    if (attrType.Name is "SoqlObjectAttribute" or "SoqlObject")
                    {
                        return attr.GetLocation();
                    }
                }
            }
        }

        return null;
    }
}
