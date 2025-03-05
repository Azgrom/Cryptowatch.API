using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using JsonConverterTests;
using RunnerTest;

namespace Benchmarks;

[DisassemblyDiagnoser]
[ExceptionDiagnoser]
[MemoryDiagnoser]
[ThreadingDiagnoser]
public class JsonConvertersBenchmarks
{
    internal static string x = RestResponses.ValidAssetPairResponse;

    [Benchmark]
    public void T()
    {
        var _ = Encoding.UTF8.GetBytes(x);
    }

    [Benchmark]
    public void R()
    {
        // var utf8JsonReader = new Utf8JsonReader(y);
        //
        // var _ = Ohlc.FromJson(ref utf8JsonReader);
    }
}
