using System.Text.Json.Serialization;

namespace Kraken.REST.API.Types;

public readonly struct Route
{
    [JsonConstructor]
    public Route(string markets) => Markets = markets;

    [JsonPropertyName("markets")] public string Markets { get; }
}
