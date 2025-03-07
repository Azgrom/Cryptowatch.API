using System.Text.Json;
using CoreAbstractions;
using Kraken.REST.API;

namespace JsonConverterTests;

public class AssetPairJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public AssetPairJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new AssetPairJsonConverter());
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsAssetPairResponse()
    {
        var result = JsonSerializer.Deserialize<Result<AssetPairResponse>>(RestResponses.TradableAssetPairExample, _options);
        Assert.True(result.IsSuccess, "Expected successful deserialization.");
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Pairs.Count);
        Assert.Contains("XETHXXBT", result.Value.Pairs.Keys);
        Assert.Contains("XXBTZUSD", result.Value.Pairs.Keys);

        var pair1 = result.Value.Pairs["XETHXXBT"];
        Assert.Equal("ETHXBT",                 pair1.Altname);
        Assert.Equal("ETH/XBT",                pair1.Wsname);
        Assert.Equal("currency",               pair1.AclassBase);
        Assert.Equal("XETH",                   pair1.Base);
        Assert.Equal("currency",               pair1.AclassQuote);
        Assert.Equal("XXBT",                   pair1.Quote);
        Assert.Equal("unit",                   pair1.Lot);
        Assert.Equal(5,                        pair1.PairDecimals);
        Assert.Equal(6,                        pair1.CostDecimals);
        Assert.Equal(8,                        pair1.LotDecimals);
        Assert.Equal(1,                        pair1.LotMultiplier);
        Assert.Equal(new int[] { 2, 3, 4, 5 }, pair1.LeverageBuy);
        Assert.Equal(new int[] { 2, 3, 4, 5 }, pair1.LeverageSell);
        Assert.NotEmpty(pair1.Fees);
        Assert.NotEmpty(pair1.FeesMaker);
        Assert.Equal("ZUSD",    pair1.FeeVolumeCurrency);
        Assert.Equal(80,        pair1.MarginCall);
        Assert.Equal(40,        pair1.MarginStop);
        Assert.Equal("0.01",    pair1.Ordermin);
        Assert.Equal("0.00002", pair1.Costmin);
        Assert.Equal("0.00001", pair1.TickSize);
        Assert.Equal("online",  pair1.Status);
        Assert.Equal(1100,      pair1.LongPositionLimit);
        Assert.Equal(400,       pair1.ShortPositionLimit);
    }

    [Fact]
    public void Deserialize_MissingRequiredProperty_ReturnsFailure()
    {
        // Remove "altname" from the XETHXXBT asset pair.
        string json = @"{
            ""error"": [],
            ""result"": {
                ""XETHXXBT"": {
                    ""wsname"": ""ETH/XBT"",
                    ""aclass_base"": ""currency"",
                    ""base"": ""XETH"",
                    ""aclass_quote"": ""currency"",
                    ""quote"": ""XXBT"",
                    ""lot"": ""unit"",
                    ""pair_decimals"": 5,
                    ""cost_decimals"": 6,
                    ""lot_decimals"": 8,
                    ""lot_multiplier"": 1,
                    ""leverage_buy"": [2, 3, 4, 5],
                    ""leverage_sell"": [2, 3, 4, 5],
                    ""fees"": [
                        [0, 0.26]
                    ],
                    ""fees_maker"": [
                        [0, 0.16]
                    ],
                    ""fee_volume_currency"": ""ZUSD"",
                    ""margin_call"": 80,
                    ""margin_stop"": 40,
                    ""ordermin"": ""0.01"",
                    ""costmin"": ""0.00002"",
                    ""tick_size"": ""0.00001"",
                    ""status"": ""online"",
                    ""long_position_limit"": 1100,
                    ""short_position_limit"": 400
                }
            }
        }";

        var result = JsonSerializer.Deserialize<Result<AssetPairResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing 'altname'");
        Assert.Contains("Missing property 'altname'", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_InvalidStatus_ReturnsFailure()
    {
        // Set an invalid status value.
        string json = @"{
            ""error"": [],
            ""result"": {
                ""XXBTZUSD"": {
                    ""altname"": ""XBTUSD"",
                    ""wsname"": ""XBT/USD"",
                    ""aclass_base"": ""currency"",
                    ""base"": ""XXBT"",
                    ""aclass_quote"": ""currency"",
                    ""quote"": ""ZUSD"",
                    ""lot"": ""unit"",
                    ""pair_decimals"": 1,
                    ""cost_decimals"": 5,
                    ""lot_decimals"": 8,
                    ""lot_multiplier"": 1,
                    ""leverage_buy"": [2, 3, 4, 5],
                    ""leverage_sell"": [2, 3, 4, 5],
                    ""fees"": [
                        [0, 0.26]
                    ],
                    ""fees_maker"": [
                        [0, 0.16]
                    ],
                    ""fee_volume_currency"": ""ZUSD"",
                    ""margin_call"": 80,
                    ""margin_stop"": 40,
                    ""ordermin"": ""0.0001"",
                    ""costmin"": ""0.5"",
                    ""tick_size"": ""0.1"",
                    ""status"": ""offline"",
                    ""long_position_limit"": 250,
                    ""short_position_limit"": 200
                }
            }
        }";
        var result = JsonSerializer.Deserialize<Result<AssetPairResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to invalid status value");
        Assert.Contains("Invalid status value", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_ErrorPropertyNotArray_ReturnsFailure()
    {
        string json = @"{
            ""error"": ""Some error"",
            ""result"": {
                ""XETHXXBT"": {
                    ""altname"": ""ETHXBT"",
                    ""wsname"": ""ETH/XBT"",
                    ""aclass_base"": ""currency"",
                    ""base"": ""XETH"",
                    ""aclass_quote"": ""currency"",
                    ""quote"": ""XXBT"",
                    ""lot"": ""unit"",
                    ""pair_decimals"": 5,
                    ""cost_decimals"": 6,
                    ""lot_decimals"": 8,
                    ""lot_multiplier"": 1,
                    ""leverage_buy"": [2, 3, 4, 5],
                    ""leverage_sell"": [2, 3, 4, 5],
                    ""fees"": [
                        [0, 0.26]
                    ],
                    ""fees_maker"": [
                        [0, 0.16]
                    ],
                    ""fee_volume_currency"": ""ZUSD"",
                    ""margin_call"": 80,
                    ""margin_stop"": 40,
                    ""ordermin"": ""0.01"",
                    ""costmin"": ""0.00002"",
                    ""tick_size"": ""0.00001"",
                    ""status"": ""online"",
                    ""long_position_limit"": 1100,
                    ""short_position_limit"": 400
                }
            }
        }";
        var result = JsonSerializer.Deserialize<Result<AssetPairResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure because the 'error' property is not an array");
        Assert.Contains("Expected StartArray", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_RootNotObject_ReturnsFailure()
    {
        string json   = @"[]";
        var    result = JsonSerializer.Deserialize<Result<AssetPairResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure because root is not an object");
        Assert.Contains("Expected StartObject", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Deserialize_MissingResultProperty_ReturnsFailure()
    {
        string json = @"{
            ""error"": []
        }";
        var result = JsonSerializer.Deserialize<Result<AssetPairResponse>>(json, _options);
        Assert.True(result.IsFailure, "Expected failure due to missing 'result' property");
        Assert.Contains("Expected 'result' property", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }
}