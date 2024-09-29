using System.Text.Json.Serialization;

namespace CryptoWatch.REST.API;

public struct SystemStatusResponse
{
    [JsonConstructor]
    public SystemStatusResponse(Error[] error, SystemStatus result)
    {
        Error = error;
        Result = result;
    }

    [JsonPropertyName("error")]
    public Error[] Error { get; set; }

    [JsonPropertyName("result")]
    public SystemStatus Result { get; set; }
}

public struct SystemStatus
{
    [JsonConstructor]
    public SystemStatus(string status, string timestamp)
    {
        Status = status;
        Timestamp = timestamp;
    }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }
}
