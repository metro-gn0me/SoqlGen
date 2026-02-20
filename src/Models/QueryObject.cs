namespace SoqlGen.Models;

internal partial struct QueryObject(ObjectInfo objectInfo, List<QueryField> fields)
{
    private List<string>? _cachedProjectedFields = null;

    public string ObjectName { get; } = objectInfo.ObjectName;
    public string ClassName { get; } = objectInfo.ClassName;
    public string Namespace { get; } = objectInfo.Namespace;
    public string Key { get; } = objectInfo.Key;
    public int TypeHandling { get; } = objectInfo.TypeHandling;
    public List<QueryField> Fields { get; } = fields;
}