
using System;
using System.Collections.Generic;
using IntegrationTest.Models;
using SoqlGen;
using Xunit;

namespace IntegrationTest.Tests;

public class ComprehensiveTests
{
    [Fact]
    public void Deserialize_AllBasicTypes_PopulatesCorrectly()
    {
        // Arrange
        var json = """
        {
            "records": [
                {
                    "MyField_StringField": "Test String",
                    "MyField_NullableStringField": "Test String",
                    "MyField_IntField": 42,
                    "MyField_NullableIntField": 42,
                    "MyField_LongField": 1234567890,
                    "MyField_NullableLongField": 1234567890,
                    "MyField_DoubleField": 3.14159,
                    "MyField_NullableDoubleField": 3.14159,
                    "MyField_DecimalField": 100.50,
                    "MyField_NullableDecimalField": 100.50,
                    "MyField_BoolField": true,
                    "MyField_NullableBoolField": true,
                    "MyField_DateTimeField": "2024-04-19T12:00:00Z",
                    "MyField_NullableDateTimeField": "2024-04-19T12:00:00Z",
                    "MyField_DateTimeOffsetField": "2024-04-19T12:00:00+00:00",
                    "MyField_NullableDateTimeOffsetField": "2024-04-19T12:00:00+00:00",
                    "MyField_GuidField": "d9f8c0a5-3e2b-4d1c-9a4f-5e6c7d8e9f0a",
                    "MyField_NullableGuidField": "d9f8c0a5-3e2b-4d1c-9a4f-5e6c7d8e9f0a",
                    "MyField_NullableIntExplicit": 999,
                    "MyField_NullableGuidExplicit": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
                }
            ]
        }
        """;

        // Act
        var result = AllTypesModel.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(result);
        var item = result[0];

        // String
        Assert.Equal("Test String", item.StringField);
        Assert.Equal("Test String", item.NullableStringField);

        // Int
        Assert.Equal(42, item.IntField);
        Assert.Equal(42, item.NullableIntField);
        Assert.Equal(999, item.NullableIntExplicit);

        // Long
        Assert.Equal(1234567890L, item.LongField);
        Assert.Equal(1234567890L, item.NullableLongField);

        // Double
        Assert.Equal(3.14159, item.DoubleField, precision: 5);
        Assert.Equal(3.14159, item.NullableDoubleField!.Value, precision: 5);

        // Decimal
        Assert.Equal(100.50m, item.DecimalField);
        Assert.Equal(100.50m, item.NullableDecimalField);

        // Bool
        Assert.True(item.BoolField);
        Assert.Equal(new DateTime(2024, 04, 19, 12, 0, 0, DateTimeKind.Utc), item.DateTimeField.ToUniversalTime());
        Assert.Equal(new DateTime(2024, 04, 19, 12, 0, 0, DateTimeKind.Utc), item.NullableDateTimeField!.Value.ToUniversalTime());
        Assert.Equal(new DateTimeOffset(2024, 04, 19, 12, 0, 0, TimeSpan.Zero), item.DateTimeOffsetField);
        Assert.Equal(new DateTimeOffset(2024, 04, 19, 12, 0, 0, TimeSpan.Zero), item.NullableDateTimeOffsetField);
        Assert.Equal(Guid.Parse("d9f8c0a5-3e2b-4d1c-9a4f-5e6c7d8e9f0a"), item.GuidField);
        Assert.Equal(Guid.Parse("d9f8c0a5-3e2b-4d1c-9a4f-5e6c7d8e9f0a"), item.NullableGuidField);
        Assert.Equal(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), item.NullableGuidExplicit);
    }

    [Fact]
    public void Deserialize_NullableTypes_NullsAreHandled()
    {
        // Arrange
        var json = """
        {
            "records": [
                {
                    "MyField_NullableStringField": null,
                    "MyField_NullableIntField": null,
                    "MyField_NullableLongField": null,
                    "MyField_NullableDoubleField": null,
                    "MyField_NullableDecimalField": null,
                    "MyField_NullableBoolField": null,
                    "MyField_NullableDateTimeField": null,
                    "MyField_NullableDateTimeOffsetField": null,
                    "MyField_NullableGuidField": null
                }
            ]
        }
        """;

        // Act
        var result = AllTypesModel.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(result);
        var item = result[0];
        Assert.Null(item.NullableStringField);
        Assert.Null(item.NullableIntField);
        Assert.Null(item.NullableLongField);
        Assert.Null(item.NullableDoubleField);
        Assert.Null(item.NullableDecimalField);
        Assert.Null(item.NullableBoolField);
        Assert.Null(item.NullableDateTimeField);
        Assert.Null(item.NullableDateTimeOffsetField);
        Assert.Null(item.NullableGuidField);
    }

    [Fact]
    public void Deserialize_PrimitiveCollections_PopulatesListAndArray()
    {
        // Arrange
        var json = """
        {
            "records": [
                {
                    "MyField_StringList": ["A", "B", "C"],
                    "MyField_IntArray": [1, 2, 3],
                    "MyField_DateList": ["2024-01-01T00:00:00Z"]
                }
            ]
        }
        """;

        // Act
        var result = AllTypesModel.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(result);
        var item = result[0];

        Assert.Equal(3, item.StringList.Count);
        Assert.Equal("A", item.StringList[0]);

        Assert.Equal(3, item.IntArray.Length);
        Assert.Equal(1, item.IntArray[0]);

        Assert.Single(item.DateList);
        Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), item.DateList[0].ToUniversalTime());
    }

    [Fact]
    public void Deserialize_NestedSoqlObjects_And_Collections()
    {
        // Arrange
        var json = """
        {
            "records": [
                {
                    "MyField_PrimaryContact": {
                        "LastName": "Smith"
                    },
                    "MyField_ContactsList": {
                        "records": [
                            { "LastName": "Doe" },
                            { "LastName": "Ray" }
                        ]
                    },
                    "MyField_ContactsArray": {
                        "records": [
                            { "LastName": "Me" }
                        ]
                    }
                }
            ]
        }
        """;

        // Act
        var result = AllTypesModel.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(result);
        var item = result[0];

        Assert.NotNull(item.PrimaryContact);
        Assert.Equal("Smith", item.PrimaryContact.LastName);

        Assert.Equal(2, item.ContactsList.Count);
        Assert.Equal("Doe", item.ContactsList[0].LastName);

        Assert.Single(item.ContactsArray);
        Assert.Equal("Me", item.ContactsArray[0].LastName);
    }

    [Fact]
    public void Deserialize_NonSoqlPoco_FallsBackToJsonSerializer()
    {
        // Arrange
        var json = """
        {
            "records": [
                {
                    "MyField_ArbitraryJsonData": {
                        "Key": "Validation",
                        "Value": 100
                    },
                    "MyField_JsonDataList": [
                        { "Key": "Item1", "Value": 1 },
                        { "Key": "Item2", "Value": 2 }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = AllTypesModel.MyQuery.Deserialize(json);

        // Assert
        Assert.Single(result);
        var item = result[0];

        Assert.NotNull(item.ArbitraryJsonData);
        Assert.Equal("Validation", item.ArbitraryJsonData.Key);
        Assert.Equal(100, item.ArbitraryJsonData.Value);

        Assert.Equal(2, item.JsonDataList.Count);
        Assert.Equal("Item1", item.JsonDataList[0].Key);
    }
}
