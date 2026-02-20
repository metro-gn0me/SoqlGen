using SoqlGen;

namespace IntegrationTest.Models;

[SoqlObject("Coercion", "MyQuery", TypeHandling = SoqlTypeHandling.Coerce)] // Default to Coerce
public partial class CoercionModel
{
    // Coerced from String to Int
    [SoqlField("MyField_IntFromString", "MyQuery")]
    public int IntFromString { get; set; }

    // Coerced from Number to String
    [SoqlField("MyField_StringFromInt", "MyQuery")]
    public string StringFromInt { get; set; } = null!;

    // Coerced from String to Bool ("true")
    [SoqlField("MyField_BoolFromString", "MyQuery")]
    public bool BoolFromString { get; set; }

    // Coerced from Number to Bool (1)
    [SoqlField("MyField_BoolFromInt", "MyQuery")]
    public bool BoolFromInt { get; set; }

    // Override: Strict Mode for this field (Should be 0/default if mismatched)
    [SoqlField("MyField_StrictInt", "MyQuery", TypeHandling = SoqlTypeHandling.Strict)]
    public int StrictInt { get; set; }
}
