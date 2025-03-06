using System.Text.Json;
using CoreAbstractions;
using RunnerTest;

namespace JsonConverterTests;

public class SystemStatusJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public SystemStatusJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new SystemStatusJsonConverter());
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsSystemStatus()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""status"": ""online"",
                ""timestamp"": ""2023-07-06T18:52:00Z""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<SystemStatus>>(json, _options);
        Assert.True(result.IsSuccess, "Expected a successful result");
        Assert.Equal("online",               result.Value.Status);
        Assert.Equal("2023-07-06T18:52:00Z", result.Value.Timestamp);
    }

    [Fact]
    public void Deserialize_MissingStatus_ReturnsFailure()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""timestamp"": ""2023-07-06T18:52:00Z""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<SystemStatus>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing status");
        Assert.Contains("status", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_MissingTimestamp_ReturnsFailure()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""status"": ""online""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<SystemStatus>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing timestamp");
        Assert.Contains("timestamp", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_InvalidStatus_ReturnsFailure()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""status"": ""invalid_status"",
                ""timestamp"": ""2023-07-06T18:52:00Z""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<SystemStatus>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to invalid status value");
        Assert.Contains("Invalid status value", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_InvalidTimestamp_ReturnsFailure()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""status"": ""online"",
                ""timestamp"": ""not-a-date""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<SystemStatus>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to invalid timestamp format");
        Assert.Contains("Invalid timestamp format", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_ErrorPropertyNotArray_ReturnsFailure()
    {
        string json = @"{
            ""error"": ""Some error"",
            ""result"": {
                ""status"": ""online"",
                ""timestamp"": ""2023-07-06T18:52:00Z""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<SystemStatus>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure because the error property is not an array");
        Assert.Contains("Expected StartArray", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_RootNotObject_ReturnsFailure()
    {
        string json   = @"[]";
        var    result = JsonSerializer.Deserialize<Result<SystemStatus>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure when root is not an object");
        Assert.Contains("Expected StartObject", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }
}