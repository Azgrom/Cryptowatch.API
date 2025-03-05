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
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
        };
        var ohlcJsonConverter     = new OrderBookJsonConverter();
        jsonSerializerOptions.Converters.Add(ohlcJsonConverter);
        for (int i = 0; i < 100_000_000; i++)
        {
            var deserialize = JsonSerializer.Deserialize<Result<OrderBook>>(RestObjects.ValidAssetPairResponse, jsonSerializerOptions);
        }

        Console.WriteLine("finished");
    }
}
