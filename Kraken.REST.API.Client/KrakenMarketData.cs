// ReSharper disable ConvertToPrimaryConstructor

using System.Net.Http.Json;
using System.Text.Json;
using CoreAbstractions;
using Kraken.REST.API.Client.Types;

namespace Kraken.REST.API.Client;

public class KrakenMarketData
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CancellationToken  _token;

    private static readonly JsonSerializerOptions Options = new()
    {
        Converters =
        {
            new AssetInfoJsonConverter(),
            new AssetPairJsonConverter(),
            new AssetTickerJsonConverter(),
            new OhlcJsonConverter(),
            new OrderBookJsonConverter(),
            new SpreadJsonConverter(),
            new RecentTradesJsonConverter(),
            new ServerTimeInfoJsonConverter(),
            new SystemStatusJsonConverter()
        }
    };

    // Paths defined as constants
    private const  string PublicPathV0                           = "/0/public";
    internal const string ServerTimePath                         = $"{PublicPathV0}/Time";
    internal const string SystemStatusPath                       = $"{PublicPathV0}/SystemStatus";
    internal const string AssetsPath                             = $"{PublicPathV0}/Assets";
    internal const string AssetPairsPath                         = $"{PublicPathV0}/AssetPairs";
    internal const string AssetTickerPath                        = $"{PublicPathV0}/Ticker";
    internal const string OhlcPath                               = $"{PublicPathV0}/OHLC";
    internal const string OrderBookPath                          = $"{PublicPathV0}/Depth";
    internal const string RecentTradesPath                       = $"{PublicPathV0}/Trades";
    internal const string SpreadPath                             = $"{PublicPathV0}/Spread";
    private const  int    SmallestNumberOfAsksAndBids            = 1;
    private const  int    GreatestNumberOfAsksAndBids            = 500;
    private const  int    GreatestAmountOfRecentTradesPerRequest = 1000;
    private const  int    SmallestAmountOfRecentTradesPerRequest = 1;

    public KrakenMarketData(IHttpClientFactory httpClientFactory, CancellationToken token = default)
    {
        _httpClientFactory = httpClientFactory;
        _token             = token;
    }

    public Task<Result<ServerTime>> GetServerTimeAsync() =>
        _httpClientFactory.GetFromJsonAsync<ServerTime>(
            nameof(KrakenMarketData),
            ServerTimePath,
            Options,
            _token
        );

    public Task<Result<SystemStatus>> GetSystemStatusAsync() =>
        _httpClientFactory
            .GetFromJsonAsync<SystemStatus>(
                nameof(KrakenMarketData),
                SystemStatusPath,
                Options,
                _token
            );

    public Task<Result<AssetInfoResponse>> GetAssetsAsync() =>
        _httpClientFactory
            .GetFromJsonAsync<AssetInfoResponse>(
                nameof(KrakenMarketData),
                AssetsPath,
                Options,
                _token
            );

    public Task<Result<AssetInfoResponse>> GetAssetsAsync(string asset) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetInfoResponse>(
                nameof(KrakenMarketData),
                $"{AssetsPath}?asset={asset}",
                Options,
                _token
            );

    public Task<Result<AssetInfoResponse>> GetAssetsByClassAsync(string aclass) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetInfoResponse>(
                nameof(KrakenMarketData),
                $"{AssetsPath}?aclass={aclass}",
                Options,
                _token
            );

    public Task<Result<AssetInfoResponse>> GetAssetsAsync(string asset, string aclass) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetInfoResponse>(
                nameof(KrakenMarketData),
                $"{AssetsPath}?asset={asset}&aclass={aclass}",
                Options,
                _token
            );

    public Task<Result<AssetPair>> GetTradableAssetPairsAsync() =>
        _httpClientFactory
            .GetFromJsonAsync<AssetPair>(
                nameof(KrakenMarketData),
                AssetPairsPath,
                Options,
                _token
            );

    public Task<Result<AssetPair>> GetTradableAssetPairsAsync(string pair) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetPair>(
                nameof(KrakenMarketData),
                $"{AssetPairsPath}?pair={pair}",
                Options,
                _token
            );

    public Task<Result<AssetPair>> GetTradableAssetPairsByInfoAsync(string info) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetPair>(
                nameof(KrakenMarketData),
                $"{AssetPairsPath}?info={info}",
                Options,
                _token
            );

    public Task<Result<AssetPair>> GetTradableAssetPairsByCountryAsync(string countryCode) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetPair>(
                nameof(KrakenMarketData),
                $"{AssetPairsPath}?country={countryCode}",
                Options,
                _token
            );

    public Task<Result<AssetPair>> GetTradableAssetPairsAsync(string pair, string info) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetPair>(
                nameof(KrakenMarketData),
                $"{AssetPairsPath}?pair={pair}&info={info}",
                Options,
                _token
            );

    public Task<Result<AssetPair>> GetTradableAssetPairsWithCountryAsync(string pair, string countryCode) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetPair>(
                nameof(KrakenMarketData),
                $"{AssetPairsPath}?pair={pair}&country={countryCode}",
                Options,
                _token
            );

    public Task<Result<AssetPair>> GetTradableAssetPairsByInfoAndCountryAsync(string info, string countryCode) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetPair>(
                nameof(KrakenMarketData),
                $"{AssetPairsPath}?info={info}&country={countryCode}",
                Options,
                _token
            );

    public Task<Result<AssetPair>> GetTradableAssetPairsAsync(string pair, string info, string countryCode) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetPair>(
                nameof(KrakenMarketData),
                $"{AssetPairsPath}?pair={pair}&info={info}&country={countryCode}",
                Options,
                _token
            );


    public Task<Result<AssetTickerInfo>> GetAssetTickerAsync() =>
        _httpClientFactory
            .GetFromJsonAsync<AssetTickerInfo>(
                nameof(KrakenMarketData),
                AssetTickerPath,
                Options,
                _token
            );

    public Task<Result<AssetTickerInfo>> GetAssetTickerAsync(string pair) =>
        _httpClientFactory
            .GetFromJsonAsync<AssetTickerInfo>(
                nameof(KrakenMarketData),
                $"{AssetTickerPath}?pair={pair}",
                Options,
                _token
            );

    public Task<Result<OhlcResponse>> GetOhlcAsync(string pair) =>
        _httpClientFactory
            .GetFromJsonAsync<OhlcResponse>(
                nameof(KrakenMarketData),
                $"{OhlcPath}?pair={pair}",
                Options,
                _token
            );

    public Task<Result<OhlcResponse>> GetOhlcAsync(string pair, OhlcInterval interval)
    {
        if (interval is < OhlcInterval.OneMinute or > OhlcInterval.FifteenDays)
            throw new ArgumentOutOfRangeException(nameof(interval));

        return _httpClientFactory
            .GetFromJsonAsync<OhlcResponse>(
                nameof(KrakenMarketData),
                $"{OhlcPath}?pair={pair}&interval={(int)interval}",
                Options,
                _token
            );
    }

    public Task<Result<OrderBook>> GetOrderBookAsync(string pair) =>
        _httpClientFactory
            .GetFromJsonAsync<OrderBook>(
                nameof(KrakenMarketData),
                $"{OrderBookPath}?pair={pair}",
                Options,
                _token
            );

    public Task<Result<OrderBook>> GetOrderBookAsync(string pair, int count)
    {
        if (count is < SmallestNumberOfAsksAndBids or > GreatestNumberOfAsksAndBids)
            throw new ArgumentOutOfRangeException(nameof(count));

        return _httpClientFactory
            .GetFromJsonAsync<OrderBook>(
                nameof(KrakenMarketData),
                $"{OrderBookPath}?pair={pair}&count={count}",
                Options,
                _token
            );
    }

    public Task<Result<RecentTradesResponse>> GetRecentTrades(string pair) =>
        _httpClientFactory
            .GetFromJsonAsync<RecentTradesResponse>(
                nameof(KrakenMarketData),
                $"{RecentTradesPath}?pair={pair}",
                Options,
                _token
            );

    public Task<Result<RecentTradesResponse>> GetRecentTrades(string pair, int since) =>
        _httpClientFactory
            .GetFromJsonAsync<RecentTradesResponse>(
                nameof(KrakenMarketData),
                $"{RecentTradesPath}?pair={pair}&since={since}",
                Options,
                _token
            );

    public Task<Result<RecentTradesResponse>> GetRecentTrades(string pair, int since, int count)
    {
        if (count is < SmallestAmountOfRecentTradesPerRequest or > GreatestAmountOfRecentTradesPerRequest)
            throw new ArgumentOutOfRangeException(nameof(count));

        return _httpClientFactory
            .GetFromJsonAsync<RecentTradesResponse>(
                nameof(KrakenMarketData),
                $"{RecentTradesPath}?pair={pair}&since={since}&count={count}",
                Options,
                _token
            );
    }

    public Task<Result<SpreadResponse>> GetSpreadAsync(string pair) =>
        _httpClientFactory
            .GetFromJsonAsync<SpreadResponse>(
                nameof(KrakenMarketData),
                $"{SpreadPath}?pair={pair}",
                Options,
                _token
            );

    public Task<Result<SpreadResponse>> GetSpreadAsync(string pair, int since) =>
        _httpClientFactory
            .GetFromJsonAsync<SpreadResponse>(
                nameof(KrakenMarketData),
                $"{SpreadPath}?pair={pair}&since={since}",
                Options,
                _token
            );
}
