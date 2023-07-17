using System.Text.Json.Serialization;

namespace CryptoWatch.API.Types;

public struct Exchanges
{
    [JsonPropertyName("result")] public List<Collection> Result { get; set; }
    [JsonPropertyName("cursor")] public Cursor Cursor { get; set; }
    [JsonPropertyName("allowance")] public Allowance Allowance { get; set; }

    public struct Collection
    {
        [JsonPropertyName("symbol")] public string Symbol { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("route")] public string Route { get; set; }
        [JsonPropertyName("active")] public bool Active { get; set; }
    }
}