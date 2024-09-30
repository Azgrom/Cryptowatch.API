using System.Collections;
using System.Text.Json.Serialization;

namespace CryptoWatch.REST.API;

public struct TickInformation
{
    [JsonConstructor]
    public TickInformation(
        int[]    numberOfTrades,
        string   todaysOpeningPrice,
        string[] ask,
        string[] bid,
        string[] lastClosedTrade,
        string[] volume,
        string[] volumeWeightedAveragePrice,
        string[] low,
        string[] high
    )
    {
        NumberOfTrades             = numberOfTrades;
        TodaysOpeningPrice         = todaysOpeningPrice;
        Ask                        = ask;
        Bid                        = bid;
        LastClosedTrade            = lastClosedTrade;
        Volume                     = volume;
        VolumeWeightedAveragePrice = volumeWeightedAveragePrice;
        Low                        = low;
        High                       = high;
    }

    [JsonPropertyName("t")] public int[]    NumberOfTrades             { get; set; }
    [JsonPropertyName("o")] public string   TodaysOpeningPrice         { get; set; }
    [JsonPropertyName("a")] public string[] Ask                        { get; set; }
    [JsonPropertyName("b")] public string[] Bid                        { get; set; }
    [JsonPropertyName("c")] public string[] LastClosedTrade            { get; set; }
    [JsonPropertyName("v")] public string[] Volume                     { get; set; }
    [JsonPropertyName("p")] public string[] VolumeWeightedAveragePrice { get; set; }
    [JsonPropertyName("l")] public string[] Low                        { get; set; }
    [JsonPropertyName("h")] public string[] High                       { get; set; }
}

public sealed class TickInformationCollection : IDictionary<string, TickInformation>
{
    private readonly IDictionary<string, TickInformation> _dictionaryImplementation;

    [JsonConstructor]
    public TickInformationCollection(IDictionary<string, TickInformation> dictionaryImplementation) =>
        _dictionaryImplementation = dictionaryImplementation;

    #region IDictionary<string,TickInformation> Members

    public int                          Count      => _dictionaryImplementation.Count;
    public bool                         IsReadOnly => _dictionaryImplementation.IsReadOnly;
    public ICollection<string>          Keys       => _dictionaryImplementation.Keys;
    public ICollection<TickInformation> Values     => _dictionaryImplementation.Values;

    public TickInformation this[string key]
    {
        get => _dictionaryImplementation[key];
        set => _dictionaryImplementation[key] = value;
    }

    public IEnumerator<KeyValuePair<string, TickInformation>> GetEnumerator() =>
        _dictionaryImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionaryImplementation).GetEnumerator();

    public void Add(KeyValuePair<string, TickInformation> item) => _dictionaryImplementation.Add(item);

    public void Clear() => _dictionaryImplementation.Clear();

    public bool Contains(KeyValuePair<string, TickInformation> item) => _dictionaryImplementation.Contains(item);

    public void CopyTo(KeyValuePair<string, TickInformation>[] array, int arrayIndex) =>
        _dictionaryImplementation.CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, TickInformation> item) => _dictionaryImplementation.Remove(item);

    public void Add(string key, TickInformation value) => _dictionaryImplementation.Add(key, value);

    public bool ContainsKey(string key) => _dictionaryImplementation.ContainsKey(key);

    public bool Remove(string key) => _dictionaryImplementation.Remove(key);

    public bool TryGetValue(string key, out TickInformation value) =>
        _dictionaryImplementation.TryGetValue(key, out value);

    #endregion
}

public struct TickInformationResponse
{
    [JsonConstructor]
    public TickInformationResponse(Error[] error, TickInformationCollection tickInformationCollection)
    {
        Error                     = error;
        TickInformationCollection = tickInformationCollection;
    }

    [JsonPropertyName("error")]  public Error[]                   Error                     { get; set; }
    [JsonPropertyName("result")] public TickInformationCollection TickInformationCollection { get; set; }
}
