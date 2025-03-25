using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using JsonConverterTests;
using Kraken.REST.API.Client;
using Kraken.REST.API.Client.Sandbox.Types;

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

    [Benchmark]
    public void DeserializeOrderBook()
    {
        var utf8JsonReader = new Utf8JsonReader(OrderBookResponseBytes);
        OrderBookJsonConverter.IntoOhlc(ref utf8JsonReader);
    }
}
