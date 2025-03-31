using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace Kraken.REST.API.Client.Types;

// Container record holding the overall recent trades response.
public sealed record RecentTradesResponse
{
    // "last" is an ID to be used as since when polling for new trade data.
    public string Last { get; init; } = string.Empty;
    // Maps asset pair names (e.g. "XXBTZUSD") to an array of TradeEntry.
    public Dictionary<string, TradeEntry[]> Trades { get; init; } = new();
}

// Represents a single trade entry.
public record TradeEntry
{
    public string Price     { get; init; } = string.Empty;
    public string Volume    { get; init; } = string.Empty;
    public double Time      { get; init; }
    public string Side      { get; init; } = string.Empty;
    public string OrderType { get; init; } = string.Empty;
    public string Misc      { get; init; } = string.Empty;
    public long   TradeId   { get; init; }
}

// Custom converter for deserializing the JSON into a Result<RecentTradesResponse>.
public class RecentTradesJsonConverter : JsonConverter<Result<RecentTradesResponse>>
{
    public override Result<RecentTradesResponse> Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
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
            var errorSweep = ErrorSweep(ref jsonReader);
            if (errorSweep.IsFailure)
                return new Error("ErrorSweep", errorSweep.Error);
            // We ignore the error messages.

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

            string last       = null;
            var    tradesDict = new Dictionary<string, TradeEntry[]>();

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
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for 'last' but found {jsonReader.TokenType}");
                    last = jsonReader.GetString();
                }
                else
                {
                    // Treat any other property as a trade array.
                    if (jsonReader.TokenType != JsonTokenType.StartArray)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartArray for trades under '{propName}' but found {jsonReader.TokenType}");
                    var tradeArrayResult = ReadTradeEntryArray(ref jsonReader, propName);
                    if (tradeArrayResult.IsFailure)
                        return tradeArrayResult.Error;
                    tradesDict.Add(propName, tradeArrayResult.Value);
                }
            }

            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.EndObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected EndObject for root but found {jsonReader.TokenType}");

            if (last == null)
                return new Error(ErrorCodes.MissingPropertyErrorCode, "Missing 'last' property in 'result'");

            return new RecentTradesResponse { Last = last, Trades = tradesDict };
        }
        catch (Exception ex)
        {
            return new Error("Exception", ex.Message);
        }
    }

    public override void Write(Utf8JsonWriter writer, Result<RecentTradesResponse> value, JsonSerializerOptions options)
    {
        // Serialization is not implemented in this example.
        throw new NotImplementedException();
    }

    // Reads an array of trade entry arrays for a given asset pair.
    private Result<TradeEntry[]> ReadTradeEntryArray(ref Utf8JsonReader jsonReader, string pairName)
    {
        var tradeList = new List<TradeEntry>();
        while (true)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType == JsonTokenType.EndArray)
                break;
            if (jsonReader.TokenType != JsonTokenType.StartArray)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartArray for trade entry in '{pairName}' but found {jsonReader.TokenType}");
            var tradeResult = ReadTradeEntry(ref jsonReader, pairName);
            if (tradeResult.IsFailure)
                return tradeResult.Error;
            tradeList.Add(tradeResult.Value);
        }
        return tradeList.ToArray();
    }

    // Reads a single trade entry array (expects exactly 7 elements).
    private Result<TradeEntry> ReadTradeEntry(ref Utf8JsonReader jsonReader, string pairName)
    {
        string price     = null;
        string volume    = null;
        double time      = 0;
        string side      = null;
        string orderType = null;
        string misc      = null;
        long   tradeId   = 0;

        // Read exactly 7 elements.
        for (int i = 0; i < 7; i++)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            switch (i)
            {
                case 0:
                    // Price: allow string or number.
                    if (jsonReader.TokenType == JsonTokenType.String)
                        price = jsonReader.GetString();
                    else if (jsonReader.TokenType == JsonTokenType.Number)
                        price = jsonReader.GetDecimal().ToString();
                    else
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string or number for price in '{pairName}', element {i}, found {jsonReader.TokenType}");
                    break;
                case 1:
                    // Volume: allow string or number.
                    if (jsonReader.TokenType == JsonTokenType.String)
                        volume = jsonReader.GetString();
                    else if (jsonReader.TokenType == JsonTokenType.Number)
                        volume = jsonReader.GetDecimal().ToString();
                    else
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string or number for volume in '{pairName}', element {i}, found {jsonReader.TokenType}");
                    break;
                case 2:
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected number for time in '{pairName}', element {i}, found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetDouble(out time))
                        return new Error("ParsingError", $"Failed to parse time for trade entry in '{pairName}'");
                    break;
                case 3:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for side in '{pairName}', element {i}, found {jsonReader.TokenType}");
                    side = jsonReader.GetString();
                    break;
                case 4:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for order type in '{pairName}', element {i}, found {jsonReader.TokenType}");
                    orderType = jsonReader.GetString();
                    break;
                case 5:
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for miscellaneous in '{pairName}', element {i}, found {jsonReader.TokenType}");
                    misc = jsonReader.GetString();
                    break;
                case 6:
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected number for trade id in '{pairName}', element {i}, found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt64(out tradeId))
                        return new Error("ParsingError", $"Failed to parse trade id for trade entry in '{pairName}'");
                    break;
            }
        }
        // Expect EndArray.
        var finalResult = jsonReader.ReadNext();
        if (finalResult.IsFailure)
            return new Error(ErrorCodes.StartReadingErrorCode, finalResult.Error);
        if (jsonReader.TokenType != JsonTokenType.EndArray)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected EndArray for trade entry in '{pairName}'");
        return new TradeEntry
        {
            Price     = price,
            Volume    = volume,
            Time      = time,
            Side      = side,
            OrderType = orderType,
            Misc      = misc,
            TradeId   = tradeId
        };
    }

    // Helper method to read the "error" array.
    private Result<string[]> ErrorSweep(ref Utf8JsonReader jsonReader)
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