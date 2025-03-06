using System.Text.Json;
using CoreAbstractions;
using RunnerTest;

namespace JsonConverterTests;

public class AssetInfoJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public AssetInfoJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new AssetInfoJsonConverter());
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsAssetInfoResponse()
    {
        string json = @"{
            ""error"": [],
            ""result"": {
                ""XXBT"": {
                    ""aclass"": ""currency"",
                    ""altname"": ""XBT"",
                    ""decimals"": 10,
                    ""display_decimals"": 5,
                    ""collateral_value"": 1,
                    ""status"": ""enabled""
                },
                ""ZEUR"": {
                    ""aclass"": ""currency"",
                    ""altname"": ""EUR"",
                    ""decimals"": 4,
                    ""display_decimals"": 2,
                    ""collateral_value"": 1,
                    ""status"": ""enabled""
                },
                ""ZUSD"": {
                    ""aclass"": ""currency"",
                    ""altname"": ""USD"",
                    ""decimals"": 4,
                    ""display_decimals"": 2,
                    ""collateral_value"": 1,
                    ""status"": ""enabled""
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetInfoResponse>>(json, _options);
        Assert.True(result.IsSuccess, "Expected successful deserialization");
        Assert.NotNull(result.Value);

        // Verify the dictionary contains three assets.
        var assets = result.Value.Assets;
        Assert.Equal(3, assets.Count);
        Assert.True(assets.ContainsKey("XXBT"));
        Assert.True(assets.ContainsKey("ZEUR"));
        Assert.True(assets.ContainsKey("ZUSD"));

        // Verify one asset's details.
        var xxbt = assets["XXBT"];
        Assert.Equal("currency", xxbt.Aclass);
        Assert.Equal("XBT",      xxbt.Altname);
        Assert.Equal(10,         xxbt.Decimals);
        Assert.Equal(5,          xxbt.DisplayDecimals);
        Assert.Equal(1,          xxbt.CollateralValue);
        Assert.Equal("enabled",  xxbt.Status);
    }

    [Fact]
    public void Deserialize_MissingAclass_ReturnsFailure()
    {
        // Remove "aclass" from the XXBT asset.
        string json = @"{
            ""error"": [],
            ""result"": {
                ""XXBT"": {
                    ""altname"": ""XBT"",
                    ""decimals"": 10,
                    ""display_decimals"": 5,
                    ""collateral_value"": 1,
                    ""status"": ""enabled""
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetInfoResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing 'aclass'");
        Assert.Contains("Missing property 'aclass'", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_InvalidStatus_ReturnsFailure()
    {
        // Provide an invalid status value.
        string json = @"{
            ""error"": [],
            ""result"": {
                ""ZEUR"": {
                    ""aclass"": ""currency"",
                    ""altname"": ""EUR"",
                    ""decimals"": 4,
                    ""display_decimals"": 2,
                    ""collateral_value"": 1,
                    ""status"": ""not_enabled""
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetInfoResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to invalid status value");
        Assert.Contains("Invalid status value", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_ErrorPropertyNotArray_ReturnsFailure()
    {
        string json = @"{
            ""error"": ""Some error"",
            ""result"": {
                ""XXBT"": {
                    ""aclass"": ""currency"",
                    ""altname"": ""XBT"",
                    ""decimals"": 10,
                    ""display_decimals"": 5,
                    ""collateral_value"": 1,
                    ""status"": ""enabled""
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetInfoResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure because 'error' property is not an array");
        Assert.Contains("Expected StartArray", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_RootNotObject_ReturnsFailure()
    {
        string json   = @"[]";
        var    result = JsonSerializer.Deserialize<Result<AssetInfoResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure because root is not an object");
        Assert.Contains("Expected StartObject", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_MissingResultProperty_ReturnsFailure()
    {
        // JSON with no "result" property.
        string json = @"{
            ""error"": []
        }";

        var result = JsonSerializer.Deserialize<Result<AssetInfoResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing 'result' property");
        Assert.Contains("Expected 'result' property", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }
}