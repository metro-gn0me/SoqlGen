using Microsoft.CodeAnalysis;

namespace SoqlGen.Models;

internal record struct LocationInfo(string? FilePath, int Start, int Length)
{
    public static LocationInfo FromLocation(Location? location)
    {
        if (location is null || location == Location.None)
        {
            return new LocationInfo(null, 0, 0);
        }

        var tree = location.SourceTree;
        if (tree is null)
        {
            return new LocationInfo(null, 0, 0);
        }

        var span = location.SourceSpan;
        return new LocationInfo(tree.FilePath, span.Start, span.Length);
    }
}
