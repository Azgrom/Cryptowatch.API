using Kraken.REST.API.Client.Types;
using NSubstitute;

namespace Kraken.REST.API.Client.Tests;

public class OrderBookHttpClientTests
{
    private const    int                Port               = 5000;
    private readonly KrakenApiMock      _krakenApiMock     = new();
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
        _krakenApiMock.SetupValidGetOrderBookEndpoint();

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
        var asksPriceAverage     = deserializedOrderBookResponse.Value.PairOrderBookSpan
            .SelectMany(x => x.BookAsks)
            .Average(x => x.Price);
        var asksVolumeAverage    = deserializedOrderBookResponse.Value.PairOrderBookSpan
            .SelectMany(x => x.BookAsks)
            .Average(x => x.Volume);
        var asksTimestampAverage = deserializedOrderBookResponse.Value.PairOrderBookSpan
            .SelectMany(x => x.BookAsks)
            .Average(x => (decimal)x.Timestamp);
        Assert.True(asksPriceAverage is 102336.95000m);
        Assert.True(asksVolumeAverage is 1.23643m);
        Assert.True(asksTimestampAverage is 1738365966.61m);
    }

    [Fact]
    public async Task AssertOrderBookWithInvalidErrorDeserialization()
    {
        _krakenApiMock.SetupGetOrderBookEndpointWithInvalidErrorObject();

        var orderBookAsync = async () => await new KrakenMarketData(_httpClientFactory, _token)
            .GetOrderBookAsync("XBTUSD");

        var exception = await Record.ExceptionAsync(orderBookAsync);
        Assert.Null(exception);
        var result = orderBookAsync.Invoke().Result;
        Assert.True(result.IsFailure);
        Assert.Contains("UnexpectedTokenError -> String == null a position 0\n", result.Error.Message, StringComparison.InvariantCultureIgnoreCase);
    }
}
