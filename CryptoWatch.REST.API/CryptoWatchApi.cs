using CryptoWatch.REST.API.Paths;

namespace CryptoWatch.REST.API;

public class CryptoWatchRestApi
{
    public const string RootUrl = "https://api.cryptowat.ch";
    private readonly HttpClient _httpClient;

    public CryptoWatchRestApi(HttpClient httpClient) =>
        _httpClient = httpClient;

    public AssetsApi Assets => new(_httpClient);
    public PairsApi Pairs => new(_httpClient);
    public MarketsApi Markets => new(_httpClient);
    public ExchangesApi Exchanges => new(_httpClient);
}

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

public class Error
{
}
