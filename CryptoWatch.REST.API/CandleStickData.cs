using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoWatch.REST.API;

public readonly struct CandleStickData : IDictionary<string, double[][]>
{
    [JsonIgnore] private readonly IDictionary<string, double[][]> _dictionaryImplementation;

    [JsonConstructor]
    public CandleStickData(IDictionary<string, double[][]> dictionaryImplementation, ulong last)
    {
        _dictionaryImplementation = dictionaryImplementation;
        Last                      = last;
    }

    [JsonIgnore] public ulong Last { get; }

    [JsonIgnore] public double[][] this[string key]
    {
        get => _dictionaryImplementation[key];
        set => _dictionaryImplementation[key] = value;
    }

    [JsonIgnore] public int                     Count      => _dictionaryImplementation.Count;
    [JsonIgnore] public bool                    IsReadOnly => _dictionaryImplementation.IsReadOnly;
    [JsonIgnore] public ICollection<string>     Keys       => _dictionaryImplementation.Keys;
    [JsonIgnore] public ICollection<double[][]> Values     => _dictionaryImplementation.Values;

    public IEnumerator<KeyValuePair<string, double[][]>> GetEnumerator() => _dictionaryImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionaryImplementation).GetEnumerator();

    public void Add(KeyValuePair<string, double[][]> item) { _dictionaryImplementation.Add(item); }

    public void Clear() { _dictionaryImplementation.Clear(); }

    public bool Contains(KeyValuePair<string, double[][]> item) => _dictionaryImplementation.Contains(item);

    public void CopyTo(KeyValuePair<string, double[][]>[] array, int arrayIndex) =>
        _dictionaryImplementation.CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, double[][]> item) => _dictionaryImplementation.Remove(item);

    public void Add(string key, double[][] value) { _dictionaryImplementation.Add(key, value); }

    public bool ContainsKey(string key) => _dictionaryImplementation.ContainsKey(key);

    public bool Remove(string key) => _dictionaryImplementation.Remove(key);

    public bool TryGetValue(string key, out double[][] value) => _dictionaryImplementation.TryGetValue(key, out value);
}

public struct CandleStickResponse
{
    [JsonIgnore] private CandleStickData? _candleStickData;

    [JsonConstructor]
    public CandleStickResponse(Error[] error, JsonElement candleStickData)
    {
        Error                     = error;
        SerializedCandleStickData = candleStickData;
    }

    [JsonPropertyName("error")]  public Error[]     Error                     { get; }
    [JsonPropertyName("result")] public JsonElement SerializedCandleStickData { get; }

    // [JsonIgnore] public CandleStickData CandleStickData
    // {
    //     get
    //     {
    //         if (_candleStickData is null) _candleStickData = DeserializeCandleStickDataFromJsonElement();
    //
    //         return (CandleStickData)_candleStickData;
    //     }
    // }

    private CandleStickData DeserializeCandleStickDataFromJsonElement()
    {
        var jsonObjectEnumerator = SerializedCandleStickData.EnumerateObject();

        var last = jsonObjectEnumerator.Single(x => x.Name is "last").Value
            .Deserialize<ulong>();
        var doublesMap = jsonObjectEnumerator
            .Where(x => x.Name != "last")
            .ToDictionary(x => x.Name, x => x.Value.Deserialize<double[][]>());

        return new CandleStickData(doublesMap, last);
    }
}
