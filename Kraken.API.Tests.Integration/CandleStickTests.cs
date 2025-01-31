using System.Text.Json;
using Kraken.REST.API;
using Xunit;

namespace Kraken.API.Tests.Integration;

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
