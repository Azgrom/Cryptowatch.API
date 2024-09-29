using System.Collections;
using System.Text.Json.Serialization;

namespace CryptoWatch.REST.API;

public struct TradableAssetPairInfo
{
    [JsonConstructor]
    public TradableAssetPairInfo(
        int        marginCall,
        int        marginStop,
        int        costDecimals,
        int        pairDecimals,
        int        lotDecimals,
        int        lotMultiplier,
        int        longPositionLimit,
        int        shortPositionLimit,
        int[]      leverageBuy,
        int[]      leverageSell,
        string     lot,
        string     @base,
        string     quote,
        string     status,
        string     wsName,
        string     altName,
        string     costMin,
        string     orderMin,
        string     tickSize,
        string     aClassBase,
        string     aClassQuote,
        string     feeVolumeCurrency,
        double[][] fees,
        double[][] feesMaker
    )
    {
        MarginCall = marginCall;
        MarginStop = marginStop;
        CostDecimals = costDecimals;
        PairDecimals = pairDecimals;
        LotDecimals = lotDecimals;
        LotMultiplier = lotMultiplier;
        LongPositionLimit = longPositionLimit;
        ShortPositionLimit = shortPositionLimit;
        LeverageBuy = leverageBuy;
        LeverageSell = leverageSell;
        Lot = lot;
        Base = @base;
        Quote = quote;
        Status = status;
        WsName = wsName;
        AltName = altName;
        CostMin = costMin;
        OrderMin = orderMin;
        TickSize = tickSize;
        AClassBase = aClassBase;
        AClassQuote = aClassQuote;
        FeeVolumeCurrency = feeVolumeCurrency;
        Fees = fees;
        FeesMaker = feesMaker;
    }

    [JsonPropertyName("margin_call")]          public int        MarginCall         { get; set; }
    [JsonPropertyName("margin_stop")]          public int        MarginStop         { get; set; }
    [JsonPropertyName("cost_decimals")]        public int        CostDecimals       { get; set; }
    [JsonPropertyName("pair_decimals")]        public int        PairDecimals       { get; set; }
    [JsonPropertyName("lot_decimals")]         public int        LotDecimals        { get; set; }
    [JsonPropertyName("lot_multiplier")]       public int        LotMultiplier      { get; set; }
    [JsonPropertyName("long_position_limit")]  public int        LongPositionLimit  { get; set; }
    [JsonPropertyName("short_position_limit")] public int        ShortPositionLimit { get; set; }
    [JsonPropertyName("leverage_buy")]         public int[]      LeverageBuy        { get; set; }
    [JsonPropertyName("leverage_sell")]        public int[]      LeverageSell       { get; set; }
    [JsonPropertyName("lot")]                  public string     Lot                { get; set; }
    [JsonPropertyName("base")]                 public string     Base               { get; set; }
    [JsonPropertyName("quote")]                public string     Quote              { get; set; }
    [JsonPropertyName("status")]               public string     Status             { get; set; }
    [JsonPropertyName("wsname")]               public string     WsName             { get; set; }
    [JsonPropertyName("altname")]              public string     AltName            { get; set; }
    [JsonPropertyName("costmin")]              public string     CostMin            { get; set; }
    [JsonPropertyName("ordermin")]             public string     OrderMin           { get; set; }
    [JsonPropertyName("tick_size")]            public string     TickSize           { get; set; }
    [JsonPropertyName("aclass_base")]          public string     AClassBase         { get; set; }
    [JsonPropertyName("aclass_quote")]         public string     AClassQuote        { get; set; }
    [JsonPropertyName("fee_volume_currency")]  public string     FeeVolumeCurrency  { get; set; }
    [JsonPropertyName("fees")]                 public double[][] Fees               { get; set; }
    [JsonPropertyName("fees_maker")]           public double[][] FeesMaker          { get; set; }
}

public struct TradableAssetPairCollection : IDictionary<string, TradableAssetPairInfo>
{
    private readonly IDictionary<string, TradableAssetPairInfo> _dictionaryImplementation;

    [JsonConstructor]
    public TradableAssetPairCollection(IDictionary<string, TradableAssetPairInfo> dictionaryImplementation) =>
        _dictionaryImplementation = dictionaryImplementation;

    #region IDictionary<string,TradableAssetPairInfo> Members

    public int Count => _dictionaryImplementation.Count;

    public bool IsReadOnly => _dictionaryImplementation.IsReadOnly;

    public ICollection<string> Keys => _dictionaryImplementation.Keys;

    public ICollection<TradableAssetPairInfo> Values => _dictionaryImplementation.Values;

    public TradableAssetPairInfo this[string key]
    {
        get => _dictionaryImplementation[key];
        set => _dictionaryImplementation[key] = value;
    }

    public IEnumerator<KeyValuePair<string, TradableAssetPairInfo>> GetEnumerator() =>
        _dictionaryImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionaryImplementation).GetEnumerator();

    public void Add(KeyValuePair<string, TradableAssetPairInfo> item) => _dictionaryImplementation.Add(item);

    public void Clear() => _dictionaryImplementation.Clear();

    public bool Contains(KeyValuePair<string, TradableAssetPairInfo> item) => _dictionaryImplementation.Contains(item);

    public void CopyTo(KeyValuePair<string, TradableAssetPairInfo>[] array, int arrayIndex) =>
        _dictionaryImplementation.CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<string, TradableAssetPairInfo> item) => _dictionaryImplementation.Remove(item);

    public void Add(string key, TradableAssetPairInfo value) => _dictionaryImplementation.Add(key, value);

    public bool ContainsKey(string key) => _dictionaryImplementation.ContainsKey(key);

    public bool Remove(string key) => _dictionaryImplementation.Remove(key);

    public bool TryGetValue(string key, out TradableAssetPairInfo value) =>
        _dictionaryImplementation.TryGetValue(key, out value);

    #endregion
}

public struct TradableAssetPairResponse
{
    [JsonConstructor]
    public TradableAssetPairResponse(Error[] error, TradableAssetPairCollection tradableAssetPairCollection)
    {
        Error = error;
        TradableAssetPairCollection = tradableAssetPairCollection;
    }

    [JsonPropertyName("error")]
    public Error[] Error { get; set; }

    [JsonPropertyName("result")]
    public TradableAssetPairCollection TradableAssetPairCollection { get; set; }
}
