using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SoqlGen.Diagnostics;
using SoqlGen.Models;

namespace SoqlGen.Analyzers;

internal static class CollectionAnalyzer
{
    public static (
        Dictionary<(string ClassName, string Key), QueryObject> QueryObjects,
        List<Diagnostic> Diagnostics
    ) ExtractAndValidateInfo(this (ImmutableArray<ObjectInfo> Left, ImmutableArray<FieldInfo> Right) combined, Compilation compilation)
    {
        var diagnostics = new List<Diagnostic>();

        var objModels = new HashSet<string>();
        var objs = new Dictionary<(string ClassName, string Key), ObjectInfo>();
        var fields = new Dictionary<(string ClassName, string Key), List<FieldInfo>>();

        foreach (var o in combined.Left)
        {
            var modelSym = o.ResolveModelSymbol(compilation);
            if (modelSym is not null && modelSym.Constructors.All(c => c.Parameters.Length > 0))
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

        foreach (var f in combined.Right)
        {
            var prop = f.ResolvePropertySymbol(compilation);
            if (prop is null || prop.SetMethod is null ||
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

            if (list.Any(existing => existing.PropertyName == f.PropertyName && existing.FieldName == f.FieldName))
            {
                diagnostics.Add(DiagnosticPresets.DuplicateField(f, compilation));
                continue;
            }
            list.Add(f);
        }

        var objectsWithNoFields = objs.Where(o => !fields.ContainsKey(o.Key));
        diagnostics.AddRange(objectsWithNoFields.Select(o => DiagnosticPresets.ObjectWithNoFields(o.Value, compilation)));
        foreach (var o in objectsWithNoFields.Select(o => o.Key).ToList())
        {
            objs.Remove(o);
        }

        var queryObjects = objs.ToDictionary(
            o => (o.Key.ClassName, o.Key.Key),
            o =>
            {
                var fieldList = fields.TryGetValue(o.Key, out var keyList) ? keyList : [];
                var resolvedQueryFields = new List<QueryField>(fieldList.Count);
                foreach (var f in fieldList)
                {
                    var prop = f.ResolvePropertySymbol(compilation);
                    if (prop is not null)
                    {
                        resolvedQueryFields.Add(QueryField.FromResolved(f, prop));
                    }
                    else
                    {
                        resolvedQueryFields.Add(QueryField.FromUnresolved(f));
                    }
                }

                return new QueryObject(o.Value, resolvedQueryFields);
            });

        return (queryObjects, diagnostics);
    }
}
