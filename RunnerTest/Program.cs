using System.Text;
using System.Text.Json;
using CoreAbstractions;
using RunnerTest;

internal class Program
{
    private static readonly byte[] ValidAssetPairResponse;

    static Program() { ValidAssetPairResponse = Encoding.UTF8.GetBytes(RestObjects.ValidAssetPairResponse); }

    public static void Main(string[] args)
    {
        // var ohlcs             = new List<Ohlc>(10_000);
        var ohlcJsonConverter = new OhlcJsonConverter();
        for (int i = 0; i < 100_000_000; i++)
        {
            var utf8JsonReader = new Utf8JsonReader(ValidAssetPairResponse);
            var value          = ohlcJsonConverter.IntoOhlc(ref utf8JsonReader).Value;
            // ohlcs.Add(value);
            //
            // if (ohlcs.Count is 10_000)
            //     ohlcs.Clear();
        }

        Console.WriteLine("finished");
    }
}
