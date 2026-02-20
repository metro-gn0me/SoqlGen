
using IntegrationTest.Models;
using Xunit;
using SoqlGen;

namespace IntegrationTest.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void Deserialize_ExtraFieldsInJson_AreIgnored()
    {
        // Arrange: JSON has "ExtraField" not in Account model
        var json = """
        {
            "records": [
                {
                    "Name": "Standard Account",
                    "ExtraField": "This should be ignored"
                }
            ]
        }
        """;

        // Act
        var accounts = Account.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(accounts);
        Assert.Equal("Standard Account", accounts[0].Name);
    }

    [Fact]
    public void Deserialize_MissingFieldsInJson_ResultInDefault()
    {
        // Arrange: JSON missing "Name" (required in model, but technically allowed in JSON)
        var json = """
        {
            "records": [
                {
                    "AnnualRevenue": 100
                }
            ]
        }
        """;

        // Act
        var accounts = Account.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(accounts);
        Assert.Equal("", accounts[0].Name); // Should be empty string as it's missing and initialized to ""
        Assert.Equal(100m, accounts[0].AnnualRevenue);
    }
    
    [Fact]
    public void Deserialize_ReservedKeywords_HandlesEscaping()
    {
        // Arrange
        var json = """
        {
            "records": [
                {
                    "namespace": "MyNamespace",
                    "class": "MyClass",
                    "event": "MyEvent"
                }
            ]
        }
        """;

        // Act
        var items = EdgeCaseModel.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(items);
        Assert.Equal("MyNamespace", items[0].@namespace);
        Assert.Equal("MyClass", items[0].@class);
        Assert.Equal("MyEvent", items[0].@event);
    }
}
