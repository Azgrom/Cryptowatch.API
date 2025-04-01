using System.Net;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace Kraken.REST.API.Client.Tests;

public sealed class KrakenApiMock
{
    private readonly WireMockServer _wireMockServer;

    private readonly IRequestBuilder _orderBookGetPath = Request.Create()
        .WithPath(KrakenMarketData.OrderBookPath)
        .WithParam("pair", "XBTUSD")
        .UsingGet();

    public KrakenApiMock() => _wireMockServer = WireMockServer.Start();

    public string Url => _wireMockServer.Url!;

    public void SetupGetServerTimeAsync() =>
        _wireMockServer.Given(
                Request.Create()
                    .WithPath(KrakenMarketData.ServerTimePath)
                    .UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithBody(RestResponses.ServerTimeExample)
            );

    public void SetupValidGetOrderBookEndpoint()
    {
        _wireMockServer.Given(
                _orderBookGetPath
            )
            .RespondWith(
                Response.Create()
                    .WithBody(RestResponses.ValidOrderBookResponse)
                    .WithStatusCode(HttpStatusCode.OK)
            );
    }

    public void SetupGetOrderBookEndpointWithInvalidErrorObject()
    {
        _wireMockServer.Given(
                _orderBookGetPath
            )
            .RespondWith(
                Response.Create()
                    .WithBody(RestResponses.OrderBookResponseWithInvalidErrorObject)
                    .WithStatusCode(HttpStatusCode.OK)
            );
    }

    public static void SetupMappings(WireMockServer server)
    {
        // Base paths

        // 2. GetSystemStatusAsync
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.SystemStatusPath)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    result = new { status = "online", timestamp = "2023-02-14T12:34:56Z" }
                }));

        // 3. GetAssetsAsync (No query)
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetsPath)
                .WithParam("asset",  new WildcardMatcher(""))
                .WithParam("aclass", new WildcardMatcher(""))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetsData" }));

        // 4. GetAssetsAsync with asset parameter
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetsPath)
                .WithParam("asset", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetDataWithAsset" }));

        // 5. GetAssetsByClassAsync with aclass param
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetsPath)
                .WithParam("aclass", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetDataWithClass" }));

        // 6. GetAssetsAsync with asset and aclass params
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetsPath)
                .WithParam("asset",  "*")
                .WithParam("aclass", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetDataWithAssetClass" }));

        // 7. GetTradableAssetPairsAsync (no params)
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetPairsPath)
                .WithParam("pair",    new WildcardMatcher("*"))
                .WithParam("info",    new WildcardMatcher("*"))
                .WithParam("country", new WildcardMatcher("*"))
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetPairsData" }));

        // 8. GetTradableAssetPairsAsync with pair param
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetPairsPath)
                .WithParam("pair", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetPairsDataWithPair" }));

        // 9. GetTradableAssetPairsByInfoAsync with info param
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetPairsPath)
                .WithParam("info", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetPairsDataWithInfo" }));

        // 10. GetTradableAssetPairsByCountryAsync
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetPairsPath)
                .WithParam("country", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetPairsDataWithCountry" }));

        // 11. GetTradableAssetPairsAsync with pair and info params
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetPairsPath)
                .WithParam("pair", "*")
                .WithParam("info", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetPairsDataWithPairInfo" }));

        // 12. GetTradableAssetPairsWithCountryAsync with pair and country
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetPairsPath)
                .WithParam("pair",    "*")
                .WithParam("country", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetPairsDataWithPairCountry" }));

        // 13. GetTradableAssetPairsByInfoAndCountryAsync with info and country
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetPairsPath)
                .WithParam("info",    "*")
                .WithParam("country", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { result = "AssetPairsDataWithInfoCountry" }));

        // 14. GetTradableAssetPairsAsync with pair, info, country
        server.Given(Request.Create()
                .WithPath(KrakenMarketData.AssetPairsPath)
                .WithParam("pair",    "*")
                .WithParam("info",    "*")
                .WithParam("country", "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    result = "AssetPairsDataWithPairInfoCountry"
                }));
    }
}
