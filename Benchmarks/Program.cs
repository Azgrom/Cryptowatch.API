// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using Benchmarks;

internal class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<JsonConvertersBenchmarks>();
    }
}