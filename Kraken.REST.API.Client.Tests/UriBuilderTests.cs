using System.Web;

namespace Kraken.REST.API.Client.Tests;

public class UriBuilderTests
{
    private readonly Uri _baseUri = new Uri("https://api.kraken.com/0/public/");

    private Uri BuildUri(string relativePath, params (string Key, string Value)[] queryParams)
    {
        var encodedPath = HttpUtility.UrlPathEncode(relativePath);
        var builder = new UriBuilder(new Uri(_baseUri, encodedPath));

        var query = HttpUtility.ParseQueryString(string.Empty);
        foreach (var (key, value) in queryParams)
        {
            if (!string.IsNullOrEmpty(value))
                query[key] = value;
        }

        builder.Query = query.ToString();
        return builder.Uri;
    }

    [Fact]
    public void UriBuilder_ShouldCreate_CorrectUriWhenNoQueryParams()
    {
        // Act
        var uri = BuildUri("Time");

        // Assert
        Assert.Equal("https://api.kraken.com/0/public/Time", uri.ToString());
    }

    [Fact]
    public void UriBuilder_ShouldAppend_SingleQueryParamCorrectly()
    {
        // Act
        var uri = BuildUri("Assets", ("asset", "BTC"));

        // Assert
        Assert.Equal("https://api.kraken.com/0/public/Assets?asset=BTC", uri.ToString());
    }

    [Fact]
    public void UriBuilder_ShouldAppend_MultipleQueryParamsCorrectly()
    {
        // Act
        var uri = BuildUri("Assets", ("asset", "BTC"), ("aclass", "crypto"));

        // Assert
        Assert.Equal("https://api.kraken.com/0/public/Assets?asset=BTC&aclass=crypto", uri.ToString());
    }

    [Fact]
    public void UriBuilder_ShouldUrlEncode_QueryParamValues()
    {
        // Act
        var uri = BuildUri("Assets", ("asset", "BTC/USD"));

        // Assert
        Assert.Equal("https://api.kraken.com/0/public/Assets?asset=BTC%2fUSD", uri.ToString(), ignoreCase: true);
    }

    [Theory]
    [InlineData("BTCUSD", "fees", "US",
        "https://api.kraken.com/0/public/AssetPairs?pair=BTCUSD&info=fees&country_code=US")]
    [InlineData("ETHUSD", "", "",
        "https://api.kraken.com/0/public/AssetPairs?pair=ETHUSD")]
    [InlineData("", "info", "",
        "https://api.kraken.com/0/public/AssetPairs?info=info")]
    [InlineData("", "", "CA",
        "https://api.kraken.com/0/public/AssetPairs?country_code=CA")]
    public void UriBuilder_ShouldGenerate_CorrectUri_WithVariousParameters(
        string pair, string info, string countryCode, string expectedUri)
    {
        // Act
        var uri = BuildUri("AssetPairs",
            ("pair", pair),
            ("info", info),
            ("country_code", countryCode));

        // Assert
        Assert.Equal(expectedUri, uri.ToString(), ignoreCase: true);
    }
}
