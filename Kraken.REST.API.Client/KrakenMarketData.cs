using System.Web;

namespace Kraken.REST.API.Client;

public class KrakenMarketData
{
    private readonly HttpClient _httpClient;
    private const    string     BaseUrl = "https://api.kraken.com/0/public/";

    public KrakenMarketData(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    // GET: Server Time
    public Task<HttpResponseMessage> GetServerTimeAsync()
    {
        return _httpClient.GetAsync($"{BaseUrl}Time");
    }

    // GET: System Status
    public Task<HttpResponseMessage> GetSystemStatusAsync()
    {
        return _httpClient.GetAsync($"{BaseUrl}SystemStatus");
    }

    // GET: Asset Info (All combinations of optional parameters)
    public Task<HttpResponseMessage> GetAssetsAsync()
    {
        return _httpClient.GetAsync($"{BaseUrl}Assets");
    }

    public Task<HttpResponseMessage> GetAssetsAsync(string asset)
    {
        var builder = new UriBuilder($"{BaseUrl}Assets");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["asset"] = asset;
        builder.Query  = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    public Task<HttpResponseMessage> GetAssetsByClassAsync(string aclass)
    {
        var builder = new UriBuilder($"{BaseUrl}Assets");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["aclass"] = aclass;
        builder.Query   = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    public Task<HttpResponseMessage> GetAssetsAsync(string asset, string aclass)
    {
        var builder = new UriBuilder($"{BaseUrl}Assets");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["asset"]  = asset;
        query["aclass"] = aclass;
        builder.Query   = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    // GET: Tradable Asset Pairs (all combinations of query parameters pair, info, country_code)
    public Task<HttpResponseMessage> GetTradableAssetPairsAsync()
    {
        return _httpClient.GetAsync($"{BaseUrl}AssetPairs");
    }

    public Task<HttpResponseMessage> GetTradableAssetPairsAsync(string pair)
    {
        var builder = new UriBuilder($"{BaseUrl}AssetPairs");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["pair"] = pair;
        builder.Query = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    public Task<HttpResponseMessage> GetTradableAssetPairsByInfoAsync(string info)
    {
        var builder = new UriBuilder($"{BaseUrl}AssetPairs");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["info"] = info; // Possible values: info, leverage, fees, margin
        builder.Query = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    public Task<HttpResponseMessage> GetTradableAssetPairsByCountryAsync(string countryCode)
    {
        var builder = new UriBuilder($"{BaseUrl}AssetPairs");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["country_code"] = countryCode;
        builder.Query         = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    public Task<HttpResponseMessage> GetTradableAssetPairsAsync(string pair, string info)
    {
        var builder = new UriBuilder($"{BaseUrl}AssetPairs");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["pair"] = pair;
        query["info"] = info;
        builder.Query = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    public Task<HttpResponseMessage> GetTradableAssetPairsWithCountryAsync(string pair, string countryCode)
    {
        var builder = new UriBuilder($"{BaseUrl}AssetPairs");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["pair"]         = pair;
        query["country_code"] = countryCode;
        builder.Query         = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    public Task<HttpResponseMessage> GetTradableAssetPairsByInfoAndCountryAsync(string info, string countryCode)
    {
        var builder = new UriBuilder($"{BaseUrl}AssetPairs");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["info"]         = info;
        query["country_code"] = countryCode;
        builder.Query         = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }

    public Task<HttpResponseMessage> GetTradableAssetPairsAsync(string pair, string info, string countryCode)
    {
        var builder = new UriBuilder($"{BaseUrl}AssetPairs");
        var query   = HttpUtility.ParseQueryString(string.Empty);

        query["pair"]         = pair;
        query["info"]         = info;
        query["country_code"] = countryCode;
        builder.Query         = query.ToString();

        return _httpClient.GetAsync(builder.Uri);
    }
}