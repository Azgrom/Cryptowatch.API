// ReSharper disable ConvertToPrimaryConstructor

namespace Kraken.REST.API.Client;

public class KrakenMarketData
{
    private readonly HttpClient _httpClient;

    // Paths defined as constants
    private const string ServerTimePath   = "/0/public/Time";
    private const string SystemStatusPath = "/0/public/SystemStatus";
    private const string AssetsPath       = "/0/public/Assets";
    private const string AssetPairsPath   = "/0/public/AssetPairs";

    public KrakenMarketData(HttpClient httpClient) => _httpClient = httpClient;

    public Task<HttpResponseMessage> GetServerTimeAsync() => _httpClient.GetAsync(ServerTimePath);

    public Task<HttpResponseMessage> GetSystemStatusAsync() => _httpClient.GetAsync(SystemStatusPath);

    public Task<HttpResponseMessage> GetAssetsAsync() => _httpClient.GetAsync(AssetsPath);

    public Task<HttpResponseMessage> GetAssetsAsync(string asset) =>
        _httpClient.GetAsync($"{AssetsPath}?asset={asset}");

    public Task<HttpResponseMessage> GetAssetsByClassAsync(string aclass) =>
        _httpClient.GetAsync($"{AssetsPath}?aclass={aclass}");

    public Task<HttpResponseMessage> GetAssetsAsync(string asset, string aclass) =>
        _httpClient.GetAsync($"{AssetsPath}?asset={asset}&aclass={aclass}");

    public Task<HttpResponseMessage> GetTradableAssetPairsAsync() => _httpClient.GetAsync(AssetPairsPath);

    public Task<HttpResponseMessage> GetTradableAssetPairsAsync(string pair) =>
        _httpClient.GetAsync($"{AssetPairsPath}?pair={pair}");

    public Task<HttpResponseMessage> GetTradableAssetPairsByInfoAsync(string info) =>
        _httpClient.GetAsync($"{AssetPairsPath}?info={info}");

    public Task<HttpResponseMessage> GetTradableAssetPairsByCountryAsync(string countryCode) =>
        _httpClient.GetAsync($"{AssetPairsPath}?country={countryCode}");

    public Task<HttpResponseMessage> GetTradableAssetPairsAsync(string pair, string info) =>
        _httpClient.GetAsync($"{AssetPairsPath}?pair={pair}&info={info}");

    public Task<HttpResponseMessage> GetTradableAssetPairsWithCountryAsync(string pair, string countryCode) =>
        _httpClient.GetAsync($"{AssetPairsPath}?pair={pair}&country={countryCode}");

    public Task<HttpResponseMessage> GetTradableAssetPairsByInfoAndCountryAsync(string info, string countryCode) =>
        _httpClient.GetAsync($"{AssetPairsPath}?info={info}&country={countryCode}");

    public Task<HttpResponseMessage> GetTradableAssetPairsAsync(string pair, string info, string countryCode) =>
        _httpClient.GetAsync($"{AssetPairsPath}?pair={pair}&info={info}&country={countryCode}");
}
