using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace Kraken.REST.API.Client.Types;

public sealed record ServerTimeResponse
{
    public ServerTimeResponse(long unixtime, string rfc1123)
    {
        Unixtime = unixtime;
        Rfc1123  = rfc1123;
    }
    public long   Unixtime { get; init; }
    public string Rfc1123  { get; init; }
}

// Custom converter for deserializing the JSON into a Result<TimeInfo>
public sealed class ServerTimeInfoJsonConverter : JsonConverter<Result<ServerTimeResponse>>
{
    public override Result<ServerTimeResponse> Read(
        ref Utf8JsonReader    jsonReader, 
        Type                  typeToConvert, 
        JsonSerializerOptions options)
    {
        try
        {
            // Begin reading the outer object.
            if (jsonReader.TokenType is JsonTokenType.None)
            {
                if (jsonReader.ReadNext().IsFailure)
                    return new Error("StartReadingError", jsonReader.ReadNext().Error);
            }

            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartObject but found {jsonReader.TokenType}");

            // Read first property – should be "error".
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

            string[] errors = Array.Empty<string>();

            if (jsonReader.TokenType == JsonTokenType.PropertyName &&
                jsonReader.ValueTextEquals("error"))
            {
                var errorSweepResult = ErrorSweep(ref jsonReader);
                if (errorSweepResult.IsFailure)
                    return new Error("ErrorSweep", errorSweepResult.Error);
                errors = errorSweepResult.Value;
            }
            else
            {
                return new Error(ErrorCodes.UnknownPropertyErrorCode,
                    $"Expected 'error' property but found '{jsonReader.GetString()}'");
            }

            // Read next property – should be "result".
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

            if (jsonReader.TokenType != JsonTokenType.PropertyName ||
                !jsonReader.ValueTextEquals("result"))
            {
                return new Error(ErrorCodes.UnknownPropertyErrorCode,
                    $"Expected 'result' property but found '{jsonReader.GetString()}'");
            }

            // Read the value of "result"
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                    $"Expected StartObject for 'result' but found {jsonReader.TokenType}");

            // Prepare to read properties from the result object.
            long   unixtime      = 0;
            string rfc1123       = string.Empty;
            bool   foundUnixtime = false;
            bool   foundRfc1123  = false;

            // Loop through the result properties.
            while (true)
            {
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure)
                    return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

                // End of the "result" object.
                if (jsonReader.TokenType == JsonTokenType.EndObject)
                    break;

                if (jsonReader.TokenType != JsonTokenType.PropertyName)
                    return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                        $"Expected a property name but found {jsonReader.TokenType}");

                string propName = jsonReader.GetString();
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure)
                    return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

                if (propName == "unixtime")
                {
                    if (jsonReader.TokenType == JsonTokenType.Number)
                    {
                        if (jsonReader.TryGetInt64(out long value))
                        {
                            unixtime      = value;
                            foundUnixtime = true;
                        }
                        else
                        {
                            return new Error("ParsingError", "Failed to parse unixtime as Int64");
                        }
                    }
                    else if (jsonReader.TokenType == JsonTokenType.String)
                    {
                        // Allow string representation of the number.
                        var str = jsonReader.GetString();
                        if (long.TryParse((string?)str, out long value))
                        {
                            unixtime      = value;
                            foundUnixtime = true;
                        }
                        else
                        {
                            return new Error("ParsingError", "Failed to parse unixtime string to Int64");
                        }
                    }
                    else
                    {
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                            $"Expected Number or String for unixtime but found {jsonReader.TokenType}");
                    }
                }
                else if (propName == "rfc1123")
                {
                    if (jsonReader.TokenType == JsonTokenType.String)
                    {
                        rfc1123      = jsonReader.GetString();
                        foundRfc1123 = true;
                    }
                    else
                    {
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                            $"Expected String for rfc1123 but found {jsonReader.TokenType}");
                    }
                }
                else
                {
                    // For any unknown property, skip its value.
                    jsonReader.SkipToEnd();
                }
            }

            if (!foundUnixtime || !foundRfc1123)
                return new Error(ErrorCodes.MissingPropertyErrorCode,
                    "Result object must contain both 'unixtime' and 'rfc1123'");

            // Read the end of the root object.
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.EndObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                    $"Expected EndObject for root but found {jsonReader.TokenType}");

            var timeInfo = new ServerTimeResponse(unixtime, rfc1123);
            return timeInfo; // Uses implicit conversion operator for a success.
        }
        catch (Exception ex)
        {
            // Any exception is caught and returned as an Error.
            return new Error("Exception", ex.Message);
        }
    }

    public override void Write(
        Utf8JsonWriter        writer, 
        Result<ServerTimeResponse>    value,
        JsonSerializerOptions options)
    {
        // For demonstration we provide a basic implementation.
        // Serialization should also use error propagation and not throw.
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
            writer.WriteNumber("unixtime", value.Value.Unixtime);
            writer.WriteString("rfc1123", value.Value.Rfc1123);
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads the "error" array from the JSON.
    /// </summary>
    private Result<string[]> ErrorSweep(ref Utf8JsonReader jsonReader)
    {
        var nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error("StartReadingError", nextResult.Error);

        if (jsonReader.TokenType != JsonTokenType.StartArray)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                $"Expected StartArray for 'error' but found {jsonReader.TokenType}");

        var errors = new List<string>();
        while (true)
        {
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

            if (jsonReader.TokenType == JsonTokenType.EndArray)
                break;

            if (jsonReader.TokenType == JsonTokenType.String)
                errors.Add(jsonReader.GetString());
            else
                return new Error(ErrorCodes.UnexpectedTokenErrorCode,
                    $"Expected String in error array but found {jsonReader.TokenType}");
        }
        return errors.ToArray();
    }
}