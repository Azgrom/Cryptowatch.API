using System.Text.Json.Serialization;

namespace CryptoWatch.REST.API;

public class SystemStatusResponse
{
    [JsonPropertyName("error")]
    public Error[] error { get; set; }

    [JsonPropertyName("result")]
    public SystemStatus result { get; set; }
}

public class SystemStatus
{
    [JsonPropertyName("status")]
    public string status { get; set; }

    [JsonPropertyName("timestamp")]
    public string timestamp { get; set; }
}
