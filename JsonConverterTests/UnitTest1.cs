using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RunnerTest;

namespace JsonConverterTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var validAssetPairResponse = Encoding.UTF8.GetBytes(RestResponses.ValidAssetPairResponse);
        var utf8JsonReader         = new Utf8JsonReader(validAssetPairResponse);

        var ohlcJsonConverter      = Ohlc.FromJson(ref utf8JsonReader);

        Console.WriteLine(ohlcJsonConverter);
    }
}
