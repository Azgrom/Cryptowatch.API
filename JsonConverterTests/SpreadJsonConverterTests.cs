using System.Text.Json;
using CoreAbstractions;
using Kraken.REST.API.Client;
using Kraken.REST.API.Client.Types;

namespace JsonConverterTests;

public class SpreadJsonConverterTests
{
  private readonly JsonSerializerOptions _options;

  public SpreadJsonConverterTests()
  {
    _options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };
    _options.Converters.Add(new SpreadJsonConverter());
  }

  [Fact]
  public void Deserialize_ValidJson_ReturnsSpreadResponse()
  {
    var result = JsonSerializer.Deserialize<Result<SpreadResponse>>(RestResponses.RecentSpreadsExample, _options);
    Assert.True(result.IsSuccess, "Expected successful deserialization");

    var response = result.Value;
    Assert.Equal(1688672106, response.Last);
    Assert.True(response.Spreads.ContainsKey("XXBTZUSD"), "Expected ticker 'XXBTZUSD' to be present");

    var spreads = response.Spreads["XXBTZUSD"];
    Assert.Equal(3, spreads.Length);

    // Verify the first spread entry.
    var spread1 = spreads[0];
    Assert.Equal(1688671834,    spread1.Time);
    Assert.Equal("30292.10000", spread1.Bid);
    Assert.Equal("30297.50000", spread1.Ask);
  }

  [Fact]
  public void Deserialize_MissingLastProperty_ReturnsFailure()
  {
    // Missing the "last" property from the "result" object.
    string json = @"
{
  ""error"": [],
  ""result"": {
    ""XXBTZUSD"": [
      [
        1688671834,
        ""30292.10000"",
        ""30297.50000""
      ]
    ]
  }
}";
    var result = JsonSerializer.Deserialize<Result<SpreadResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure due to missing 'last' property");
    Assert.Contains("Missing 'last' property", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_SpreadEntryArrayTooShort_ReturnsFailure()
  {
    // Provide a spread entry array with only two elements instead of three.
    string json = @"
{
  ""error"": [],
  ""result"": {
    ""XXBTZUSD"": [
      [
        1688671834,
        ""30292.10000""
      ]
    ],
    ""last"": 1688672106
  }
}";
    var result = JsonSerializer.Deserialize<Result<SpreadResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure due to spread entry array being too short");
    // Depending on implementation, the error message should mention the missing element (bid or ask).
    Assert.Contains("Expected", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_ErrorPropertyNotArray_ReturnsFailure()
  {
    // "error" property is not an array.
    string json = @"
{
  ""error"": ""Some error message"",
  ""result"": {
    ""XXBTZUSD"": [
      [
        1688671834,
        ""30292.10000"",
        ""30297.50000""
      ]
    ],
    ""last"": 1688672106
  }
}";
    var result = JsonSerializer.Deserialize<Result<SpreadResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure because 'error' property is not an array");
    Assert.Contains("Expected StartArray", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }

  [Fact]
  public void Deserialize_RootNotObject_ReturnsFailure()
  {
    // Root is not an object.
    string json   = @"[]";
    var    result = JsonSerializer.Deserialize<Result<SpreadResponse>>(json, _options);
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
    var result = JsonSerializer.Deserialize<Result<SpreadResponse>>(json, _options);
    Assert.True(result.IsFailure, "Expected failure due to missing 'result' property");
    Assert.Contains("Expected 'result' property", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
  }
}