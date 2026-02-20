using Microsoft.CodeAnalysis;
using SoqlGen.Diagnostics;

namespace SoqlGen.Models;

internal partial struct QueryObject
{
    public (string? Query, Diagnostic? Diagnostic) GenerateQuery(
        QueryDictionary queryObjects,
        HashSet<string>? visited = null,
        string? fromObject = null)
    {
        visited ??= [];

        var (projectedFields, diagnostic) = GetProjectedFields(queryObjects, visited);
        if (diagnostic is not null)
        {
            return (null, diagnostic);
        }
        if (projectedFields is null)
        {
            return (null, null);
        }

        return ($"SELECT {string.Join(", ", projectedFields)} FROM {fromObject ?? ObjectName}", null);
    }

    public (List<string>? ProjectedFields, Diagnostic? Diagnostic) GetProjectedFields(
        QueryDictionary queryObjects,
        HashSet<string> visited)
    {
        if (_cachedProjectedFields is not null)
        {
            return (_cachedProjectedFields, null);
        }

        if (visited.Contains(ObjectName))
        {
            return (null, DiagnosticPresets.CyclicDependency(this));
        }

        if (Fields.Count == 0)
        {
            return (null, null);
        }

        visited.Add(ObjectName);
        var projectedFields = new List<string>(Fields.Count);
        foreach (var field in Fields)
        {
            var typeToCheck = field.IsCollection && field.CollectionBaseType is not null
                ? field.CollectionBaseType
                : field.TypeName;
            if (!queryObjects.TryGetValue((typeToCheck.Trim('?'), Key), out var nestedObject))
            {
                projectedFields.Add(field.FieldName);
                continue;
            }

            if (field.IsCollection)
            {
                var (nestedQuery, nestedDiagnostic) = nestedObject.GenerateQuery(queryObjects, visited, field.FieldName);
                if (nestedDiagnostic is not null)
                {
                    return (null, nestedDiagnostic);
                }
                if (nestedQuery is not null)
                {
                    projectedFields.Add($"({nestedQuery})");
                }
                continue;
            }

            var (nestedFields, diagnostic) = nestedObject.GetProjectedFields(queryObjects, visited);
            if (diagnostic is not null)
            {
                return (null, diagnostic);
            }
            if (nestedFields is null)
            {
                projectedFields.Add(field.FieldName);
                continue;
            }

            foreach (var nestedField in nestedFields)
            {
                projectedFields.Add($"{field.FieldName}.{nestedField}");
            }
        }
        visited.Remove(ObjectName);

        _cachedProjectedFields = projectedFields;
        return (projectedFields, null);
    }
}