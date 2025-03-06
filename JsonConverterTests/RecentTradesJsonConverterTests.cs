using System.Text.Json;
using CoreAbstractions;
using RunnerTest;

namespace JsonConverterTests;

public class RecentTradesJsonConverterTests
{
  private readonly JsonSerializerOptions _options;

  public RecentTradesJsonConverterTests()
  {
    _options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };
    _options.Converters.Add(new RecentTradesJsonConverter());
  }

  [Fact]
  public void Deserialize_ValidJson_ReturnsRecentTradesResponse()
  {
    string json = @"
{
  ""error"": [],
  ""result"": {
    ""XXBTZUSD"": [
      [
        ""30243.40000"",
        ""0.34507674"",
        1688669597.8277369,
        ""b"",
        ""m"",
        """",
        61044952
      ],
      [
        ""30243.30000"",
        ""0.00376960"",
        1688669598.2804112,
        ""s"",
        ""l"",
        """",
        61044953
      ],
      [
        ""30243.30000"",
        ""0.01235716"",
        1688669602.698379,
        ""s"",
        ""m"",
        """",
        61044956
      ]
    ],
    ""last"": ""1688671969993150842""
  }
}";
    var result = JsonSerializer.Deserialize<Result<RecentTradesResponse>>(json, _options);
    Assert.True(result.IsSuccess, "Expected successful deserialization");
    var response = result.Value;
    Assert.Equal("1688671969993150842", response.Last);
    Assert.True(response.Trades.ContainsKey("XXBTZUSD"), "Expected ticker 'XXBTZUSD' to be present");

    var trades = response.Trades["XXBTZUSD"];
    Assert.Equal(3, trades.Length);

    // Verify the first trade entry.
    var trade = trades[0];
    Assert.Equal("30243.40000", trade.Price);
    Assert.Equal("0.34507674",  trade.Volume);
    // Allow a small tolerance when comparing floating point numbers.
    Assert.True(Math.Abs(1688669597.8277369 - trade.Time) < 0.0001, "Trade time mismatch");
    Assert.Equal("b",      trade.Side);
    Assert.Equal("m",      trade.OrderType);
    Assert.Equal("",       trade.Misc);
    Assert.Equal(61044952, trade.TradeId);
  }

  [Fact]
  public void Deserialize_MissingLastProperty_ReturnsFailure()
  {
    // JSON where "last" property is missing from "result".
    string json = @"
{
  ""error"": [],
  ""result"": {
    ""XXBTZUSD"": [
      [
        ""30243.40000"",
        ""0.34507674"",
        1688669597.8277369,
        ""b"",
        ""m"",
        """",
        61044952
      ]
    ]
  }
}";
    var result = JsonSerializer.Deserialize<Result<RecentTradesResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure due to missing 'last' property");
    Assert.Contains("Missing 'last' property", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_TradeEntryArrayTooShort_ReturnsFailure()
  {
    // Provide a trade entry with only 6 elements (missing the trade id).
    string json = @"
{
  ""error"": [],
  ""result"": {
    ""XXBTZUSD"": [
      [
        ""30243.40000"",
        ""0.34507674"",
        1688669597.8277369,
        ""b"",
        ""m"",
        """"
      ]
    ],
    ""last"": ""1688671969993150842""
  }
}";
    var result = JsonSerializer.Deserialize<Result<RecentTradesResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure due to trade entry missing a field");
    Assert.Contains("Expected number for trade id", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_ErrorPropertyNotArray_ReturnsFailure()
  {
    // "error" property is provided as a string instead of an array.
    string json = @"
{
  ""error"": ""Some error"",
  ""result"": {
    ""XXBTZUSD"": [
      [
        ""30243.40000"",
        ""0.34507674"",
        1688669597.8277369,
        ""b"",
        ""m"",
        """",
        61044952
      ]
    ],
    ""last"": ""1688671969993150842""
  }
}";
    var result = JsonSerializer.Deserialize<Result<RecentTradesResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure because the 'error' property is not an array");
    Assert.Contains("Expected StartArray", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_RootNotObject_ReturnsFailure()
  {
    // Root is not an object.
    string json   = @"[]";
    var    result = JsonSerializer.Deserialize<Result<RecentTradesResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure because the root is not an object");
    Assert.Contains("Expected StartObject", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_MissingResultProperty_ReturnsFailure()
  {
    // JSON missing the "result" property.
    string json = @"
{
  ""error"": []
}";
    var result = JsonSerializer.Deserialize<Result<RecentTradesResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure due to missing 'result' property");
    Assert.Contains("Expected 'result' property", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }
}