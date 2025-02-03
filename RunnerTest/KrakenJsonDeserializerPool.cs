using System.Buffers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace RunnerTest;

public class KrakenJsonDeserializerPool
{
    private const  uint                                PricePositionInBookArray     = 1;
    private const  uint                                VolumePositionInBookArray    = 2;
    private const  uint                                TimestampPositionInBookArray = 3;
    private static ILogger<KrakenJsonDeserializerPool> _logger;

    private static readonly JsonReaderOptions JsonReaderOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling     = JsonCommentHandling.Skip
    };

    static readonly List<string> Errors = new();

    public static void T()
    {
        ReadOnlySpan<byte> readAllBytes = Encoding.UTF8.GetBytes(RestObjects.ValidAssetPairResponse);

        var utf8JsonReader = new Utf8JsonReader(readAllBytes, JsonReaderOptions);

        while (utf8JsonReader.Read())
        {
            var tokenType = utf8JsonReader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.StartObject: break;
                case JsonTokenType.PropertyName:
                    // utf8JsonReader.ValueSequence
                    var errorPropertyName  = utf8JsonReader.ValueTextEquals("error");
                    var resultPropertyName = utf8JsonReader.ValueTextEquals("result");

                    if (!errorPropertyName && !resultPropertyName)
                    {
                        if (utf8JsonReader.HasValueSequence)
                        {
                            var propName = Encoding.UTF8.GetString(utf8JsonReader.ValueSequence);
                            throw new JsonException($"Unexpected property name: {propName}");
                        }

                        break;
                    }

                    if (errorPropertyName) ErrorSweep(ref utf8JsonReader);
                    if (resultPropertyName) ResultSweep(ref utf8JsonReader);

                    break;
            }
        }

        // JsonSerializer.Deserialize()
        Console.WriteLine();
    }

    private static void ResultSweep(ref Utf8JsonReader utf8JsonReader)
    {
        var successfulRead = utf8JsonReader.Read();
        var tokenType      = utf8JsonReader.TokenType;
        if (!successfulRead)
        {
            _logger.LogWarning("Unexpected end of json");
        }

        if (tokenType is not JsonTokenType.StartObject)
        {
            _logger.LogWarning("Unexpected end of json");
            return;
        }

        successfulRead = utf8JsonReader.Read();
        tokenType      = utf8JsonReader.TokenType;

        if (!successfulRead || tokenType != JsonTokenType.PropertyName)
        {
            _logger.LogWarning("Unexpected end of json");
            return;
        }

        var propName = utf8JsonReader.GetString();

        successfulRead = utf8JsonReader.Read();
        tokenType      = utf8JsonReader.TokenType;

        if (!successfulRead || tokenType is not JsonTokenType.StartObject)
        {
            _logger.LogWarning("Unexpected end of json");
            return;
        }

        successfulRead = utf8JsonReader.Read();
        tokenType      = utf8JsonReader.TokenType;
        var asksPropertyName = utf8JsonReader.ValueTextEquals("asks");

        if (!successfulRead || tokenType is not JsonTokenType.PropertyName ||
            !asksPropertyName)
        {
            _logger.LogWarning("Unexpected end of json");
            return;
        }

        uint matchingBracketsCount = 0;
        uint doubleNumbersCount    = 0;
        uint asksCount               = 0;
        var pairOrderBookSpan = new PairOrderBookSpan(propName);

        while (utf8JsonReader.Read())
        {
            tokenType = utf8JsonReader.TokenType;

            if (tokenType is JsonTokenType.StartArray)
            {
                matchingBracketsCount++;
                continue;
            }

            if (tokenType is JsonTokenType.Number or JsonTokenType.String)
            {
                doubleNumbersCount++;

                if (doubleNumbersCount is PricePositionInBookArray)
                {
                    if (decimal.TryParse(utf8JsonReader.ValueSpan, out var price))
                        pairOrderBookSpan.AsksSpan[(int)asksCount].Price = price;
                    else
                    {
                        if (asksCount is 0)
                        {
                            // TODO
                        }
                        else
                        {
                            var previousPrice = pairOrderBookSpan.AsksSpan[(int)asksCount - 1].Price;
                            pairOrderBookSpan.AsksSpan[(int)asksCount].Price = previousPrice;
                        }
                    }

                    continue;
                }

                if (doubleNumbersCount is VolumePositionInBookArray)
                {
                    if (decimal.TryParse(utf8JsonReader.ValueSpan, out var volume))
                    {
                        pairOrderBookSpan.AsksSpan[(int)asksCount].Volume = volume;
                    }
                    else
                    {
                        if (asksCount is 0)
                        {
                            // TODO
                        }
                        else
                        {
                            var previousVolume = pairOrderBookSpan.AsksSpan[(int)asksCount - 1].Volume;
                            pairOrderBookSpan.AsksSpan[(int)asksCount].Volume = previousVolume;
                        }
                    }

                    continue;
                }

                if (doubleNumbersCount is TimestampPositionInBookArray)
                {
                    doubleNumbersCount = 0;
                    var timestamp = utf8JsonReader.GetUInt64();
                    pairOrderBookSpan.AsksSpan[(int)asksCount].Timestamp = timestamp;

                    continue;
                }
            }

            if (tokenType is JsonTokenType.EndArray)
            {
                matchingBracketsCount--;

                if (matchingBracketsCount == 0)
                {
                    break;
                }

                asksCount++;
            }
        }

        successfulRead = utf8JsonReader.Read();
        tokenType      = utf8JsonReader.TokenType;
        var bidsPropertyName = utf8JsonReader.ValueTextEquals("bids");

        if (successfulRead && tokenType is JsonTokenType.PropertyName && bidsPropertyName)
        {
            uint bidsCount = 0;

            while (utf8JsonReader.Read())
            {
                tokenType = utf8JsonReader.TokenType;

                if (tokenType is JsonTokenType.StartArray)
                {
                    matchingBracketsCount++;
                    continue;
                }

                if (tokenType is JsonTokenType.Number or JsonTokenType.String)
                {
                    doubleNumbersCount++;

                    if (doubleNumbersCount is PricePositionInBookArray)
                    {
                        if (decimal.TryParse(utf8JsonReader.ValueSpan, out var price))
                        {
                            pairOrderBookSpan.BidsSpan[(int)bidsCount].Price = price;
                        }
                        else
                        {
                            if (bidsCount is 0)
                            {
                                // TODO
                            }
                            else
                            {
                                var previousPrice = pairOrderBookSpan.BidsSpan[(int)bidsCount - 1].Price;
                                pairOrderBookSpan.BidsSpan[(int)bidsCount].Price = previousPrice;
                            }
                        }
                    }

                    if (doubleNumbersCount is VolumePositionInBookArray)
                    {

                    }

                    if (doubleNumbersCount is TimestampPositionInBookArray)
                    {

                    }
                }

                if (tokenType is JsonTokenType.EndArray)
                {
                    matchingBracketsCount--;

                    if (matchingBracketsCount == 0)
                    {
                        break;
                    }

                    bidsCount++;
                }
            }
        }

        Console.WriteLine();
    }

    private static void ErrorSweep(ref Utf8JsonReader utf8JsonReader)
    {
        while (utf8JsonReader.Read())
        {
            var tokenType = utf8JsonReader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.StartArray: break;
                case JsonTokenType.EndArray:   return;
                case JsonTokenType.String:
                    var s        = utf8JsonReader.GetString();
                    var position = utf8JsonReader.Position.GetInteger();
                    Debug.Assert(s == null, $"{nameof(JsonTokenType.String)} == null a position {position}");
                    Debug.Assert(s != null, $"{nameof(JsonTokenType.String)} != null a position {position}");
                    Errors.Add(s);
                    break;
                default: Debug.Fail($"Detected unexpected token type: {tokenType}"); break;
            }
        }
    }
}

public record struct PairBookAsk(decimal Price, decimal Volume, ulong Timestamp)
{
    public decimal Price     { get; set; } = 0;
    public decimal Volume    { get; set; } = 0;
    public ulong   Timestamp { get; set; } = 0;
}

public record struct PairBookBid(decimal Price, decimal Volume, ulong Timestamp)
{
    public decimal Price     { get; set; } = 0;
    public decimal Volume    { get; set; } = 0;
    public ulong   Timestamp { get; set; } = 0;
}

public record struct PairOrderBookEntries
{
    private const int BookMaximumSize = 500;

    public PairOrderBookEntries(
        string        orderBookName
    )
    {
        OrderBookName = orderBookName;
    }

    public string        OrderBookName { get; init; }
    public PairBookAsk[] PairBookAsks  { get; init; } = new PairBookAsk[BookMaximumSize];
    public PairBookBid[] PairBookBids  { get; init; } = new PairBookBid[BookMaximumSize];
}

public sealed class PairOrderBookSpan
{
    private PairOrderBookEntries _orderBookEntries;

    public PairOrderBookSpan(string orderBookName)
    {
        _orderBookEntries = new PairOrderBookEntries(orderBookName);
    }

    public string OrderBookName => _orderBookEntries.OrderBookName;
    public Span<PairBookAsk> AsksSpan => _orderBookEntries.PairBookAsks;
    public Span<PairBookBid> BidsSpan => _orderBookEntries.PairBookBids;
}