using BenchmarkDotNet.Running;

namespace Benchmarks;

internal static class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<JsonConvertersBenchmarks>();
        //var summary = BenchmarkRunner.Run<KrakenApiBenchmarks>();
    }
}