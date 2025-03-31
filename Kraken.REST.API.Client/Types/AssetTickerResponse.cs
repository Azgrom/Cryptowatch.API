using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace Kraken.REST.API.Client.Types;

/// <summary>
/// Represents the response containing asset ticker data.
/// </summary>
public sealed record AssetTickerResponse
{
    public AssetTickerResponse(Dictionary<string, AssetTickerInfo> tickers) { Tickers = tickers; }
    public Dictionary<string, AssetTickerInfo> Tickers { get; init; }
}

/// <summary>
///  Represents the ticker information for an asset.
/// </summary>
public record AssetTickerInfo
{
    /// <summary>
    ///  Ask: [<price>, <whole lot volume>, <lot volume>]
    /// </summary>
    public string[] A { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Bid: [<price>, <whole lot volume>, <lot volume>]
    /// </summary>
    public string[] B { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Last trade closed: [<price>, <lot volume>]
    /// </summary>
    public string[] C { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Volume: [<today>, <last 24 hours>]
    /// </summary>
    public string[] V { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Volume weighted average price: [<today>, <last 24 hours>]
    /// </summary>
    public string[] P { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Number of trades: [<today>, <last 24 hours>]
    /// </summary>
    public int[] T { get; init; } = Array.Empty<int>();

    /// <summary>
    /// Low: [<today>, <last 24 hours>]
    /// </summary>
    public string[] L { get; init; } = Array.Empty<string>();

    /// <summary>
    /// High: [<today>, <last 24 hours>]
    /// </summary>
    public string[] H { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Today's opening price
    /// </summary>
    public string O { get; init; } = string.Empty;
}

/// <summary>
/// Custom converter for deserializing the JSON into a Result<AssetTickerResponse>.
/// It uses error propagation so that any underlying parsing errors are returned as an Error.
/// </summary>
public sealed class AssetTickerJsonConverter : JsonConverter<Result<AssetTickerResponse>>
{
    public override Result<AssetTickerResponse> Read(
        ref Utf8JsonReader    jsonReader,
        Type                  typeToConvert,
        JsonSerializerOptions options
    )
    {
        // Begin reading the root object.
        var nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.StartObject)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartObject but found {jsonReader.TokenType}");

        // Read the "error" property.
        nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("error"))
            return new Error(ErrorCodes.UnknownPropertyErrorCode,
                $"Expected 'error' property but found '{jsonReader.GetString()}'");
        var errorSweepResult = ErrorSweep(ref jsonReader);
        if (errorSweepResult.IsFailure) return new Error("ErrorSweep", errorSweepResult.Error);
        // (The errors array is parsed but not used further.)

        // Read the next property, which must be "result".
        nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("result"))
            return new Error(ErrorCodes.UnknownPropertyErrorCode,
                $"Expected 'result' property but found '{jsonReader.GetString()}'");

        nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.StartObject)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                $"Expected StartObject for 'result' but found {jsonReader.TokenType}");

        // Loop through all properties in the "result" object.
        var tickers = new Dictionary<string, AssetTickerInfo>();
        while (true)
        {
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType == JsonTokenType.EndObject) break;
            if (jsonReader.TokenType != JsonTokenType.PropertyName)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                    $"Expected property name in 'result' but found {jsonReader.TokenType}");

            string tickerKey = jsonReader.GetString();
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                    $"Expected StartObject for ticker '{tickerKey}' but found {jsonReader.TokenType}");

            var tickerResult = ReadAssetTickerInfo(ref jsonReader, tickerKey);
            if (tickerResult.IsFailure) return tickerResult.Error;
            tickers.Add(tickerKey, tickerResult.Value);
        }

        nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.EndObject)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                $"Expected EndObject for root but found {jsonReader.TokenType}");

        return new AssetTickerResponse(tickers);
    }

    public override void Write(Utf8JsonWriter writer, Result<AssetTickerResponse> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("error");
        writer.WriteStartArray();
        if (value.IsFailure)
        {
            writer.WriteStringValue(value.Error.Code);
            writer.WriteStringValue(value.Error.Message);
        }

        writer.WriteEndArray();

        writer.WritePropertyName("result");
        writer.WriteStartObject();
        if (value.IsSuccess)
        {
            foreach (var kvp in value.Value.Tickers)
            {
                writer.WritePropertyName(kvp.Key);
                writer.WriteStartObject();
                // Write array properties.
                writer.WritePropertyName("a");
                writer.WriteStartArray();
                foreach (var s in kvp.Value.A) writer.WriteStringValue(s);
                writer.WriteEndArray();

                writer.WritePropertyName("b");
                writer.WriteStartArray();
                foreach (var s in kvp.Value.B) writer.WriteStringValue(s);
                writer.WriteEndArray();

                writer.WritePropertyName("c");
                writer.WriteStartArray();
                foreach (var s in kvp.Value.C) writer.WriteStringValue(s);
                writer.WriteEndArray();

                writer.WritePropertyName("v");
                writer.WriteStartArray();
                foreach (var s in kvp.Value.V) writer.WriteStringValue(s);
                writer.WriteEndArray();

                writer.WritePropertyName("p");
                writer.WriteStartArray();
                foreach (var s in kvp.Value.P) writer.WriteStringValue(s);
                writer.WriteEndArray();

                writer.WritePropertyName("t");
                writer.WriteStartArray();
                foreach (var n in kvp.Value.T) writer.WriteNumberValue(n);
                writer.WriteEndArray();

                writer.WritePropertyName("l");
                writer.WriteStartArray();
                foreach (var s in kvp.Value.L) writer.WriteStringValue(s);
                writer.WriteEndArray();

                writer.WritePropertyName("h");
                writer.WriteStartArray();
                foreach (var s in kvp.Value.H) writer.WriteStringValue(s);
                writer.WriteEndArray();

                writer.WriteString("o", kvp.Value.O);
                writer.WriteEndObject();
            }
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    // Reads an AssetTickerInfo object from the JSON.
    private static Result<AssetTickerInfo> ReadAssetTickerInfo(ref Utf8JsonReader jsonReader, string tickerKey)
    {
        string[] a = null;
        string[] b = null;
        string[] c = null;
        string[] v = null;
        string[] p = null;
        int[]    t = null;
        string[] l = null;
        string[] h = null;
        string   o = null;

        bool foundA = false,
            foundB  = false,
            foundC  = false,
            foundV  = false,
            foundP  = false,
            foundT  = false,
            foundL  = false,
            foundH  = false,
            foundO  = false;

        while (true)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType == JsonTokenType.EndObject) break;
            if (jsonReader.TokenType != JsonTokenType.PropertyName)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                    $"Expected property name in ticker '{tickerKey}' but found {jsonReader.TokenType}");

            string propName = jsonReader.GetString();
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

            switch (propName)
            {
                case "a":
                    a      = ReadStringArray(ref jsonReader);
                    foundA = true;
                    break;
                case "b":
                    b      = ReadStringArray(ref jsonReader);
                    foundB = true;
                    break;
                case "c":
                    c      = ReadStringArray(ref jsonReader);
                    foundC = true;
                    break;
                case "v":
                    v      = ReadStringArray(ref jsonReader);
                    foundV = true;
                    break;
                case "p":
                    p      = ReadStringArray(ref jsonReader);
                    foundP = true;
                    break;
                case "t":
                    t      = ReadIntArray(ref jsonReader);
                    foundT = true;
                    break;
                case "l":
                    l      = ReadStringArray(ref jsonReader);
                    foundL = true;
                    break;
                case "h":
                    h      = ReadStringArray(ref jsonReader);
                    foundH = true;
                    break;
                case "o":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                            $"Expected string for 'o' in ticker '{tickerKey}', found {jsonReader.TokenType}");
                    o      = jsonReader.GetString();
                    foundO = true;
                    break;
                default: jsonReader.Skip(); break;
            }
        }

        if (!foundA) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'a' for ticker '{tickerKey}'");
        if (!foundB) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'b' for ticker '{tickerKey}'");
        if (!foundC) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'c' for ticker '{tickerKey}'");
        if (!foundV) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'v' for ticker '{tickerKey}'");
        if (!foundP) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'p' for ticker '{tickerKey}'");
        if (!foundT) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 't' for ticker '{tickerKey}'");
        if (!foundL) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'l' for ticker '{tickerKey}'");
        if (!foundH) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'h' for ticker '{tickerKey}'");
        if (!foundO) return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'o' for ticker '{tickerKey}'");

        return new AssetTickerInfo
        {
            A = a,
            B = b,
            C = c,
            V = v,
            P = p,
            T = t,
            L = l,
            H = h,
            O = o
        };
    }

    // Reads an array of strings from the current JSON array.
    private static string[] ReadStringArray(ref Utf8JsonReader jsonReader)
    {
        var list = new List<string>();
        if (jsonReader.TokenType != JsonTokenType.StartArray)
            throw new InvalidOperationException("Expected StartArray");
        while (true)
        {
            var r = jsonReader.ReadNext();
            if (r.IsFailure) throw new InvalidOperationException("Error reading array element");
            if (jsonReader.TokenType == JsonTokenType.EndArray) break;
            if (jsonReader.TokenType != JsonTokenType.String)
                throw new InvalidOperationException("Expected string element in array");
            list.Add(jsonReader.GetString());
        }

        return list.ToArray();
    }

    // Reads an array of integers from the current JSON array.
    private static int[] ReadIntArray(ref Utf8JsonReader jsonReader)
    {
        var list = new List<int>();
        if (jsonReader.TokenType != JsonTokenType.StartArray)
            throw new InvalidOperationException("Expected StartArray");
        while (true)
        {
            var r = jsonReader.ReadNext();
            if (r.IsFailure) throw new InvalidOperationException("Error reading array element");
            if (jsonReader.TokenType == JsonTokenType.EndArray) break;
            if (jsonReader.TokenType != JsonTokenType.Number)
                throw new InvalidOperationException("Expected number element in array");
            if (!jsonReader.TryGetInt32(out int value))
                throw new InvalidOperationException("Failed to parse number in array");
            list.Add(value);
        }

        return list.ToArray();
    }

    // Helper to read the "error" array from the JSON.
    private static Result<string[]> ErrorSweep(ref Utf8JsonReader jsonReader)
    {
        var nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.StartArray)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                $"Expected StartArray for 'error' but found {jsonReader.TokenType}");
        var errors = new List<string>();
        while (true)
        {
            var r = jsonReader.ReadNext();
            if (r.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, r.Error);
            if (jsonReader.TokenType == JsonTokenType.EndArray) break;
            if (jsonReader.TokenType == JsonTokenType.String)
                errors.Add(jsonReader.GetString());
            else
                return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                    $"Expected string in error array but found {jsonReader.TokenType}");
        }

        return errors.ToArray();
    }
}
