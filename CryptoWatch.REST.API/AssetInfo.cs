using System.Collections;
using System.Text.Json.Serialization;

namespace CryptoWatch.REST.API;

public struct AssetInfo
{
    [JsonConstructor]
    public AssetInfo(
        int    decimals,
        int    displayDecimals,
        int    collateralValue,
        string aclass,
        string altname,
        string status
    )
    {
        Decimals = decimals;
        DisplayDecimals = displayDecimals;
        CollateralValue = collateralValue;
        Aclass = aclass;
        Altname = altname;
        Status = status;
    }

    [JsonPropertyName("decimals")]
    public int Decimals { get; set; }
    [JsonPropertyName("display_decimals")]
    public int DisplayDecimals { get; set; }
    [JsonPropertyName("collateral_value")]
    public int CollateralValue { get; set; }
    [JsonPropertyName("aclass")]
    public string Aclass { get; set; }
    [JsonPropertyName("altname")]
    public string Altname { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
}

public struct AssetInfoCollection : IDictionary<string, AssetInfo>
{
    private readonly IDictionary<string, AssetInfo> _dictionaryImplementation;

    [JsonConstructor]
    public AssetInfoCollection(IDictionary<string, AssetInfo> dictionaryImplementation) =>
        _dictionaryImplementation = dictionaryImplementation;

    #region IDictionary<string,AssetInfo> Members

    public int Count => _dictionaryImplementation.Count;
    public bool IsReadOnly => _dictionaryImplementation.IsReadOnly;
    public ICollection<string> Keys => _dictionaryImplementation.Keys;
    public ICollection<AssetInfo> Values => _dictionaryImplementation.Values;
    public AssetInfo this[string key]
    {
        get => _dictionaryImplementation[key];
        set => _dictionaryImplementation[key] = value;
    }

    public IEnumerator<KeyValuePair<string, AssetInfo>> GetEnumerator() => _dictionaryImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionaryImplementation).GetEnumerator();

    public void Add(KeyValuePair<string, AssetInfo> item) => _dictionaryImplementation.Add(item);

    public void Clear() => _dictionaryImplementation.Clear();

    public bool Contains(KeyValuePair<string, AssetInfo> item) => _dictionaryImplementation.Contains(item);

    public void CopyTo(KeyValuePair<string, AssetInfo>[] array, int arrayIndex) =>
        _dictionaryImplementation.CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, AssetInfo> item) => _dictionaryImplementation.Remove(item);

    public void Add(string key, AssetInfo value) => _dictionaryImplementation.Add(key, value);

    public bool ContainsKey(string key) => _dictionaryImplementation.ContainsKey(key);

    public bool Remove(string key) => _dictionaryImplementation.Remove(key);

    public bool TryGetValue(string key, out AssetInfo value) => _dictionaryImplementation.TryGetValue(key, out value);

    #endregion
}

public struct AssetInfoResponse
{
    [JsonConstructor]
    public AssetInfoResponse(Error[] error, AssetInfoCollection assetInfoCollection)
    {
        Error = error;
        AssetInfoCollection = assetInfoCollection;
    }

    [JsonPropertyName("error")] public Error[] Error { get; set; }
    [JsonPropertyName("result")] public AssetInfoCollection AssetInfoCollection { get; set; }
}
