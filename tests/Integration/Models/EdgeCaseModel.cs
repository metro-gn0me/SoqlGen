
using SoqlGen;

namespace IntegrationTest.Models;

[SoqlObject("EdgeCase", "MyQuery")]
public partial class EdgeCaseModel
{
    // Test reserved keywords
    [SoqlField("namespace", "MyQuery")]
    public string @namespace { get; set; } = "";

    [SoqlField("class", "MyQuery")]
    public string @class { get; set; } = "";

    [SoqlField("event", "MyQuery")]
    public string @event { get; set; } = "";
}
