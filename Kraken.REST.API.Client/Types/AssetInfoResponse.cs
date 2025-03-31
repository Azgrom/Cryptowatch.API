using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace Kraken.REST.API.Client.Types;

/// <summary>
/// Record that holds the complete mapping of asset identifiers to AssetInfo.
/// </summary>
public sealed record AssetInfoResponse
{
    public AssetInfoResponse(Dictionary<string, AssetInfoData> assets) => Assets = assets;
    public Dictionary<string, AssetInfoData> Assets { get; init; }
}


/// <summary>
/// Record that represents the asset information.
/// </summary>
public record AssetInfoData
{
    public string  Aclass          { get; init; } = string.Empty;
    public string  Altname         { get; init; } = string.Empty;
    public int     Decimals        { get; init; }
    public int     DisplayDecimals { get; init; }
    public decimal CollateralValue { get; init; }
    public string  Status          { get; init; } = string.Empty;
}

/// <summary>
/// Custom converter for deserializing the JSON into a Result<AssetInfoResponse>.
/// </summary>
public class AssetInfoJsonConverter : JsonConverter<Result<AssetInfoResponse>>
{
    /// <summary>
    /// Allowed status values as per the schema.
    /// </summary>
    private static readonly string[] AllowedStatuses = new[]
    {
        "enabled",
        "deposit_only",
        "withdrawal_only",
        "funding_temporarily_disabled"
    };

    public override Result<AssetInfoResponse> Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Begin reading the root object.
        var nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

        if (jsonReader.TokenType != JsonTokenType.StartObject)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartObject but found {jsonReader.TokenType}");

        // Read the "error" property.
        nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error(ErrorCodes.ReadingTokenErrorCode, nextResult.Error);

        if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("error"))
            return new Error(ErrorCodes.UnknownPropertyErrorCode, $"Expected 'error' property but found '{jsonReader.GetString()}'");

        var errorSweepResult = ErrorSweep(ref jsonReader);
        if (errorSweepResult.IsFailure)
            return new Error(ErrorCodes.SweepingPropertyErrorCode, $"Sweeping errors returned {errorSweepResult.Error}");
        // (The errors array is parsed but not used further.)

        // Read the next property, which must be "result".
        nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error(ErrorCodes.ReadingTokenErrorCode, nextResult.Error);

        if (jsonReader.TokenType != JsonTokenType.PropertyName || !jsonReader.ValueTextEquals("result"))
            return new Error(ErrorCodes.UnknownPropertyErrorCode, $"Expected 'result' property but found '{jsonReader.GetString()}'");

        // Read the value for "result".
        nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error(ErrorCodes.ReadingTokenErrorCode, nextResult.Error);

        if (jsonReader.TokenType != JsonTokenType.StartObject)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartObject for 'result' but found {jsonReader.TokenType}");

        var assets = new Dictionary<string, AssetInfoData>();

        // Loop through all properties in the "result" object.
        while (true)
        {
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.ReadingTokenErrorCode, nextResult.Error);

            if (jsonReader.TokenType == JsonTokenType.EndObject)
                break; // End of "result" object.

            if (jsonReader.TokenType != JsonTokenType.PropertyName)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected property name in 'result' but found {jsonReader.TokenType}");

            string assetKey = jsonReader.GetString();

            // Read the asset info object.
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.ReadingTokenErrorCode, nextResult.Error);
            if (jsonReader.TokenType != JsonTokenType.StartObject)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartObject for asset '{assetKey}' but found {jsonReader.TokenType}");

            var assetInfoResult = ReadAssetInfo(ref jsonReader, assetKey);
            if (assetInfoResult.IsFailure)
                return assetInfoResult.Error;

            assets.Add(assetKey, assetInfoResult.Value);
        }

        // Read the end of the root object.
        nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error(ErrorCodes.ReadingTokenErrorCode, nextResult.Error);
        if (jsonReader.TokenType != JsonTokenType.EndObject)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected EndObject for root but found {jsonReader.TokenType}");

        return new AssetInfoResponse(assets);
    }

    public override void Write(Utf8JsonWriter writer, Result<AssetInfoResponse> value, JsonSerializerOptions options)
    {
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
            foreach (var kvp in value.Value.Assets)
            {
                writer.WritePropertyName(kvp.Key);
                writer.WriteStartObject();
                writer.WriteString("aclass",  kvp.Value.Aclass);
                writer.WriteString("altname", kvp.Value.Altname);
                writer.WriteNumber("decimals",         kvp.Value.Decimals);
                writer.WriteNumber("display_decimals", kvp.Value.DisplayDecimals);
                writer.WriteNumber("collateral_value", kvp.Value.CollateralValue);
                writer.WriteString("status", kvp.Value.Status);
                writer.WriteEndObject();
            }
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads a single AssetInfo object from the current JSON position.
    /// </summary>
    private static Result<AssetInfoData> ReadAssetInfo(ref Utf8JsonReader jsonReader, string assetKey)
    {
        string  aclass           = string.Empty;
        string  altname          = string.Empty;
        int     decimals         = 0;
        int     display_decimals = 0;
        decimal collateral_value = 0;
        string  status           = string.Empty;

        bool foundAclass          = false;
        bool foundAltname         = false;
        bool foundDecimals        = false;
        bool foundDisplayDecimals = false;
        bool foundCollateralValue = false;
        bool foundStatus          = false;

        while (true)
        {
            var nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.ReadingTokenErrorCode, nextResult.Error);

            if (jsonReader.TokenType == JsonTokenType.EndObject)
                break; // End of current asset object.

            if (jsonReader.TokenType != JsonTokenType.PropertyName)
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected property name in asset '{assetKey}' but found {jsonReader.TokenType}");

            string propertyName = jsonReader.GetString();
            nextResult = jsonReader.ReadNext();
            if (nextResult.IsFailure)
                return new Error(ErrorCodes.ReadingTokenErrorCode, nextResult.Error);

            switch (propertyName)
            {
                case "aclass":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for 'aclass' in asset '{assetKey}', found {jsonReader.TokenType}");
                    aclass      = jsonReader.GetString();
                    foundAclass = true;
                    break;
                case "altname":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for 'altname' in asset '{assetKey}', found {jsonReader.TokenType}");
                    altname      = jsonReader.GetString();
                    foundAltname = true;
                    break;
                case "decimals":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected number for 'decimals' in asset '{assetKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out decimals))
                        return new Error("ParsingError", $"Failed to parse 'decimals' as integer for asset '{assetKey}'");
                    foundDecimals = true;
                    break;
                case "display_decimals":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected number for 'display_decimals' in asset '{assetKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetInt32(out display_decimals))
                        return new Error("ParsingError", $"Failed to parse 'display_decimals' as integer for asset '{assetKey}'");
                    foundDisplayDecimals = true;
                    break;
                case "collateral_value":
                    if (jsonReader.TokenType != JsonTokenType.Number)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected number for 'collateral_value' in asset '{assetKey}', found {jsonReader.TokenType}");
                    if (!jsonReader.TryGetDecimal(out collateral_value))
                        return new Error("ParsingError", $"Failed to parse 'collateral_value' as decimal for asset '{assetKey}'");
                    foundCollateralValue = true;
                    break;
                case "status":
                    if (jsonReader.TokenType != JsonTokenType.String)
                        return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string for 'status' in asset '{assetKey}', found {jsonReader.TokenType}");
                    status      = jsonReader.GetString();
                    foundStatus = true;
                    break;
                default:
                    // Skip any unknown property.
                    jsonReader.Skip();
                    break;
            }
        }

        // Ensure all required properties are present.
        if (!foundAclass)
            return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'aclass' for asset '{assetKey}'");
        if (!foundAltname)
            return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'altname' for asset '{assetKey}'");
        if (!foundDecimals)
            return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'decimals' for asset '{assetKey}'");
        if (!foundDisplayDecimals)
            return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'display_decimals' for asset '{assetKey}'");
        if (!foundCollateralValue)
            return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'collateral_value' for asset '{assetKey}'");
        if (!foundStatus)
            return new Error(ErrorCodes.MissingPropertyErrorCode, $"Missing property 'status' for asset '{assetKey}'");

        // Validate that the status is one of the allowed values.
        if (Array.IndexOf(AllowedStatuses, status) < 0)
            return new Error(ErrorCodes.InvalidValueErrorCode, $"Invalid status value '{status}' for asset '{assetKey}'");

        return new AssetInfoData
        {
            Aclass          = aclass,
            Altname         = altname,
            Decimals        = decimals,
            DisplayDecimals = display_decimals,
            CollateralValue = collateral_value,
            Status          = status
        };
    }

    /// <summary>
    /// Reads the "error" array from the JSON.
    /// </summary>
    private Result<string[]> ErrorSweep(ref Utf8JsonReader jsonReader)
    {
        var nextResult = jsonReader.ReadNext();
        if (nextResult.IsFailure)
            return new Error(ErrorCodes.StartReadingErrorCode, nextResult.Error);

        if (jsonReader.TokenType != JsonTokenType.StartArray)
            return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected StartArray for 'error' but found {jsonReader.TokenType}");

        var errors = new List<string>();
        while (true)
        {
            var r = jsonReader.ReadNext();
            if (r.IsFailure)
                return new Error(ErrorCodes.ReadingTokenErrorCode, r.Error);

            if (jsonReader.TokenType == JsonTokenType.EndArray)
                break;

            if (jsonReader.TokenType == JsonTokenType.String)
                errors.Add(jsonReader.GetString());
            else
                return new Error(ErrorCodes.UnexpectedTokenErrorCode, $"Expected string in error array but found {jsonReader.TokenType}");
        }
        return errors.ToArray();
    }
}