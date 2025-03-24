using System.Text.Json;
using CoreAbstractions;
using Kraken.REST.API.Client;
using Kraken.REST.API.Client.Types;

namespace JsonConverterTests;

public class ServerTimeJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public ServerTimeJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new ServerTimeInfoJsonConverter());
    }
    
    [Fact]
    public void Test1()
    {
        var utf8JsonReader              = Helper.CreateReader(RestResponses.ServerTimeExample);
        var serverServerTimeJsonConverter = new ServerTimeInfoJsonConverter();

        var result = serverServerTimeJsonConverter.Read(ref utf8JsonReader, typeof(Result<ServerTime>), null);

        Console.WriteLine(result);
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsServerTime()
    {
        string json = """
                      {
                        "error": [],
                        "result": {
                          "unixtime": 1688669448,
                          "rfc1123": "Thu, 06 Jul 23 18:50:48 +0000"
                        }
                      }
                      """;

        var result = JsonSerializer.Deserialize<Result<ServerTime>>(json, _options);
        Assert.True(result.IsSuccess, "Expected a successful result");
        Assert.Equal(1688669448, result.Value.Unixtime);
        Assert.Equal("Thu, 06 Jul 23 18:50:48 +0000", result.Value.Rfc1123);
    }

    [Fact]
    public void Deserialize_MissingUnixtime_ReturnsFailure()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""rfc1123"": ""Thu, 06 Jul 23 18:50:48 +0000""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<ServerTime>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing unixtime");
        Assert.Contains("unixtime", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_MissingRfc1123_ReturnsFailure()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""unixtime"": 1688669448
            }
        }";

        var result = JsonSerializer.Deserialize<Result<ServerTime>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing rfc1123");
        Assert.Contains("rfc1123", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_InvalidRoot_ReturnsFailure()
    {
        // Root is not an object.
        string json = @"[]";
        var result = JsonSerializer.Deserialize<Result<ServerTime>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure when root is not an object");
        Assert.Contains("Expected StartObject", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_ErrorPropertyNotArray_ReturnsFailure()
    {
        string json = @"{
            ""error"": ""Some error"",
            ""result"": {
                ""unixtime"": 1688669448,
                ""rfc1123"": ""Thu, 06 Jul 23 18:50:48 +0000""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<ServerTime>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure because the error property is not an array");
        Assert.Contains("Expected StartArray", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_UnixtimeAsString_ReturnsServerTime()
    {
        // unixtime provided as a string.
        string json = @"{
            ""error"": [],
            ""result"": {
                ""unixtime"": ""1688669448"",
                ""rfc1123"": ""Thu, 06 Jul 23 18:50:48 +0000""
            }
        }";

        var result = JsonSerializer.Deserialize<Result<ServerTime>>(json, _options);
        Assert.True(result.IsSuccess, "Expected a successful result when unixtime is a string");
        Assert.Equal(1688669448, result.Value.Unixtime);
        Assert.Equal("Thu, 06 Jul 23 18:50:48 +0000", result.Value.Rfc1123);
    }
}
