
using System;
using System.Text.Json;
using Xunit;
using SoqlGen;
using IntegrationTest.Models;

namespace IntegrationTest.Tests;

public class ReadOnlySpanTests
{
    [Fact]
    public void Deserialize_FromBinaryData_UsingSpan_WorksCorrectly()
    {
        // Arrange
        var json = """
        {
            "records": [
                {
                    "MyField_StringField": "BinaryData Test",
                    "MyField_IntField": 123
                }
            ]
        }
        """;
        
        var binaryData = new BinaryData(json);

        // Act
        // This validates the new ReadOnlySpan<byte> overload
        var result = AllTypesModel.MyQuery.Deserialize(binaryData.ToMemory().Span);

        // Assert
        Assert.Single(result);
        Assert.Equal("BinaryData Test", result[0].StringField);
        Assert.Equal(123, result[0].IntField);
    }
}
