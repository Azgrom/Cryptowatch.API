using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace RunnerTest;

// Represents a single spread entry.
public record SpreadData
{
    public int    Time { get; init; }
    public string Bid  { get; init; } = string.Empty;
    public string Ask  { get; init; } = string.Empty;
}

// Container record holding the overall spread response.
public sealed record SpreadResponse
{
    public int                              Last    { get; init; }
    public Dictionary<string, SpreadData[]> Spreads { get; init; } = new();
}

// Custom converter for deserializing the JSON into a Result<SpreadResponse>.
public class SpreadJsonConverter : JsonConverter<Result<SpreadResponse>>
{
    public override Result<SpreadResponse> Read(
        ref Utf8JsonReader    jsonReader,
        Type                  typeToConvert,
        JsonSerializerOptions options
    )
    {
        try
        {
            // Begin reading the root object.if (jsonReader.TokenType is JsonTokenType.None)
            if (jsonReader.TokenType is JsonTokenType.None)
            {
                if (jsonReader.ReadNext().IsFailure) return new Error("StartReadingError", jsonReader.ReadNext().Error);
            }
            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error("UnexpectedTokenError", $"Expected StartObject but found {jsonReader.TokenType}");

            // Read the "error" property.
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error("ReadingTokenError", nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("error"))
                return new Error("UnknownPropertyError",
                    $"Expected 'error' property but found '{jsonReader.GetString()}'");
            var errorSweep = ErrorSweep(ref jsonReader);
            if (errorSweep.IsFailure) return new Error("ErrorSweep", errorSweep.Error);
            // (The error messages are ignored.)

            // Read the "result" property.
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error("ReadingTokenError", nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("result"))
                return new Error("UnknownPropertyError",
                    $"Expected 'result' property but found '{jsonReader.GetString()}'");
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error("ReadingTokenError", nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error("UnexpectedTokenError",
                    $"Expected StartObject for 'result' but found {jsonReader.TokenType}");

            int last    = 0;
            var spreads = new Dictionary<string, SpreadData[]>();

            // Loop through properties inside "result".
            while (true)
            {
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure) return new Error("ReadingTokenError", nextResult.Error);
                if (jsonReader.TokenType == JsonTokenType.EndObject) break;
                if (jsonReader.TokenType != JsonTokenType.PropertyName)
                    return new Error("UnexpectedTokenError",
                        $"Expected property name in 'result' but found {jsonReader.TokenType}");
                string propName = jsonReader.GetString();
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure) return new Error("ReadingTokenError", nextResult.Error);
                if (propName == "last")
                {
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError",
                            $"Expected number for 'last' but found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out last))
                        return new Error("ParsingError", "Failed to parse 'last' as integer");
                }
                else
                {
                    // All other properties are assumed to be arrays of spread entries.
                    if (jsonReader.TokenType != JsonTokenType.StartArray)
                        return new Error("UnexpectedTokenError",
                            $"Expected StartArray for spread data under '{propName}' but found {jsonReader.TokenType}");
                    var spreadResult = ReadSpreadDataArray(ref jsonReader, propName);
                    if (spreadResult.IsFailure) return spreadResult.Error;
                    spreads.Add(propName, spreadResult.Value);
                }
            }

            // Read end of the root object.
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error("ReadingTokenError", nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.EndObject)
                return new Error("UnexpectedTokenError",
                    $"Expected EndObject for root but found {jsonReader.TokenType}");

            return new SpreadResponse { Last = last, Spreads = spreads };
        }
        catch (Exception ex)
        {
            return new Error("Exception", ex.Message);
        }
    }

    public override void Write(Utf8JsonWriter writer, Result<SpreadResponse> value, JsonSerializerOptions options)
    {
        // Serialization is not implemented in this example.
        throw new NotImplementedException();
    }

    // Reads an array of spread entries for a given asset pair.
    private Result<SpreadData[]> ReadSpreadDataArray(ref Utf8JsonReader jsonReader, string pairName)
    {
        var spreadList = new List<SpreadData>();
        while (true)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error("ReadingTokenError", nextResult.Error);
            if (jsonReader.TokenType == JsonTokenType.EndArray) break;
            if (jsonReader.TokenType != JsonTokenType.StartArray)
                return new Error("UnexpectedTokenError",
                    $"Expected StartArray for spread entry in '{pairName}', but found {jsonReader.TokenType}");
            var spreadResult = ReadSpreadData(ref jsonReader, pairName);
            if (spreadResult.IsFailure) return spreadResult.Error;
            spreadList.Add(spreadResult.Value);
        }

        return spreadList.ToArray();
    }

    // Reads a single spread entry array (expects exactly 3 elements: time, bid, ask).
    private Result<SpreadData> ReadSpreadData(ref Utf8JsonReader jsonReader, string pairName)
    {
        int    time = 0;
        string bid  = null;
        string ask  = null;
        for (int i = 0; i < 3; i++)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure) return new Error("ReadingTokenError", nextResult.Error);
            switch (i)
            {
                case 0:
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError",
                            $"Expected number for time in spread data '{pairName}', element {i}, found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out time))
                        return new Error("ParsingError", $"Failed to parse time for spread data '{pairName}'");
                    break;
                case 1:
                    if (jsonReader.TokenType == JsonTokenType.String)
                        bid = jsonReader.GetString();
                    else if (jsonReader.TokenType == JsonTokenType.Number)
                        bid = jsonReader.GetDecimal().ToString();
                    else
                        return new Error("UnexpectedTokenError",
                            $"Expected string or number for bid in spread data '{pairName}', element {i}, found {jsonReader.TokenType}");
                    break;
                case 2:
                    if (jsonReader.TokenType == JsonTokenType.String)
                        ask = jsonReader.GetString();
                    else if (jsonReader.TokenType == JsonTokenType.Number)
                        ask = jsonReader.GetDecimal().ToString();
                    else
                        return new Error("UnexpectedTokenError",
                            $"Expected string or number for ask in spread data '{pairName}', element {i}, found {jsonReader.TokenType}");
                    break;
            }
        }

        var finalResult = jsonReader.ReadNext();
        if (finalResult.IsFailure) return new Error("ReadingTokenError", finalResult.Error);
        if (jsonReader.TokenType != JsonTokenType.EndArray)
            return new Error("UnexpectedTokenError", $"Expected EndArray for spread data in '{pairName}'");
        return new SpreadData { Time = time, Bid = bid, Ask = ask };
    }

    // Helper: Reads the "error" array.
    private Result<string[]> ErrorSweep(ref Utf8JsonReader jsonReader)
    {
        var nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure) return new Error("StartReadingError", nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.StartArray)
            return new Error("UnexpectedTokenError",
                $"Expected StartArray for 'error' but found {jsonReader.TokenType}");
        var errors = new List<string>();
        while (true)
        {
            var r = jsonReader.ReadNext();
            if (r.IsFailure) return new Error("ReadingTokenError", r.Error);
            if (jsonReader.TokenType == JsonTokenType.EndArray) break;
            if (jsonReader.TokenType != JsonTokenType.String)
                return new Error("UnexpectedTokenError",
                    $"Expected string in error array but found {jsonReader.TokenType}");
            errors.Add(jsonReader.GetString());
        }

        return errors.ToArray();
    }
}
