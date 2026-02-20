using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SoqlGen.Diagnostics;
using SoqlGen.Models;

namespace SoqlGen.Analyzers;

internal static class CollectionAnalyzer
{
    public static (QueryDictionary QueryObjects, List<Diagnostic> Diagnostics) ExtractAndValidateInfo(
        this (ImmutableArray<ObjectInfo> Left, ImmutableArray<FieldInfo> Right) combined,
        Compilation compilation)
    {
        var diagnostics = new List<Diagnostic>();

        var objModels = new HashSet<string>();
        var objs = new Dictionary<(string ClassName, string Key), ObjectInfo>();
        var fields = new Dictionary<(string ClassName, string Key), List<FieldInfo>>();

        // Process objects first
        foreach (var o in combined.Left)
        {
            // Re-resolve the model symbol from the current compilation to avoid stale references
            var modelSym = compilation.GetTypeByMetadataName(o.ClassName);
            if (modelSym is null)
            {
                // Skip if type can't be resolved in current compilation
                continue;
            }

            // Check for parameterless constructor
            if (modelSym.Constructors.All(c => c.Parameters.Length > 0))
            {
                diagnostics.Add(DiagnosticPresets.MissingParameterlessConstructor(o, compilation));
                continue;
            }

            var className = o.ClassName;
            objModels.Add(className);

            var key = o.Key;
            if (objs.TryGetValue((className, key), out var _))
            {
                diagnostics.Add(DiagnosticPresets.DuplicateObject(o));
                continue;
            }

            objs[(className, key)] = o;
        }

        // Process fields
        foreach (var f in combined.Right)
        {
            // Re-resolve the property symbol from the current compilation

            if (compilation.GetTypeByMetadataName(f.ClassName)
                ?.GetMembers(f.PropertyName)
                .FirstOrDefault() is not IPropertySymbol prop)
            {
                // Skip if property can't be resolved in current compilation
                continue;
            }

            // Validate property accessibility
            if (prop.SetMethod is null ||
                (prop.SetMethod.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected) &&
                !prop.SetMethod.IsInitOnly)
            {
                diagnostics.Add(DiagnosticPresets.InvalidPropertyAccessor(f, compilation));
                continue;
            }

            var className = f.ClassName;
            if (!objModels.Contains(className))
            {
                diagnostics.Add(DiagnosticPresets.MissingObject(f, compilation));
                continue;
            }

            var key = f.Key;
            if (!objs.TryGetValue((className, key), out var _))
            {
                diagnostics.Add(DiagnosticPresets.InvalidFieldKey(f, compilation));
                continue;
            }

            if (!fields.TryGetValue((className, key), out var list))
            {
                list = [];
                fields[(className, key)] = list;
            }

            // Check for duplicate fields
            if (list.Any(existing => existing.PropertyName == f.PropertyName && existing.FieldName == f.FieldName))
            {
                diagnostics.Add(DiagnosticPresets.DuplicateField(f, compilation));
                continue;
            }

            list.Add(f);
        }

        // Remove objects with no fields and report warnings
        var objectsWithNoFields = objs.Where(o => !fields.ContainsKey(o.Key));
        foreach (var objPair in objectsWithNoFields)
        {
            diagnostics.Add(DiagnosticPresets.ObjectWithNoFields(objPair.Value, compilation));
            objs.Remove(objPair.Key);
        }

        // Build QueryObjects with resolved symbols
        var queryObjects = new QueryDictionary();

        foreach (var objPair in objs)
        {
            var objectInfo = objPair.Value;
            var fieldList = fields.TryGetValue(objPair.Key, out var keyList) ? keyList : [];
            var resolvedQueryFields = new List<QueryField>(fieldList.Count);

            foreach (var f in fieldList)
            {
                // Re-resolve the property symbol to get current type information
                var prop = compilation.GetTypeByMetadataName(f.ClassName)
                    ?.GetMembers(f.PropertyName)
                    .FirstOrDefault() as IPropertySymbol;

                if (prop is not null)
                {
                    resolvedQueryFields.Add(QueryField.FromResolved(f, prop));
                }
                else
                {
                    resolvedQueryFields.Add(QueryField.FromUnresolved(f));
                }
            }

            queryObjects[objPair.Key] = new QueryObject(objectInfo, resolvedQueryFields);
        }

        return (queryObjects, diagnostics);
    }
}
