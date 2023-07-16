namespace CryptoWatch.API.Types;

public struct Quote
{
    public int id { get; set; }
    public string route { get; set; }
    public string symbol { get; set; }
    public string name { get; set; }
    public bool fiat { get; set; }
}