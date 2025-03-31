using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using CoreAbstractions;
using Kraken.REST.API.Client.Tests;
using Kraken.REST.API.Client.Types;

namespace Benchmarks;

[DisassemblyDiagnoser]
[ExceptionDiagnoser]
[MemoryDiagnoser]
[ThreadingDiagnoser]
public class JsonConvertersBenchmarks
{
    private static readonly byte[] OrderBookResponseBytes
        = Encoding.UTF8.GetBytes(RestResponses.ValidOrderBookResponse);

    private static readonly OrderBookJsonConverter OrderBookJsonConverter = new();

    private static readonly JsonSerializerOptions _options = new()
    {
        Converters     = { new OrderBookJsonConverter() },
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    [Benchmark]
    public void DeserializeOrderBook()
    {
        var allowNamedFloatingPointLiterals = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals;
        JsonSerializer.Deserialize<Result<OrderBook>>(RestResponses.ValidOrderBookResponse, _options);
    }
}
