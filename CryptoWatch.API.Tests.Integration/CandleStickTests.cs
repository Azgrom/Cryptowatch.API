using System.Text.Json;
using CryptoWatch.REST.API;
using Xunit;

namespace CryptoWatch.API.Tests.Integration;

public class CandleStickTests
{
   [Fact]
   public void GetCandleSticks_WhenCalled_ShouldReturnCandleSticks()
   {
      var candleStickResponse = MarketDataRestAPI.CandleStickResponse;

      var stickResponse = JsonSerializer.Deserialize<CandleStickResponse>(candleStickResponse);

      Console.WriteLine();
   }
}
