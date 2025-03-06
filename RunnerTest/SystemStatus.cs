using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace RunnerTest;

// Define the type that will hold the system status.
public sealed record SystemStatus
{
    public SystemStatus(string status, string timestamp)
    {
        Status = status;
        Timestamp = timestamp;
    }
    public string Status { get; init; }
    public string Timestamp { get; init; }
}

// Custom converter for deserializing the JSON into a Result<SystemStatus>
public class SystemStatusJsonConverter : JsonConverter<Result<SystemStatus>>
{
    // Allowed status values per schema.
    private static readonly string[] AllowedStatuses = new[] { "online", "maintenance", "cancel_only", "post_only" };

    public override Result<SystemStatus> Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            // Begin reading the root object.
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("StartReadingError", nextResult.Error);

            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error("UnexpectedTokenError", $"Expected StartObject but found {jsonReader.TokenType}");

            // Read the first property, expected to be "error"
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            string[] errors = Array.Empty<string>();
            if (jsonReader.TokenType == JsonTokenType.PropertyName && jsonReader.ValueTextEquals("error"))
            {
                var errorSweepResult = ErrorSweep(ref jsonReader);
                if (errorSweepResult.IsFailure)
                    return new Error("ErrorSweep", errorSweepResult.Error);
                errors = errorSweepResult.Value;
            }
            else
            {
                return new Error("UnknownPropertyError",
                    $"Expected 'error' property but found '{jsonReader.GetString()}'");
            }

            // Read the next property, expected to be "result".
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            if (jsonReader.TokenType != JsonTokenType.PropertyName ||
                !jsonReader.ValueTextEquals("result"))
            {
                return new Error("UnknownPropertyError",
                    $"Expected 'result' property but found '{jsonReader.GetString()}'");
            }

            // Read the value of "result"
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error("UnexpectedTokenError",
                    $"Expected StartObject for 'result' but found {jsonReader.TokenType}");

            // Initialize variables for the expected properties.
            string status = string.Empty;
            string timestamp = string.Empty;
            bool foundStatus = false;
            bool foundTimestamp = false;

            // Read properties inside the "result" object.
            while (true)
            {
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure)
                    return new Error("ReadingTokenError", nextResult.Error);

                // End of "result" object.
                if (jsonReader.TokenType == JsonTokenType.EndObject)
                    break;

                if (jsonReader.TokenType != JsonTokenType.PropertyName)
                    return new Error("UnexpectedTokenError",
                        $"Expected property name but found {jsonReader.TokenType}");

                string propertyName = jsonReader.GetString();
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure)
                    return new Error("ReadingTokenError", nextResult.Error);

                if (propertyName == "status")
                {
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError",
                            $"Expected string for 'status' but found {jsonReader.TokenType}");
                    status = jsonReader.GetString();
                    foundStatus = true;
                }
                else if (propertyName == "timestamp")
                {
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError",
                            $"Expected string for 'timestamp' but found {jsonReader.TokenType}");
                    timestamp = jsonReader.GetString();
                    foundTimestamp = true;
                }
                else
                {
                    // Skip any unknown property.
                    jsonReader.Skip();
                }
            }

            // Ensure both properties were found.
            if (!foundStatus)
                return new Error("MissingPropertyError", "Missing property 'status' in result");
            if (!foundTimestamp)
                return new Error("MissingPropertyError", "Missing property 'timestamp' in result");

            // Validate the "status" value.
            if (Array.IndexOf(AllowedStatuses, status) < 0)
                return new Error("InvalidValueError", $"Invalid status value: {status}");

            // Validate that the timestamp string can be parsed (RFC3339 is a subset of ISO8601).
            if (!DateTime.TryParse(timestamp, out _))
                return new Error("InvalidValueError", $"Invalid timestamp format: {timestamp}");

            // Read the end of the root object.
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.EndObject)
                return new Error("UnexpectedTokenError",
                    $"Expected EndObject for root but found {jsonReader.TokenType}");

            // Return the successfully parsed SystemStatus.
            var systemStatus = new SystemStatus(status, timestamp);
            return systemStatus; // Uses implicit conversion operator for a success.
        }
        catch (Exception ex)
        {
            // Any exception is caught and returned as an Error.
            return new Error("Exception", ex.Message);
        }
    }

    public override void Write(Utf8JsonWriter writer, Result<SystemStatus> value, JsonSerializerOptions options)
    {
        // Basic serialization implementation.
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
            writer.WriteString("status", value.Value.Status);
            writer.WriteString("timestamp", value.Value.Timestamp);
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
            return new Error("UnexpectedTokenError",
                $"Expected StartArray for 'error' but found {jsonReader.TokenType}");

        var errors = new List<string>();
        while (true)
        {
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            if (jsonReader.TokenType == JsonTokenType.EndArray)
                break;

            if (jsonReader.TokenType == JsonTokenType.String)
                errors.Add(jsonReader.GetString());
            else
                return new Error("UnexpectedTokenError",
                    $"Expected string in error array but found {jsonReader.TokenType}");
        }
        return errors.ToArray();
    }
}

