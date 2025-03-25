using System.Text.Json;
using CoreAbstractions;
using Kraken.REST.API.Client;
using Kraken.REST.API.Client.Sandbox.Types;

namespace JsonConverterTests;

public class OhlcJsonConverterTests
{
  private readonly JsonSerializerOptions _options;

  public OhlcJsonConverterTests()
  {
    _options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };
    _options.Converters.Add(new OhlcJsonConverter());
  }

  [Fact]
  public void Deserialize_ValidJson_ReturnsOhlcResponse()
  {
    string json = """
                  {
                    "error": [],
                    "result": {
                      "XXBTZUSD": [
                        [
                          1688671200,
                          "30306.1",
                          "30306.2",
                          "30305.7",
                          "30305.7",
                          "30306.1",
                          "3.39243896",
                          23
                        ],
                        [
                          1688671260,
                          "30304.5",
                          "30304.5",
                          "30300.0",
                          "30300.0",
                          "30300.0",
                          "4.42996871",
                          18
                        ],
                        [
                          1688671320,
                          "30300.3",
                          "30300.4",
                          "30291.4",
                          "30291.4",
                          "30294.7",
                          "2.13024789",
                          25
                        ],
                        [
                          1688671380,
                          "30291.8",
                          "30295.1",
                          "30291.8",
                          "30295.0",
                          "30293.8",
                          "1.01836275",
                          9
                        ]
                      ],
                      "last": 1688672160
                    }
                  }
                  """;
    var result = JsonSerializer.Deserialize<Result<OhlcResponse>>(json, _options);
    Assert.True(result.IsSuccess, "Expected successful deserialization");

    var ohlc = result.Value;
    Assert.Equal(1688672160, ohlc.Last);
    Assert.True(ohlc.Tickers.ContainsKey("XXBTZUSD"), "Expected ticker 'XXBTZUSD' to be present");

    var ticks = ohlc.Tickers["XXBTZUSD"];
    Assert.Equal(4, ticks.Length);

    // Verify the first tick.
    var tick1 = ticks[0];
    Assert.Equal(1688671200,   tick1.Time);
    Assert.Equal("30306.1",    tick1.Open);
    Assert.Equal("30306.2",    tick1.High);
    Assert.Equal("30305.7",    tick1.Low);
    Assert.Equal("30305.7",    tick1.Close);
    Assert.Equal("30306.1",    tick1.Vwap);
    Assert.Equal("3.39243896", tick1.Volume);
    Assert.Equal(23,           tick1.Count);
  }

  [Fact]
  public void Deserialize_TickDataArrayTooShort_ReturnsFailure()
  {
    // In this JSON, the first tick array is missing the final element (count).
    string json = """
                  {
                    "error": [],
                    "result": {
                      "XXBTZUSD": [
                        [
                          1688671200,
                          "30306.1",
                          "30306.2",
                          "30305.7",
                          "30305.7",
                          "30306.1",
                          "3.39243896"
                        ]
                      ],
                      "last": 1688672160
                    }
                  }
                  """;
    var result = JsonSerializer.Deserialize<Result<OhlcResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure due to missing tick data element");
    Assert.Contains("Expected number for count", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_ErrorPropertyNotArray_ReturnsFailure()
  {
    // The "error" property is not an array.
    string json = """
                  {
                    "error": "Some error",
                    "result": {
                      "XXBTZUSD": [
                        [
                          1688671200,
                          "30306.1",
                          "30306.2",
                          "30305.7",
                          "30305.7",
                          "30306.1",
                          "3.39243896",
                          23
                        ]
                      ],
                      "last": 1688672160
                    }
                  }
                  """;
    var result = JsonSerializer.Deserialize<Result<OhlcResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure because 'error' property is not an array");
    Assert.Contains("Expected StartArray", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_RootNotObject_ReturnsFailure()
  {
    string json   = @"[]";
    var    result = JsonSerializer.Deserialize<Result<OhlcResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure because the root is not an object");
    Assert.Contains("Expected StartObject", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_MissingResultProperty_ReturnsFailure()
  {
    string json = @"
{
  ""error"": []
}";
    var result = JsonSerializer.Deserialize<Result<OhlcResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure due to missing 'result' property");
    Assert.Contains("Expected 'result' property", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }
}