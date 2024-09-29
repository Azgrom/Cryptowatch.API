using System.Collections;
using System.Text.Json.Serialization;

namespace CryptoWatch.REST.API;

public class AssetInfoResponse
{
    [JsonPropertyName("error")]
    public Error[] error { get; set; }

    [JsonPropertyName("result")]
    public AssetInfoCollection AssetInfoCollection { get; set; }
}

public class AssetInfoCollection : IDictionary<string, AssetInfo>
{
    private IDictionary<string, AssetInfo> _dictionaryImplementation;

    #region IDictionary<string,AssetInfo> Members

    public IEnumerator<KeyValuePair<string, AssetInfo>> GetEnumerator() => _dictionaryImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionaryImplementation).GetEnumerator();

    public void Add(KeyValuePair<string, AssetInfo> item) => _dictionaryImplementation.Add(item);

    public void Clear() => _dictionaryImplementation.Clear();

    public bool Contains(KeyValuePair<string, AssetInfo> item) => _dictionaryImplementation.Contains(item);

    public void CopyTo(KeyValuePair<string, AssetInfo>[] array, int arrayIndex) =>
        _dictionaryImplementation.CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, AssetInfo> item) => _dictionaryImplementation.Remove(item);

    public int Count => _dictionaryImplementation.Count;

    public bool IsReadOnly => _dictionaryImplementation.IsReadOnly;

    public void Add(string key, AssetInfo value) => _dictionaryImplementation.Add(key, value);

    public bool ContainsKey(string key) => _dictionaryImplementation.ContainsKey(key);

    public bool Remove(string key) => _dictionaryImplementation.Remove(key);

    public bool TryGetValue(string key, out AssetInfo value) => _dictionaryImplementation.TryGetValue(key, out value);

    public AssetInfo this[string key]
    {
        get => _dictionaryImplementation[key];
        set => _dictionaryImplementation[key] = value;
    }

    public ICollection<string> Keys => _dictionaryImplementation.Keys;

    public ICollection<AssetInfo> Values => _dictionaryImplementation.Values;

    #endregion
}

public class AssetInfo
{
    public string aclass           { get; set; }
    public string altname          { get; set; }
    public int    decimals         { get; set; }
    public int    display_decimals { get; set; }
    public int    collateral_value { get; set; }
    public string status           { get; set; }
}
