using System.Text.Json;
using CoreAbstractions;
using Kraken.REST.API.Client;
using Kraken.REST.API.Client.Types;

namespace JsonConverterTests;

public class AssetTickerJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public AssetTickerJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new AssetTickerJsonConverter());
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsAssetTickerResponse()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""XXBTZUSD"": {
                    ""a"": [""30300.10000"", ""1"", ""1.000""],
                    ""b"": [""30300.00000"", ""1"", ""1.000""],
                    ""c"": [""30303.20000"", ""0.00067643""],
                    ""v"": [""4083.67001100"", ""4412.73601799""],
                    ""p"": [""30706.77771"", ""30689.13205""],
                    ""t"": [34619, 38907],
                    ""l"": [""29868.30000"", ""29868.30000""],
                    ""h"": [""31631.00000"", ""31631.00000""],
                    ""o"": ""30502.80000""
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetTickerResponse>>(json, _options);
        Assert.True(result.IsSuccess, "Expected successful deserialization");

        var ticker = result.Value.Tickers["XXBTZUSD"];
        Assert.Equal(new string[] { "30300.10000", "1", "1.000" },      ticker.A);
        Assert.Equal(new string[] { "30300.00000", "1", "1.000" },      ticker.B);
        Assert.Equal(new string[] { "30303.20000", "0.00067643" },      ticker.C);
        Assert.Equal(new string[] { "4083.67001100", "4412.73601799" }, ticker.V);
        Assert.Equal(new string[] { "30706.77771", "30689.13205" },     ticker.P);
        Assert.Equal(new int[] { 34619, 38907 },                        ticker.T);
        Assert.Equal(new string[] { "29868.30000", "29868.30000" },     ticker.L);
        Assert.Equal(new string[] { "31631.00000", "31631.00000" },     ticker.H);
        Assert.Equal("30502.80000",                                     ticker.O);
    }

    [Fact]
    public void Deserialize_MissingOpeningPrice_ReturnsFailure()
    {
        // "o" is missing.
        string json = @"{
            ""error"": [],
            ""result"": {
                ""XXBTZUSD"": {
                    ""a"": [""30300.10000"", ""1"", ""1.000""],
                    ""b"": [""30300.00000"", ""1"", ""1.000""],
                    ""c"": [""30303.20000"", ""0.00067643""],
                    ""v"": [""4083.67001100"", ""4412.73601799""],
                    ""p"": [""30706.77771"", ""30689.13205""],
                    ""t"": [34619, 38907],
                    ""l"": [""29868.30000"", ""29868.30000""],
                    ""h"": [""31631.00000"", ""31631.00000""]
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetTickerResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing 'o' property");
        Assert.Contains("Missing property 'o'", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_InvalidTradesArray_ReturnsFailure()
    {
        // The "t" property should be an array of numbers. Here one element is a string.
        string json = @"{
            ""error"": [],
            ""result"": {
                ""XXBTZUSD"": {
                    ""a"": [""30300.10000"", ""1"", ""1.000""],
                    ""b"": [""30300.00000"", ""1"", ""1.000""],
                    ""c"": [""30303.20000"", ""0.00067643""],
                    ""v"": [""4083.67001100"", ""4412.73601799""],
                    ""p"": [""30706.77771"", ""30689.13205""],
                    ""t"": [""invalid"", 38907],
                    ""l"": [""29868.30000"", ""29868.30000""],
                    ""h"": [""31631.00000"", ""31631.00000""],
                    ""o"": ""30502.80000""
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetTickerResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to invalid element in 't' array");
        Assert.Contains("Expected number element in array", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_ErrorPropertyNotArray_ReturnsFailure()
    {
        string json = @"{
            ""error"": ""Some error"",
            ""result"": {
                ""XXBTZUSD"": {
                    ""a"": [""30300.10000"", ""1"", ""1.000""],
                    ""b"": [""30300.00000"", ""1"", ""1.000""],
                    ""c"": [""30303.20000"", ""0.00067643""],
                    ""v"": [""4083.67001100"", ""4412.73601799""],
                    ""p"": [""30706.77771"", ""30689.13205""],
                    ""t"": [34619, 38907],
                    ""l"": [""29868.30000"", ""29868.30000""],
                    ""h"": [""31631.00000"", ""31631.00000""],
                    ""o"": ""30502.80000""
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetTickerResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure because the 'error' property is not an array");
        Assert.Contains("Expected StartArray", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_RootNotObject_ReturnsFailure()
    {
        string json   = @"[]";
        var    result = JsonSerializer.Deserialize<Result<AssetTickerResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure because the root is not an object");
        Assert.Contains("Expected StartObject", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_MissingResultProperty_ReturnsFailure()
    {
        string json = @"{
            ""error"": []
        }";
        var result = JsonSerializer.Deserialize<Result<AssetTickerResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing 'result' property");
        Assert.Contains("Expected 'result' property", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }
}