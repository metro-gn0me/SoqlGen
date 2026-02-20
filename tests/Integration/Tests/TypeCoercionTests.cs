using System.Linq;
using IntegrationTest.Models;
using Xunit;

namespace IntegrationTest.Tests;

public class TypeCoercionTests
{
    [Fact]
    public void TestTypeCoercion()
    {
        var json = """
        {
            "records": [
                {
                    "MyField_IntFromString": "123",
                    "MyField_StringFromInt": 456,
                    "MyField_BoolFromString": "true",
                    "MyField_BoolFromInt": 1,
                    "MyField_StrictInt": "999"
                }
            ]
        }
        """;

        var results = CoercionModel.MyQuery.Deserialize(json);

        Assert.Single(results);
        var item = results[0];

        // Verify Coercion
        Assert.Equal(123, item.IntFromString);
        Assert.Equal("456", item.StringFromInt);
        Assert.True(item.BoolFromString);
        Assert.True(item.BoolFromInt);

        // Verify Strict Override (Should fail to parse "999" as int in strict mode, resulting in default 0)
        Assert.Equal(0, item.StrictInt);
    }
}
