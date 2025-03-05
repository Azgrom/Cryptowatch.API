using System.Text.Json;

namespace CoreAbstractions;

public static class JsonProcessExtensions
{
public static Result<bool> ReadNext(this ref Utf8JsonReader jsonReader)
{
    try
    {
        _ = jsonReader.Read();
        return true;
    }
    catch (JsonException jsonException)
    {
        var readingJsonError = new Error(jsonException.StackTrace, jsonException.Message);
        return readingJsonError;
    }
}
}
