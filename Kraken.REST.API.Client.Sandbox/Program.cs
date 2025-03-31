using System.Text.Json;
using Kraken.REST.API.Client;
using Kraken.REST.API.Client.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddHttpClient<KrakenMarketData>(
    httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://api.kraken.com/0/");
    }
);

var build = builder.Build();

var krakenMarketData = build.Services.GetRequiredService<KrakenMarketData>();

var assetsAsync = krakenMarketData.GetAssetsAsync();
assetsAsync.Wait();
Console.WriteLine(assetsAsync.Result);

var serverTimeAsync  = krakenMarketData.GetServerTimeAsync();
serverTimeAsync.Wait();
Console.WriteLine(serverTimeAsync.Result);

var readAsStreamAsync = serverTimeAsync.Result.Content.ReadAsByteArrayAsync();
var utf8JsonReader = new Utf8JsonReader(readAsStreamAsync.Result);
var result         = new ServerTimeInfoJsonConverter().Read(ref utf8JsonReader, typeof(ServerTime), null);

Console.WriteLine(result);

build.Run();