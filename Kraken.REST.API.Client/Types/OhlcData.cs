using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace Kraken.REST.API.Client.Types;

/// <summary>
///     Record struct representing a single tick of OHLC data.
/// </summary>
public readonly record struct TickData
{
    /// <summary>
    ///     Time of tick data.
    /// </summary>
    public int Time { get; init; }
    /// <summary>
    /// Opening price.
    /// </summary>
    public string Open   { get; init; }
    /// <summary>
    /// Highest price.
    /// </summary>
    public string High { get; init; }
    /// <summary>
    /// Lowest price.
    /// </summary>
    public string Low { get; init; }
    /// <summary>
    /// Closing price.
    /// </summary>
    public string Close { get; init; }
    /// <summary>
    /// Volume-weighted average price.
    /// </summary>
    public string Vwap { get; init; }
    /// <summary>
    /// Trading volume.
    /// </summary>
    public string Volume { get; init; }
    /// <summary>
    /// Number of trades.
    /// </summary>
    public int Count { get; init; }

}

/// <summary>
/// Represents OHLC response data containing last update ID and tickers data.
/// </summary>
public sealed record OhlcResponse
{
    /// <summary>
    /// Indicates the last received update ID.
    /// </summary>
    public int Last { get; init; }
    /// <summary>
    /// Contains tick data arrays grouped by ticker symbol.
    /// </summary>
    public Dictionary<string, TickData[]> Tickers { get; init; }
}

/// <summary>
/// JSON converter implementation for OHLC data.
/// </summary>
public class OhlcJsonConverter : JsonConverter<Result<OhlcResponse>>
{

    /// <summary>
    /// Deserializes OHLC response JSON content.
    /// </summary>
    /// <param name="jsonReader">UTF8 JSON reader reference.</param>
    /// <param name="typeToConvert">Object type to convert to.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>Deserialized OHLC response object.</returns>
    public override Result<OhlcResponse> Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            // Begin reading the root object.
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("StartReadingError", nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartObject but found {jsonReader.TokenType}");

            // Read the "error" property.
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("error"))
                return new Error(ErrorCodes.UnknownPropertyErrorCode, $"Expected 'error' property but found '{jsonReader.GetString()}'");
            var errorSweepResult = ErrorSweep(ref jsonReader);
            if (errorSweepResult.IsFailure)
                return new Error("ErrorSweep", errorSweepResult.Error);
            // (The errors are parsed but not used further.)

            // Read the "result" property.
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("result"))
                return new Error(ErrorCodes.UnknownPropertyErrorCode, $"Expected 'result' property but found '{jsonReader.GetString()}'");
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartObject for 'result' but found {jsonReader.TokenType}");

            int last    = 0;
            var tickers = new Dictionary<string, TickData[]>();

            // Loop through properties in "result".
            while (true)
            {
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure)
                    return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
                if (jsonReader.TokenType == JsonTokenType.EndObject)
                    break;
                if (jsonReader.TokenType != JsonTokenType.PropertyName)
                    return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected property name in 'result' but found {jsonReader.TokenType}");

                string propName = jsonReader.GetString();
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure)
                    return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

                if (propName == "last")
                {
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected number for 'last' but found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out last))
                        return new Error("ParsingError", "Failed to parse 'last' as integer");
                }
                else
                {
                    // Assume any other property is a ticker (array of tick data arrays).
                    if (jsonReader.TokenType != JsonTokenType.StartArray)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartArray for ticker '{propName}' but found {jsonReader.TokenType}");
                    var ticksResult = ReadTickDataArray(ref jsonReader, propName);
                    if (ticksResult.IsFailure)
                        return ticksResult.Error;
                    tickers.Add(propName, ticksResult.Value);
                }
            }

            // Read end of the root object.
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.EndObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected EndObject for root but found {jsonReader.TokenType}");

            return new OhlcResponse { Last = last, Tickers = tickers };
        }
        catch (Exception ex)
        {
            return new Error("Exception", ex.Message);
        }
    }

    /// <summary>
    /// Serializes OHLC response object to JSON.
    /// </summary>
    /// <param name="writer">UTF8 JSON writer.</param>
    /// <param name="value">OHLC response object to serialize.</param>
    /// <param name="options">Serialization options.</param>
    public override void Write(Utf8JsonWriter writer, Result<OhlcResponse> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads array of tick data from JSON.
    /// </summary>
    /// <param name="jsonReader">UTF8 JSON reader reference.</param>
    /// <param name="tickerKey">The ticker symbol key.</param>
    /// <returns>Deserialized tick data array.</returns>
    private static Result<TickData[]> ReadTickDataArray(ref Utf8JsonReader jsonReader, string tickerKey)
    {
        var tickDataList = new List<TickData>();
        while (true)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType == JsonTokenType.EndArray)
                break;
            if (jsonReader.TokenType != JsonTokenType.StartArray)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartArray for tick data in ticker '{tickerKey}', but found {jsonReader.TokenType}");
            var tickResult = ReadTickData(ref jsonReader, tickerKey);
            if (tickResult.IsFailure)
                return tickResult.Error;
            tickDataList.Add(tickResult.Value);
        }
        return tickDataList.ToArray();
    }

    /// <summary>
    /// Reads single tick data entry from JSON.
    /// </summary>
    /// <param name="jsonReader">UTF8 JSON reader reference.</param>
    /// <param name="tickerKey">The ticker symbol key.</param>
    /// <returns>Deserialized TickData object.</returns>
    private static Result<TickData> ReadTickData(ref Utf8JsonReader jsonReader, string tickerKey)
    {
        int    time   = 0;
        string open   = null;
        string high   = null;
        string low    = null;
        string close  = null;
        string vwap   = null;
        string volume = null;
        int    count  = 0;

        // Read exactly 8 elements.
        for (int i = 0; i < 8; i++)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            switch (i)
            {
                case 0:
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected number for time in ticker '{tickerKey}', element {i}, found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out time))
                        return new Error("ParsingError", $"Failed to parse time for ticker '{tickerKey}'");
                    break;
                case 1:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for open in ticker '{tickerKey}', element {i}, found {jsonReader.TokenType}");
                    open = jsonReader.GetString();
                    break;
                case 2:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for high in ticker '{tickerKey}', element {i}, found {jsonReader.TokenType}");
                    high = jsonReader.GetString();
                    break;
                case 3:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for low in ticker '{tickerKey}', element {i}, found {jsonReader.TokenType}");
                    low = jsonReader.GetString();
                    break;
                case 4:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for close in ticker '{tickerKey}', element {i}, found {jsonReader.TokenType}");
                    close = jsonReader.GetString();
                    break;
                case 5:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for vwap in ticker '{tickerKey}', element {i}, found {jsonReader.TokenType}");
                    vwap = jsonReader.GetString();
                    break;
                case 6:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for volume in ticker '{tickerKey}', element {i}, found {jsonReader.TokenType}");
                    volume = jsonReader.GetString();
                    break;
                case 7:
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected number for count in ticker '{tickerKey}', element {i}, found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out count))
                        return new Error("ParsingError", $"Failed to parse count for ticker '{tickerKey}'");
                    break;
            }
        }
        // Expect EndArray.
        var finalResult = jsonReader.ReadNext();
        if (finalResult.IsFailure)
            return new Error(ErrorCodes.StartReadingErrorCode, finalResult.Error);
        if (jsonReader.TokenType != JsonTokenType.EndArray)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected EndArray for tick data in ticker '{tickerKey}'");
        return new TickData { Time = time, Open = open, High = high, Low = low, Close = close, Vwap = vwap, Volume = volume, Count = count };
    }

    /// <summary>
    /// Sweeps and collects error messages from JSON.
    /// </summary>
    /// <param name="jsonReader">UTF8 JSON reader reference.</param>
    /// <returns>Array of error strings.</returns>
    private static Result<string[]> ErrorSweep(ref Utf8JsonReader jsonReader)
    {
        var nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error("StartReadingError", nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.StartArray)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartArray for 'error' but found {jsonReader.TokenType}");
        var errors = new List<string>();
        while (true)
        {
            var r = jsonReader.ReadNext();
            if (r.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, r.Error);
            if (jsonReader.TokenType == JsonTokenType.EndArray)
                break;
            if (jsonReader.TokenType != JsonTokenType.String)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string in error array but found {jsonReader.TokenType}");
            errors.Add(jsonReader.GetString());
        }
        return errors.ToArray();
    }
}
