using NSubstitute;
using WireMock.Server;

namespace Kraken.REST.API.Client.Tests;

public class Tests
{
    WireMockServer     _server;
    CancellationToken _token = default;
    IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();

    [Fact]
    public async Task GetServerTimeShouldReturnExpectedValue()
    {
        using var mockServer = WireMockServer.Start();
        KrakenApiMock.SetupMappings(mockServer);

        var client = new HttpClient { BaseAddress = new Uri(mockServer.Url) };
        var api    = new KrakenMarketData(_httpClientFactory, _token);

        var response = await api.GetServerTimeAsync();

        // Assert.True(response.IsSuccessStatusCode);
        // Assert.Contains("unixtime", content);
    }
}