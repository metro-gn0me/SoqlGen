
using System;
using Xunit;
using SoqlGen;
using IntegrationTest.Models;

namespace IntegrationTest.Tests;

public class NegativeTests
{
    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ records: [ { Name: 'Acme' } ] "; // Invalid syntax

        // Act & Assert
        Assert.ThrowsAny<System.Text.Json.JsonException>(() => 
            Account.MyQuery.Deserialize(invalidJson));
    }

    [Fact]
    public void Deserialize_TypeMismatch_ReturnsDefaultValue()
    {
        // Arrange: AnnualRevenue is decimal?, but passed as string
        // SafeGetValue logic catches InvalidOperationException and returns default
        var json = """
        {
            "records": [
                {
                    "Name": "Acme Corp",
                    "AnnualRevenue": "NotANumber"
                }
            ]
        }
        """;

        // Act
        var accounts = Account.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(accounts);
        // Should be null because deserializer fails to parse string as decimal via GetDecimal
        // NOTE: e.GetDecimal() throws InvalidOperationException if token is String.
        // SafeGetValue catches it and returns default(decimal?) => null.
        Assert.Null(accounts[0].AnnualRevenue);
    }

    [Fact]
    public void Deserialize_SingleObjectFallback_ReturnsArrayWithOneItem()
    {
        // Arrange: Response is a single object, not containing "records" array
        // This simulates a query that might return a single record structure or custom response
        // The generator fallback logic wraps this in a list.
        var json = """
        {
            "Name": "Single Account",
            "AnnualRevenue": 500
        }
        """;

        // Act
        var accounts = Account.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(accounts);
        Assert.Equal("Single Account", accounts[0].Name);
        Assert.Equal(500m, accounts[0].AnnualRevenue);
    }

    [Fact]
    public void Deserialize_NullField_WhenRequired_ReturnsDefaultOrNull()
    {
        // Arrange: Name is 'required string' but JSON has null
        var json = """
        {
            "records": [
                {
                    "Name": null
                }
            ]
        }
        """;

        // Act
        var accounts = Account.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(accounts);
        // The generated code uses "SafeGetValue" which returns default! aka null for string.
        // Even though property is required, deserialization bypasses C# nullability checks at runtime unless verified.
        // It should be null.
        Assert.Null(accounts[0].Name);
    }
}
