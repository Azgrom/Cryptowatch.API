using Moq;
using Xunit;

namespace CryptoWatch.API.Tests.Integration;

public class AssetsTests : IAsyncLifetime
{
    private readonly CryptoWatchServerApi _cryptoWatchServer = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();

    public AssetsTests()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(_cryptoWatchServer.Url)
        };

        _httpClientFactory.Setup(x => x.CreateClient(string.Empty))
            .Returns(httpClient);
    }

    public Task InitializeAsync()
    {
        _cryptoWatchServer.SetupAssetsApi();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _cryptoWatchServer.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task Asserts_CryptoWatchApiAssetsListing_HttpResponse()
    {
        var list = await new CryptoWatchApi(_httpClientFactory.Object).Assets.ListAsyncTask();

        Console.WriteLine();
    }
}