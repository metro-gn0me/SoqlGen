namespace SoqlGen.Models;

internal partial struct QueryObject(ObjectInfo objectInfo, List<QueryField> fields)
{
    private List<string>? _cachedProjectedFields = null;

    public string ObjectName { get; } = objectInfo.ObjectName;
    public string ClassName { get; } = objectInfo.ClassName;
    public string Key { get; } = objectInfo.Key;
    public List<QueryField> Fields { get; } = fields;
}