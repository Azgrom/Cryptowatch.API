using System.Text.Json;

namespace CoreAbstractions;

public static class JsonProcessExtensions
{
public static Result ReadNext(this ref Utf8JsonReader jsonReader)
{
    try
    {
        return jsonReader.Read();
    }
    catch (JsonException jsonException)
    {
        var readingJsonError = new Error(jsonException.StackTrace, jsonException.Message);
        return readingJsonError;
    }
}
}
