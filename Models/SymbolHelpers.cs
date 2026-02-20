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
        // Resolve property symbol by stored class+property identifier
        var prop = TryResolveProperty(field.ClassName, field.PropertyName, compilation);
        if (prop is null)
        {
            return null;
        }

        // Inspect each declaring syntax reference for the property and look for attributes
        foreach (var decl in prop.DeclaringSyntaxReferences)
        {
            var syntax = decl.GetSyntax();
            var attrLists = syntax.ChildNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>();
            foreach (var list in attrLists)
            {
                foreach (var attr in list.Attributes)
                {
                    var name = attr.Name.ToString();
                    if (name.EndsWith("SoqlField") || name.Contains("SoqlField"))
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

        var type = compilation.GetTypeByMetadataName(obj.ClassName);
        if (type is null)
        {
            return null;
        }

        foreach (var decl in type.DeclaringSyntaxReferences)
        {
            var syntax = decl.GetSyntax();
            var attrLists = syntax.ChildNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>();
            foreach (var list in attrLists)
            {
                foreach (var attr in list.Attributes)
                {
                    var name = attr.Name.ToString();
                    if (name.EndsWith("SoqlObject") || name.Contains("SoqlObject"))
                    {
                        return attr.GetLocation();
                    }
                }
            }
        }

        return null;
    }
}
