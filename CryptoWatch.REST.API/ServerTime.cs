using System.Text.Json.Serialization;

namespace CryptoWatch.REST.API;

public class ServerTimeResponse
{
    [JsonPropertyName("error")]
    public Error[] error { get; set; }

    [JsonPropertyName("result")]
    public ServerTime result { get; set; }
}

public class ServerTime
{
    [JsonPropertyName("unixtime")]
    public int unixtime { get; set; }

    [JsonPropertyName("rfc1123")]
    public string rfc1123 { get; set; }
}
