using System.Text.Json.Serialization;

namespace Kraken.REST.API;

public struct ServerTime
{
    [JsonConstructor]
    public ServerTime(int unixTime, string rfc1123)
    {
        UnixTime = unixTime;
        Rfc1123  = rfc1123;
    }

    [JsonPropertyName("unixtime")] public int    UnixTime { get; set; }
    [JsonPropertyName("rfc1123")]  public string Rfc1123  { get; set; }
}

public struct ServerTimeResponse
{
    [JsonConstructor]
    public ServerTimeResponse(Error[] error, ServerTime result)
    {
        Error  = error;
        Result = result;
    }

    [JsonPropertyName("error")]  public Error[]    Error  { get; set; }
    [JsonPropertyName("result")] public ServerTime Result { get; set; }
}
