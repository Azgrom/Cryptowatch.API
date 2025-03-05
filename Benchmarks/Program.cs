// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using Benchmarks;

internal class Program
{
    public static void Main(string[] args)
    {
        var jsonConvertersBenchmarks = new X();
        for (int i = 0; i < 100_000_000; i++)
        {
            jsonConvertersBenchmarks.R();
        }

        Console.WriteLine("finished");
        // var summary = BenchmarkRunner.Run<JsonConvertersBenchmarks>();
    }

    public static  void Run()
    {
        var jsonConvertersBenchmarks = new X();
        for (int i = 0; i < 100_000_000; i++)
        {
            jsonConvertersBenchmarks.R();
        }

        Console.WriteLine("finished");
    }
}