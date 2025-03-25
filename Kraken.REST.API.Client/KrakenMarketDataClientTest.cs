using Kraken.REST.API.Client;

public class KrakenMarketDataClientTest
{
    public static async Task X()
    {
        var httpClient = new HttpClient();
        var krakenData = new KrakenMarketData(httpClient);

        // Example: Get server time
        var response = await krakenData.GetServerTimeAsync();
        var content  = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
    }
}
