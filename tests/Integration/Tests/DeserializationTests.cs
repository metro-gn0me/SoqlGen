
using System;
using Xunit;
using SoqlGen;
using IntegrationTest.Models;

namespace IntegrationTest.Tests;

public class DeserializationTests
{
    public DeserializationTests()
    {
        // Ensure consistent culture for decimal parsing if relevant, though JSON is invariant usually
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    }

    [Fact]
    public void Deserialize_ComplexNestedStructure_ReturnsCorrectData()
    {
        // Arrange
        var json = """
        {
            "records": [
                {
                    "Name": "Acme Corp",
                    "AnnualRevenue": 1000000.50,
                    "Contacts": {
                        "records": [
                            { "LastName": "Smith", "CreatedDate": "2024-01-01T12:00:00Z" },
                            { "LastName": "Jones", "CreatedDate": "2024-02-01T12:00:00Z" }
                        ]
                    },
                    "Owner": {
                        "Username": "admin@acme.com"
                    }
                },
                {
                    "Name": "Startup Inc",
                    "AnnualRevenue": null,
                    "Contacts": null,
                    "Owner": null
                }
            ]
        }
        """;

        // Act
        var accounts = Account.MyQuery.Deserialize(json);

        // Assert
        Assert.Equal(2, accounts.Length);

        // Assert: Acme Corp
        var acme = accounts[0];
        Assert.Equal("Acme Corp", acme.Name);
        Assert.Equal(1000000.50m, acme.AnnualRevenue);
        Assert.NotNull(acme.Contacts);
        Assert.Equal(2, acme.Contacts.Count);
        Assert.Equal("Smith", acme.Contacts[0].LastName);
        Assert.Equal(new DateTime(2024, 01, 01, 12, 00, 00, DateTimeKind.Utc), acme.Contacts[0].CreatedDate.ToUniversalTime());
        Assert.NotNull(acme.Owner);
        Assert.Equal("admin@acme.com", acme.Owner.Username);

        // Assert: Startup Inc
        var startup = accounts[1];
        Assert.Equal("Startup Inc", startup.Name);
        Assert.Null(startup.AnnualRevenue);
        // Note: Generator behavior for missing/null collection: empty list or null? 
        // Based on Texts.SoqlCollection.cs logic:
        // "return ... ?? System.Array.Empty<elementType>()" for arrays
        // For List<T>, it calls CreateCollectionInstance which handles null inputs by creating empty list if possible.
        // Let's verify behavior. If explicit null in JSON, generator usually passes null to deserializer?
        // Actually, logic is: `nestedDeserializer(record.SafeGetValue(...)`
        // SafeGetValue returns default(T) if null, so null element.
        // CreateCollectionInstance handles null elements array by returning default? 
        // Wait, CreateCollectionInstance(TElement[]? elements) -> "elements ??= Empty".
        // So it should be empty list, not null.
        // But let's assert what we expect. IF the generator creates an empty list for null JSON, tests pass.
        // If it returns null, we fix expectation.
        // Based on code: `CreateCollectionInstance` returns `(TCollection)Activator...` so it returns a NEW instance.
        // So it should NOT be null.
        Assert.NotNull(startup.Contacts); 
        Assert.Empty(startup.Contacts);
        Assert.Null(startup.Owner);
    }

    [Fact]
    public void Deserialize_EmptyResponse_ReturnsEmptyArray()
    {
        // Arrange
        var json = """
        {
            "records": []
        }
        """;

        // Act
        var accounts = Account.MyQuery.Deserialize(json);

        // Assert
        Assert.NotNull(accounts);
        Assert.Empty(accounts);
    }
}
