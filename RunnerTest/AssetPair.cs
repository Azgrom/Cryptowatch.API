using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace RunnerTest;

// Record representing a trading asset pair.
public record AssetPair
{
    public string Altname       { get; init; } = string.Empty;
    public string Wsname        { get; init; } = string.Empty;
    public string AclassBase    { get; init; } = string.Empty;
    public string Base          { get; init; } = string.Empty;
    public string AclassQuote   { get; init; } = string.Empty;
    public string Quote         { get; init; } = string.Empty;
    public string Lot           { get; init; } = string.Empty;
    public int    PairDecimals  { get; init; }
    public int    CostDecimals  { get; init; }
    public int    LotDecimals   { get; init; }
    public int    LotMultiplier { get; init; }
    public int[]  LeverageBuy   { get; init; } = Array.Empty<int>();
    public int[]  LeverageSell  { get; init; } = Array.Empty<int>();
    // Fees are represented as a tuple array: (volume, fee)
    public (decimal volume, decimal fee)[] Fees               { get; init; } = Array.Empty<(decimal, decimal)>();
    public (decimal volume, decimal fee)[] FeesMaker          { get; init; } = Array.Empty<(decimal, decimal)>();
    public string                          FeeVolumeCurrency  { get; init; } = string.Empty;
    public int                             MarginCall         { get; init; }
    public int                             MarginStop         { get; init; }
    public string                          Ordermin           { get; init; } = string.Empty;
    public string                          Costmin            { get; init; } = string.Empty;
    public string                          TickSize           { get; init; } = string.Empty;
    public string                          Status             { get; init; } = string.Empty;
    public int                             LongPositionLimit  { get; init; }
    public int                             ShortPositionLimit { get; init; }
}

// Container record holding a mapping from pair name to AssetPair.
public sealed record AssetPairResponse
{
    public AssetPairResponse(Dictionary<string, AssetPair> pairs)
    {
        Pairs = pairs;
    }
    public Dictionary<string, AssetPair> Pairs { get; init; }
}

// Custom converter for deserializing the JSON into a Result<AssetPairResponse>.
public class AssetPairJsonConverter : JsonConverter<Result<AssetPairResponse>>
{
    // Allowed status values per the schema.
    private static readonly string[] AllowedStatuses = new[]
    {
        "online",
        "cancel_only",
        "post_only",
        "limit_only",
        "reduce_only"
    };

    public override Result<AssetPairResponse> Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            // Begin reading the root object.
            if (jsonReader.TokenType is JsonTokenType.None)
            {
                if (jsonReader.ReadNext().IsFailure)
                    return new Error("StartReadingError", jsonReader.ReadNext().Error);
            }

            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error("UnexpectedTokenError", $"Expected StartObject but found {jsonReader.TokenType}");

            // Read the "error" property.
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("error"))
                return new Error("UnknownPropertyError", $"Expected 'error' property but found '{jsonReader.GetString()}'");

            var errorSweepResult = ErrorSweep(ref jsonReader);
            if (errorSweepResult.IsFailure)
                return new Error("ErrorSweep", errorSweepResult.Error);
            // The errors array is parsed but not further used.

            // Read next property, which must be "result".
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("result"))
                return new Error("UnknownPropertyError", $"Expected 'result' property but found '{jsonReader.GetString()}'");

            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error("UnexpectedTokenError", $"Expected StartObject for 'result' but found {jsonReader.TokenType}");

            var pairs = new Dictionary<string, AssetPair>();

            // Loop through each asset pair property in the "result" object.
            while (true)
            {
                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure)
                    return new Error("ReadingTokenError", nextResult.Error);

                if (jsonReader.TokenType == JsonTokenType.EndObject)
                    break; // End of the "result" object.

                if (jsonReader.TokenType != JsonTokenType.PropertyName)
                    return new Error("UnexpectedTokenError", $"Expected property name in 'result' but found {jsonReader.TokenType}");

                string pairKey = jsonReader.GetString();

                nextResult = jsonReader.ReadNext();
                if (nextResult.IsFailure)
                    return new Error("ReadingTokenError", nextResult.Error);

                if (jsonReader.TokenType != JsonTokenType.StartObject)
                    return new Error("UnexpectedTokenError", $"Expected StartObject for asset pair '{pairKey}' but found {jsonReader.TokenType}");

                var assetPairResult = ReadAssetPair(ref jsonReader, pairKey);
                if (assetPairResult.IsFailure)
                    return assetPairResult.Error;

                pairs.Add(pairKey, assetPairResult.Value);
            }

            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.EndObject)
                return new Error("UnexpectedTokenError", $"Expected EndObject for root but found {jsonReader.TokenType}");

            return new AssetPairResponse(pairs);
        }
        catch (Exception ex)
        {
            return new Error("Exception", ex.Message);
        }
    }

    public override void Write(Utf8JsonWriter writer, Result<AssetPairResponse> value, JsonSerializerOptions options)
    {
        // Basic implementation to serialize back into the expected JSON format.
        writer.WriteStartObject();

        writer.WritePropertyName("error");
        writer.WriteStartArray();
        if (value.IsFailure)
        {
            writer.WriteStringValue(value.Error.Code);
            writer.WriteStringValue(value.Error.Message);
        }
        writer.WriteEndArray();

        writer.WritePropertyName("result");
        writer.WriteStartObject();
        if (value.IsSuccess)
        {
            foreach (var kvp in value.Value.Pairs)
            {
                writer.WritePropertyName(kvp.Key);
                writer.WriteStartObject();

                writer.WriteString("altname",      kvp.Value.Altname);
                writer.WriteString("wsname",       kvp.Value.Wsname);
                writer.WriteString("aclass_base",  kvp.Value.AclassBase);
                writer.WriteString("base",         kvp.Value.Base);
                writer.WriteString("aclass_quote", kvp.Value.AclassQuote);
                writer.WriteString("quote",        kvp.Value.Quote);
                writer.WriteString("lot",          kvp.Value.Lot);
                writer.WriteNumber("pair_decimals",  kvp.Value.PairDecimals);
                writer.WriteNumber("cost_decimals",  kvp.Value.CostDecimals);
                writer.WriteNumber("lot_decimals",   kvp.Value.LotDecimals);
                writer.WriteNumber("lot_multiplier", kvp.Value.LotMultiplier);

                writer.WritePropertyName("leverage_buy");
                writer.WriteStartArray();
                foreach (var num in kvp.Value.LeverageBuy)
                    writer.WriteNumberValue(num);
                writer.WriteEndArray();

                writer.WritePropertyName("leverage_sell");
                writer.WriteStartArray();
                foreach (var num in kvp.Value.LeverageSell)
                    writer.WriteNumberValue(num);
                writer.WriteEndArray();

                writer.WritePropertyName("fees");
                writer.WriteStartArray();
                foreach (var fee in kvp.Value.Fees)
                {
                    writer.WriteStartArray();
                    writer.WriteNumberValue(fee.volume);
                    writer.WriteNumberValue(fee.fee);
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();

                writer.WritePropertyName("fees_maker");
                writer.WriteStartArray();
                foreach (var fee in kvp.Value.FeesMaker)
                {
                    writer.WriteStartArray();
                    writer.WriteNumberValue(fee.volume);
                    writer.WriteNumberValue(fee.fee);
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();

                writer.WriteString("fee_volume_currency", kvp.Value.FeeVolumeCurrency);
                writer.WriteNumber("margin_call", kvp.Value.MarginCall);
                writer.WriteNumber("margin_stop", kvp.Value.MarginStop);
                writer.WriteString("ordermin",  kvp.Value.Ordermin);
                writer.WriteString("costmin",   kvp.Value.Costmin);
                writer.WriteString("tick_size", kvp.Value.TickSize);
                writer.WriteString("status",    kvp.Value.Status);
                writer.WriteNumber("long_position_limit",  kvp.Value.LongPositionLimit);
                writer.WriteNumber("short_position_limit", kvp.Value.ShortPositionLimit);

                writer.WriteEndObject();
            }
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads a single AssetPair object from the current JSON position.
    /// </summary>
    private Result<AssetPair> ReadAssetPair(ref Utf8JsonReader jsonReader, string pairKey)
    {
        string altname              = string.Empty;
        string wsname               = string.Empty;
        string aclass_base          = string.Empty;
        string _base                = string.Empty;
        string aclass_quote         = string.Empty;
        string quote                = string.Empty;
        string lot                  = string.Empty;
        int    pair_decimals        = 0;
        int    cost_decimals        = 0;
        int    lot_decimals         = 0;
        int    lot_multiplier       = 0;
        int[]  leverage_buy         = Array.Empty<int>();
        int[]  leverage_sell        = Array.Empty<int>();
        var    fees                 = new List<(decimal volume, decimal fee)>();
        var    fees_maker           = new List<(decimal volume, decimal fee)>();
        string fee_volume_currency  = string.Empty;
        int    margin_call          = 0;
        int    margin_stop          = 0;
        string ordermin             = string.Empty;
        string costmin              = string.Empty;
        string tick_size            = string.Empty;
        string status               = string.Empty;
        int    long_position_limit  = 0;
        int    short_position_limit = 0;

        // Flags to ensure required properties are found.
        bool found_altname              = false;
        bool found_wsname               = false;
        bool found_aclass_base          = false;
        bool found_base                 = false;
        bool found_aclass_quote         = false;
        bool found_quote                = false;
        bool found_lot                  = false;
        bool found_pair_decimals        = false;
        bool found_cost_decimals        = false;
        bool found_lot_decimals         = false;
        bool found_lot_multiplier       = false;
        bool found_leverage_buy         = false;
        bool found_leverage_sell        = false;
        bool found_fees                 = false;
        bool found_fees_maker           = false;
        bool found_fee_volume_currency  = false;
        bool found_margin_call          = false;
        bool found_margin_stop          = false;
        bool found_ordermin             = false;
        bool found_costmin              = false;
        bool found_tick_size            = false;
        bool found_status               = false;
        bool found_long_position_limit  = false;
        bool found_short_position_limit = false;

        while (true)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            if (jsonReader.TokenType == JsonTokenType.EndObject)
                break;

            if (jsonReader.TokenType != JsonTokenType.PropertyName)
                return new Error("UnexpectedTokenError", $"Expected property name in asset pair '{pairKey}' but found {jsonReader.TokenType}");

            string propertyName = jsonReader.GetString();
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error("ReadingTokenError", nextResult.Error);

            switch (propertyName)
            {
                case "altname":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'altname' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    altname       = jsonReader.GetString();
                    found_altname = true;
                    break;
                case "wsname":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'wsname' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    wsname       = jsonReader.GetString();
                    found_wsname = true;
                    break;
                case "aclass_base":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'aclass_base' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    aclass_base       = jsonReader.GetString();
                    found_aclass_base = true;
                    break;
                case "base":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'base' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    _base      = jsonReader.GetString();
                    found_base = true;
                    break;
                case "aclass_quote":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'aclass_quote' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    aclass_quote       = jsonReader.GetString();
                    found_aclass_quote = true;
                    break;
                case "quote":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'quote' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    quote       = jsonReader.GetString();
                    found_quote = true;
                    break;
                case "lot":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'lot' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    lot       = jsonReader.GetString();
                    found_lot = true;
                    break;
                case "pair_decimals":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError", $"Expected number for 'pair_decimals' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out pair_decimals))
                        return new Error("ParsingError", $"Failed to parse 'pair_decimals' for asset pair '{pairKey}'");
                    found_pair_decimals = true;
                    break;
                case "cost_decimals":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError", $"Expected number for 'cost_decimals' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out cost_decimals))
                        return new Error("ParsingError", $"Failed to parse 'cost_decimals' for asset pair '{pairKey}'");
                    found_cost_decimals = true;
                    break;
                case "lot_decimals":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError", $"Expected number for 'lot_decimals' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out lot_decimals))
                        return new Error("ParsingError", $"Failed to parse 'lot_decimals' for asset pair '{pairKey}'");
                    found_lot_decimals = true;
                    break;
                case "lot_multiplier":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError", $"Expected number for 'lot_multiplier' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out lot_multiplier))
                        return new Error("ParsingError", $"Failed to parse 'lot_multiplier' for asset pair '{pairKey}'");
                    found_lot_multiplier = true;
                    break;
                case "leverage_buy":
                    if (jsonReader.TokenType != JsonTokenType.StartArray)
                        return new Error("UnexpectedTokenError", $"Expected array for 'leverage_buy' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    var levBuyList = new List<int>();
                    while (true)
                    {
                        var r = jsonReader.ReadNext();
                        if (r.IsFailure)
                            return new Error("ReadingTokenError", r.Error);
                        if (jsonReader.TokenType == JsonTokenType.EndArray)
                            break;
                        if (jsonReader.TokenType != JsonTokenType.Number)
                            return new Error("UnexpectedTokenError", $"Expected number in 'leverage_buy' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                        if (!jsonReader.TryGetInt32(out int lev))
                            return new Error("ParsingError", $"Failed to parse number in 'leverage_buy' for asset pair '{pairKey}'");
                        levBuyList.Add(lev);
                    }
                    leverage_buy       = levBuyList.ToArray();
                    found_leverage_buy = true;
                    break;
                case "leverage_sell":
                    if (jsonReader.TokenType != JsonTokenType.StartArray)
                        return new Error("UnexpectedTokenError", $"Expected array for 'leverage_sell' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    var levSellList = new List<int>();
                    while (true)
                    {
                        var r = jsonReader.ReadNext();
                        if (r.IsFailure)
                            return new Error("ReadingTokenError", r.Error);
                        if (jsonReader.TokenType == JsonTokenType.EndArray)
                            break;
                        if (jsonReader.TokenType != JsonTokenType.Number)
                            return new Error("UnexpectedTokenError", $"Expected number in 'leverage_sell' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                        if (!jsonReader.TryGetInt32(out int lev))
                            return new Error("ParsingError", $"Failed to parse number in 'leverage_sell' for asset pair '{pairKey}'");
                        levSellList.Add(lev);
                    }
                    leverage_sell       = levSellList.ToArray();
                    found_leverage_sell = true;
                    break;
                case "fees":
                    if (jsonReader.TokenType != JsonTokenType.StartArray)
                        return new Error("UnexpectedTokenError", $"Expected array for 'fees' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    fees = new List<(decimal volume, decimal fee)>();
                    while (true)
                    {
                        var r = jsonReader.ReadNext();
                        if (r.IsFailure)
                            return new Error("ReadingTokenError", r.Error);
                        if (jsonReader.TokenType == JsonTokenType.EndArray)
                            break;
                        if (jsonReader.TokenType != JsonTokenType.StartArray)
                            return new Error("UnexpectedTokenError", $"Expected sub-array for 'fees' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                        decimal feeVolume  = 0;
                        decimal feePercent = 0;
                        var     r1         = jsonReader.ReadNext();
                        if (r1.IsFailure)
                            return new Error("ReadingTokenError", r1.Error);
                        if (jsonReader.TokenType != JsonTokenType.Number)
                            return new Error("UnexpectedTokenError", $"Expected number for fee volume in 'fees' for asset pair '{pairKey}', found {jsonReader.TokenType}");
                        if (!jsonReader.TryGetDecimal(out feeVolume))
                            return new Error("ParsingError", $"Failed to parse fee volume in 'fees' for asset pair '{pairKey}'");
                        var r2 = jsonReader.ReadNext();
                        if (r2.IsFailure)
                            return new Error("ReadingTokenError", r2.Error);
                        if (jsonReader.TokenType != JsonTokenType.Number)
                            return new Error("UnexpectedTokenError", $"Expected number for fee percent in 'fees' for asset pair '{pairKey}', found {jsonReader.TokenType}");
                        if (!jsonReader.TryGetDecimal(out feePercent))
                            return new Error("ParsingError", $"Failed to parse fee percent in 'fees' for asset pair '{pairKey}'");
                        var r3 = jsonReader.ReadNext();
                        if (r3.IsFailure)
                            return new Error("ReadingTokenError", r3.Error);
                        if (jsonReader.TokenType != JsonTokenType.EndArray)
                            return new Error("UnexpectedTokenError", $"Expected end of sub-array for 'fees' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                        fees.Add((feeVolume, feePercent));
                    }
                    found_fees = true;
                    break;
                case "fees_maker":
                    if (jsonReader.TokenType != JsonTokenType.StartArray)
                        return new Error("UnexpectedTokenError", $"Expected array for 'fees_maker' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    fees_maker = new List<(decimal volume, decimal fee)>();
                    while (true)
                    {
                        var r = jsonReader.ReadNext();
                        if (r.IsFailure)
                            return new Error("ReadingTokenError", r.Error);
                        if (jsonReader.TokenType == JsonTokenType.EndArray)
                            break;
                        if (jsonReader.TokenType != JsonTokenType.StartArray)
                            return new Error("UnexpectedTokenError", $"Expected sub-array for 'fees_maker' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                        decimal feeVolume  = 0;
                        decimal feePercent = 0;
                        var     r1         = jsonReader.ReadNext();
                        if (r1.IsFailure)
                            return new Error("ReadingTokenError", r1.Error);
                        if (jsonReader.TokenType != JsonTokenType.Number)
                            return new Error("UnexpectedTokenError", $"Expected number for fee volume in 'fees_maker' for asset pair '{pairKey}', found {jsonReader.TokenType}");
                        if (!jsonReader.TryGetDecimal(out feeVolume))
                            return new Error("ParsingError", $"Failed to parse fee volume in 'fees_maker' for asset pair '{pairKey}'");
                        var r2 = jsonReader.ReadNext();
                        if (r2.IsFailure)
                            return new Error("ReadingTokenError", r2.Error);
                        if (jsonReader.TokenType != JsonTokenType.Number)
                            return new Error("UnexpectedTokenError", $"Expected number for fee percent in 'fees_maker' for asset pair '{pairKey}', found {jsonReader.TokenType}");
                        if (!jsonReader.TryGetDecimal(out feePercent))
                            return new Error("ParsingError", $"Failed to parse fee percent in 'fees_maker' for asset pair '{pairKey}'");
                        var r3 = jsonReader.ReadNext();
                        if (r3.IsFailure)
                            return new Error("ReadingTokenError", r3.Error);
                        if (jsonReader.TokenType != JsonTokenType.EndArray)
                            return new Error("UnexpectedTokenError", $"Expected end of sub-array for 'fees_maker' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                        fees_maker.Add((feeVolume, feePercent));
                    }
                    found_fees_maker = true;
                    break;
                case "fee_volume_currency":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'fee_volume_currency' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    fee_volume_currency       = jsonReader.GetString();
                    found_fee_volume_currency = true;
                    break;
                case "margin_call":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError", $"Expected number for 'margin_call' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out margin_call))
                        return new Error("ParsingError", $"Failed to parse 'margin_call' for asset pair '{pairKey}'");
                    found_margin_call = true;
                    break;
                case "margin_stop":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError", $"Expected number for 'margin_stop' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out margin_stop))
                        return new Error("ParsingError", $"Failed to parse 'margin_stop' for asset pair '{pairKey}'");
                    found_margin_stop = true;
                    break;
                case "ordermin":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'ordermin' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    ordermin       = jsonReader.GetString();
                    found_ordermin = true;
                    break;
                case "costmin":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'costmin' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    costmin       = jsonReader.GetString();
                    found_costmin = true;
                    break;
                case "tick_size":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'tick_size' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    tick_size       = jsonReader.GetString();
                    found_tick_size = true;
                    break;
                case "status":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error("UnexpectedTokenError", $"Expected string for 'status' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    status       = jsonReader.GetString();
                    found_status = true;
                    break;
                case "long_position_limit":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError", $"Expected number for 'long_position_limit' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out long_position_limit))
                        return new Error("ParsingError", $"Failed to parse 'long_position_limit' for asset pair '{pairKey}'");
                    found_long_position_limit = true;
                    break;
                case "short_position_limit":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error("UnexpectedTokenError", $"Expected number for 'short_position_limit' in asset pair '{pairKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out short_position_limit))
                        return new Error("ParsingError", $"Failed to parse 'short_position_limit' for asset pair '{pairKey}'");
                    found_short_position_limit = true;
                    break;
                default:
                    // Skip any unknown property.
                    jsonReader.Skip();
                    break;
            }
        }

        // Validate that all required properties are found.
        if (!found_altname)
            return new Error("MissingPropertyError", $"Missing property 'altname' for asset pair '{pairKey}'");
        if (!found_wsname)
            return new Error("MissingPropertyError", $"Missing property 'wsname' for asset pair '{pairKey}'");
        if (!found_aclass_base)
            return new Error("MissingPropertyError", $"Missing property 'aclass_base' for asset pair '{pairKey}'");
        if (!found_base)
            return new Error("MissingPropertyError", $"Missing property 'base' for asset pair '{pairKey}'");
        if (!found_aclass_quote)
            return new Error("MissingPropertyError", $"Missing property 'aclass_quote' for asset pair '{pairKey}'");
        if (!found_quote)
            return new Error("MissingPropertyError", $"Missing property 'quote' for asset pair '{pairKey}'");
        if (!found_lot)
            return new Error("MissingPropertyError", $"Missing property 'lot' for asset pair '{pairKey}'");
        if (!found_pair_decimals)
            return new Error("MissingPropertyError", $"Missing property 'pair_decimals' for asset pair '{pairKey}'");
        if (!found_cost_decimals)
            return new Error("MissingPropertyError", $"Missing property 'cost_decimals' for asset pair '{pairKey}'");
        if (!found_lot_decimals)
            return new Error("MissingPropertyError", $"Missing property 'lot_decimals' for asset pair '{pairKey}'");
        if (!found_lot_multiplier)
            return new Error("MissingPropertyError", $"Missing property 'lot_multiplier' for asset pair '{pairKey}'");
        if (!found_leverage_buy)
            return new Error("MissingPropertyError", $"Missing property 'leverage_buy' for asset pair '{pairKey}'");
        if (!found_leverage_sell)
            return new Error("MissingPropertyError", $"Missing property 'leverage_sell' for asset pair '{pairKey}'");
        if (!found_fees)
            return new Error("MissingPropertyError", $"Missing property 'fees' for asset pair '{pairKey}'");
        if (!found_fees_maker)
            return new Error("MissingPropertyError", $"Missing property 'fees_maker' for asset pair '{pairKey}'");
        if (!found_fee_volume_currency)
            return new Error("MissingPropertyError", $"Missing property 'fee_volume_currency' for asset pair '{pairKey}'");
        if (!found_margin_call)
            return new Error("MissingPropertyError", $"Missing property 'margin_call' for asset pair '{pairKey}'");
        if (!found_margin_stop)
            return new Error("MissingPropertyError", $"Missing property 'margin_stop' for asset pair '{pairKey}'");
        if (!found_ordermin)
            return new Error("MissingPropertyError", $"Missing property 'ordermin' for asset pair '{pairKey}'");
        if (!found_costmin)
            return new Error("MissingPropertyError", $"Missing property 'costmin' for asset pair '{pairKey}'");
        if (!found_tick_size)
            return new Error("MissingPropertyError", $"Missing property 'tick_size' for asset pair '{pairKey}'");
        if (!found_status)
            return new Error("MissingPropertyError", $"Missing property 'status' for asset pair '{pairKey}'");
        if (!found_long_position_limit)
            return new Error("MissingPropertyError", $"Missing property 'long_position_limit' for asset pair '{pairKey}'");
        if (!found_short_position_limit)
            return new Error("MissingPropertyError", $"Missing property 'short_position_limit' for asset pair '{pairKey}'");

        // Validate the "status" value.
        if (Array.IndexOf(AllowedStatuses, status) < 0)
            return new Error("InvalidValueError", $"Invalid status value '{status}' for asset pair '{pairKey}'");

        var assetPair = new AssetPair
        {
            Altname            = altname,
            Wsname             = wsname,
            AclassBase         = aclass_base,
            Base               = _base,
            AclassQuote        = aclass_quote,
            Quote              = quote,
            Lot                = lot,
            PairDecimals       = pair_decimals,
            CostDecimals       = cost_decimals,
            LotDecimals        = lot_decimals,
            LotMultiplier      = lot_multiplier,
            LeverageBuy        = leverage_buy,
            LeverageSell       = leverage_sell,
            Fees               = fees.ToArray(),
            FeesMaker          = fees_maker.ToArray(),
            FeeVolumeCurrency  = fee_volume_currency,
            MarginCall         = margin_call,
            MarginStop         = margin_stop,
            Ordermin           = ordermin,
            Costmin            = costmin,
            TickSize           = tick_size,
            Status             = status,
            LongPositionLimit  = long_position_limit,
            ShortPositionLimit = short_position_limit
        };

        return assetPair;
    }

    /// <summary>
    /// Reads the "error" array from the JSON.
    /// </summary>
    private Result<string[]> ErrorSweep(ref Utf8JsonReader jsonReader)
    {
        var nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error("StartReadingError", nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.StartArray)
            return new Error("UnexpectedTokenError", $"Expected StartArray for 'error' but found {jsonReader.TokenType}");
        var errors = new List<string>();
        while (true)
        {
            var r = jsonReader.ReadNext();
            if (r.IsFailure)
                return new Error("ReadingTokenError", r.Error);
            if (jsonReader.TokenType == JsonTokenType.EndArray)
                break;
            if (jsonReader.TokenType == JsonTokenType.String)
                errors.Add(jsonReader.GetString());
            else
                return new Error("UnexpectedTokenError", $"Expected string in error array but found {jsonReader.TokenType}");
        }
        return errors.ToArray();
    }
}