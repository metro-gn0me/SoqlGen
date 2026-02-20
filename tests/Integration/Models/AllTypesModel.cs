using SoqlGen;

namespace IntegrationTest.Models;

[SoqlObject("AllTypes", "MyQuery")]
public partial class AllTypesModel
{
    // String
    [SoqlField("MyField_StringField", "MyQuery")]
    public string StringField { get; set; } = "";

    [SoqlField("MyField_NullableStringField", "MyQuery")]
    public string? NullableStringField { get; set; }

    // Int
    [SoqlField("MyField_IntField", "MyQuery")]
    public int IntField { get; set; }

    [SoqlField("MyField_NullableIntField", "MyQuery")]
    public int? NullableIntField { get; set; }

    // Long
    [SoqlField("MyField_LongField", "MyQuery")]
    public long LongField { get; set; }

    [SoqlField("MyField_NullableLongField", "MyQuery")]
    public long? NullableLongField { get; set; }

    // Double
    [SoqlField("MyField_NullableIntExplicit", "MyQuery")]
    public Nullable<int> NullableIntExplicit { get; set; }

    [SoqlField("MyField_NullableGuidExplicit", "MyQuery")]
    public Nullable<Guid> NullableGuidExplicit { get; set; }

    [SoqlField("MyField_DoubleField", "MyQuery")]
    public double DoubleField { get; set; }

    [SoqlField("MyField_NullableDoubleField", "MyQuery")]
    public double? NullableDoubleField { get; set; }

    // Decimal
    [SoqlField("MyField_DecimalField", "MyQuery")]
    public decimal DecimalField { get; set; }

    [SoqlField("MyField_NullableDecimalField", "MyQuery")]
    public decimal? NullableDecimalField { get; set; }

    // Bool
    [SoqlField("MyField_BoolField", "MyQuery")]
    public bool BoolField { get; set; }

    [SoqlField("MyField_NullableBoolField", "MyQuery")]
    public bool? NullableBoolField { get; set; }

    // DateTime
    [SoqlField("MyField_DateTimeField", "MyQuery")]
    public DateTime DateTimeField { get; set; }

    [SoqlField("MyField_NullableDateTimeField", "MyQuery")]
    public DateTime? NullableDateTimeField { get; set; }

    // DateTimeOffset
    [SoqlField("MyField_DateTimeOffsetField", "MyQuery")]
    public DateTimeOffset DateTimeOffsetField { get; set; }

    [SoqlField("MyField_NullableDateTimeOffsetField", "MyQuery")]
    public DateTimeOffset? NullableDateTimeOffsetField { get; set; }

    // Guid
    [SoqlField("MyField_GuidField", "MyQuery")]
    public Guid GuidField { get; set; }
    
    [SoqlField("MyField_NullableGuidField", "MyQuery")]
    public Guid? NullableGuidField { get; set; }

    // Primitive Collections
    [SoqlField("MyField_StringList", "MyQuery")]
    public List<string> StringList { get; set; } = new();

    [SoqlField("MyField_IntArray", "MyQuery")]
    public int[] IntArray { get; set; } = Array.Empty<int>();
    
    [SoqlField("MyField_DateList", "MyQuery")]
    public List<DateTime> DateList { get; set; } = new();

    // Nested SOQL Object
    [SoqlField("MyField_PrimaryContact", "MyQuery")]
    public Contact? PrimaryContact { get; set; }

    // Nested SOQL Object Collections
    [SoqlField("MyField_ContactsList", "MyQuery")]
    public List<Contact> ContactsList { get; set; } = new();

    [SoqlField("MyField_ContactsArray", "MyQuery")]
    public Contact[] ContactsArray { get; set; } = Array.Empty<Contact>();

    // Non-SOQL Object (POCO) - Should be handled by standard JSON deserializer fallback
    [SoqlField("MyField_ArbitraryJsonData", "MyQuery")]
    public CustomJsonData? ArbitraryJsonData { get; set; }

    // List of Non-SOQL Objects
    [SoqlField("MyField_JsonDataList", "MyQuery")]
    public List<CustomJsonData> JsonDataList { get; set; } = new();
}

public class CustomJsonData
{
    public string Key { get; set; } = "";
    public int Value { get; set; }
}
