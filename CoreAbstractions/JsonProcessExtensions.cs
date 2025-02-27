using System.Text.Json;

namespace CoreAbstractions;

public static class JsonProcessExtensions
{
public static Result ReadNext(this ref Utf8JsonReader jsonReader)
{
    try
    {
        _ = jsonReader.Read();
        return Result.Success();
    }
    catch (JsonException jsonException)
    {
        var readingJsonError = new Error(jsonException.StackTrace, jsonException.Message);
        return Result.Failure<bool>(readingJsonError);
    }
}
}
