using System.Text;
using System.Text.Json;
using Kraken.REST.API;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace JsonConverterTests;

public static class Helper
{
    public static Utf8JsonReader CreateReader(string json) =>
        new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
}

// These tests assume that your converter returns a Result<Ohlc> where IsFailure is true when an error is encountered,
// and that the Error property contains a Code matching the string constants used in the converter.
public class OrderBookJsonConverterTests
{
    [Fact]
    public void Test1()
    {
        var utf8JsonReader = Helper.CreateReader(RestResponses.ValidAssetPairResponse);
        var jsonConverter  = new OrderBookJsonConverter();

        var ohlcJsonConverter      = jsonConverter.IntoOhlc(ref utf8JsonReader);

        Console.WriteLine(ohlcJsonConverter);
    }

    [Fact]
        public void Deserialize_InvalidErrorProperty_ShouldReturnUnexpectedTokenError()
        {
            // "error" property is not an array.
            string json = @"
            {
              ""error"": ""some error"",
              ""result"": { 
                  ""XXBTZUSD"": { 
                      ""asks"": [],
                      ""bids"": []
                  }
              }
            }";
            var reader    = Helper.CreateReader(json);
            var converter = new OrderBookJsonConverter();

            var result = converter.Read(ref reader, typeof(OrderBook), options: null);

            Assert.True(result.IsFailure, "Deserialization should have failed.");
            // When the "error" property is not an array, the error sweep fails and returns UnexpectedTokenError.
            Assert.Equal("UnexpectedTokenError", result.Error.Code);
        }

        [Fact]
        public void Deserialize_MissingResultProperty_ShouldReturnUnexpectedTokenError()
        {
            // No "result" property is provided.
            string json = @"
            {
              ""error"": []
            }";
            var reader    = Helper.CreateReader(json);
            var converter = new OrderBookJsonConverter();

            var result = converter.Read(ref reader, typeof(OrderBook), options: null);

            Assert.True(result.IsFailure, "Deserialization should have failed due to missing result.");
            // In the absence of a "result" property, the converter ends up with a null order book and returns UnexpectedTokenError.
            Assert.Equal("UnexpectedTokenError", result.Error.Code);
        }

        [Fact]
        public void Deserialize_ResultNotObject_ShouldReturnEnteringObjectError()
        {
            // "result" property is present but is not an object.
            string json = @"
            {
              ""error"": [],
              ""result"": ""not an object""
            }";
            var reader    = Helper.CreateReader(json);
            var converter = new OrderBookJsonConverter();

            var result = converter.Read(ref reader, typeof(OrderBook), options: null);

            Assert.True(result.IsFailure, "Deserialization should have failed because result is not an object.");
            Assert.Equal("EnteringObjectError", result.Error.Code);
        }

        [Fact]
        public void Deserialize_MalformedAsksProperty_ShouldReturnEnteringPropertyNameError()
        {
            // Inside the order book, "asks" is not an array.
            string json = @"
            {
              ""error"": [],
              ""result"": {
                ""XXBTZUSD"": {
                  ""asks"": ""not an array"",
                  ""bids"": []
                }
              }
            }";
            var reader    = Helper.CreateReader(json);
            var converter = new OrderBookJsonConverter();

            var result = converter.Read(ref reader, typeof(OrderBook), options: null);

            Assert.True(result.IsFailure, "Deserialization should have failed because asks is not an array.");
            Assert.Equal("EnteringPropertyNameError", result.Error.Code);
        }

        [Fact]
        public void Deserialize_MalformedBidsProperty_ShouldReturnEnteringPropertyNameError()
        {
            // Inside the order book, "bids" is not an array.
            string json = @"
            {
              ""error"": [],
              ""result"": {
                ""XXBTZUSD"": {
                  ""asks"": [],
                  ""bids"": ""not an array""
                }
              }
            }";
            var reader    = Helper.CreateReader(json);
            var converter = new OrderBookJsonConverter();

            var result = converter.Read(ref reader, typeof(OrderBook), options: null);

            Assert.True(result.IsFailure, "Deserialization should have failed because bids is not an array.");
            Assert.Equal("EnteringPropertyNameError", result.Error.Code);
        }

        [Fact]
        public void Deserialize_UnexpectedProperty_ShouldReturnUnknownPropertyError()
        {
            // JSON contains a property that is neither "error" nor "result".
            string json = @"
            {
              ""foobar"": ""baz""
            }";
            var reader    = Helper.CreateReader(json);
            var converter = new OrderBookJsonConverter();

            var result = converter.Read(ref reader, typeof(OrderBook), options: null);

            Assert.True(result.IsFailure, "Deserialization should have failed due to an unexpected property.");
            Assert.Equal("UnknownPropertyError", result.Error.Code);
        }
}
