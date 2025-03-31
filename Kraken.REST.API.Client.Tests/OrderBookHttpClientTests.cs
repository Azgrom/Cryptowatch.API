using NSubstitute;

namespace Kraken.REST.API.Client.Tests;

public class OrderBookHttpClientTests
{
    private const    int                Port               = 5000;
    private readonly KrakenApiMock      _krakenApiMock     = new(Port);
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly CancellationToken  _token             = CancellationToken.None;

    public OrderBookHttpClientTests()
    {
        _httpClientFactory.CreateClient(nameof(KrakenMarketData))
            .Returns(new HttpClient()
            {
                BaseAddress = new Uri(_krakenApiMock.Url)
            });
    }

    [Fact]
    public async Task TestNormalRequest()
    {
        _krakenApiMock.SetupGetOrderBook();

        var deserializedOrderBookResponse = await new KrakenMarketData(_httpClientFactory, _token)
            .GetOrderBookAsync("XBTUSD");

        Assert.False(deserializedOrderBookResponse.IsFailure);
        Assert.True(deserializedOrderBookResponse.Value.PairOrderBookSpan.Count == 1);
        var pairOrderBookEntries = deserializedOrderBookResponse.Value.PairOrderBookSpan.First();
        Assert.True(pairOrderBookEntries.OrderBookName is "XXBTZUSD");
        var bookAsksCount = pairOrderBookEntries.BookAsks.Count;
        Assert.True(bookAsksCount == 100, $"expected 100 asks, got: {bookAsksCount}");
        var bookBidsCount = pairOrderBookEntries.BookBids.Count;
        Assert.True(bookBidsCount == 100, $"expected 100 bids, got: {bookBidsCount}");
        Console.WriteLine();
    }
}
