using NSubstitute;

namespace Kraken.REST.API.Client.Tests;

public class OrderBookHttpClientTests
{
    private const    int                Port               = 5000;
    private readonly KrakenApiMock      _krakenApiMock     = new(Port);
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly CancellationToken  _token             = default;

    public OrderBookHttpClientTests()
    {
        _httpClientFactory.CreateClient(nameof(KrakenMarketData))
            .Returns(new HttpClient()
            {
                BaseAddress = new Uri(_krakenApiMock.Url)
            });
    }

    public void TestNormalRequest()
    {
        _krakenApiMock.SetupGetOrderBook();

        var krakenMarketData = new KrakenMarketData(_httpClientFactory, _token);
    }
}
