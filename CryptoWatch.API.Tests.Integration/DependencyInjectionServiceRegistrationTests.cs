using CryptoWatch.REST.API;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CryptoWatch.API.Tests.Integration;

public class DependencyInjectionServiceRegistrationTests
{
    [Fact]
    public void Assert_RestApiHttpClient_Registration()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddCryptoWatchHttpClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var cryptoWatchRestApi = () => serviceProvider.GetRequiredService<CryptoWatchRestApi>();
        cryptoWatchRestApi.Should()
            .NotThrow()
            .And.Subject.Invoke()
            .Should()
            .BeOfType<CryptoWatchRestApi>();
    }
}
