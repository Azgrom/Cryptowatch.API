using System.Buffers.Text;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;

namespace Kraken.REST.API.Client.Types;

public sealed record OrderBookResponse
{
    public OrderBookResponse(string[] errors, List<PairOrderBookEntries> pairOrderBookSpan)
    {
        Errors            = errors;
        PairOrderBookSpan = pairOrderBookSpan;
    }

    public string[]                   Errors            { get; private set; }
    public List<PairOrderBookEntries> PairOrderBookSpan { get; private set; }
}

public class OrderBookJsonConverter : JsonConverter<Result<OrderBookResponse>>
{
    private const uint          PricePositionInBookArray     = 1;
    private const uint          VolumePositionInBookArray    = 2;
    private const uint          TimestampPositionInBookArray = 3;
    private       Result        _nextReadResult              = Error.NullValue;
    private       JsonTokenType _tokenType                   = JsonTokenType.None;
    private       BookBuilder   _bookBuilder                 = BookBuilder.Create();

    public Result<OrderBookResponse> IntoOhlc(ref Utf8JsonReader jsonReader) =>
        Read(ref jsonReader, typeof(OrderBookResponse), null);

    public override Result<OrderBookResponse> Read(
        ref Utf8JsonReader    jsonReader,
        Type                  typeToConvert,
        JsonSerializerOptions options
    )
    {
        _nextReadResult = jsonReader.ReadNext();
        if (_nextReadResult.IsSuccess)
        {
            _tokenType = jsonReader.TokenType;
        }
        else
        {
            _nextReadResult = new Error(ErrorCodes.StartReadingErrorCode, _nextReadResult.Error);
        }

        if (_tokenType is JsonTokenType.StartObject && _nextReadResult.IsSuccess)
        {
            _nextReadResult = jsonReader.ReadNext();
            if (_nextReadResult.IsSuccess)
            {
                _tokenType = jsonReader.TokenType;
            }
            else
            {
                _nextReadResult = new Error(ErrorCodes.EnteringObjectErrorCode, _nextReadResult.Error);
            }
        }
        else
        {
            // TODO Failure result for JsonTokenType different of StartObject
        }

        var errors            = Array.Empty<string>();
        var errorPropertyName = jsonReader.ValueTextEquals("error");
        if (errorPropertyName && _nextReadResult.IsSuccess)
        {
            var errorSweep = ErrorSweep(ref jsonReader);

            if (errorSweep.IsFailure)
            {
                _nextReadResult = errorSweep.Error;
            }
            else
            {
                if (errorSweep.Value.Length > 0)
                {
                    errors = errorSweep.Value;
                }
            }
        }

        _nextReadResult = jsonReader.ReadNext();
        if (_nextReadResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, _nextReadResult.Error);

        _tokenType = jsonReader.TokenType;
        var resultPropertyName = jsonReader.ValueTextEquals("result");

        List<PairOrderBookEntries>? pairOrderBookEntries = null;
        if (resultPropertyName && _nextReadResult.IsSuccess)
        {
            var resultSweep = ResultSweep(ref jsonReader);
            if (resultSweep.IsFailure)
            {
                _nextReadResult = resultSweep.Error;
            }
            else
            {
                pairOrderBookEntries = resultSweep.Value;
            }
        }

        _nextReadResult = jsonReader.ReadNext();
        if (_nextReadResult.IsFailure) return new Error(ErrorCodes.StartReadingErrorCode, _nextReadResult.Error);

        _tokenType = jsonReader.TokenType;

        if (!errorPropertyName && !resultPropertyName && jsonReader.HasValueSequence &&
            pairOrderBookEntries is null)
        {
            _nextReadResult = UnknownPropertyResult(ref jsonReader);
        }

        if (_tokenType is JsonTokenType.EndObject &&
            pairOrderBookEntries is not null)
        {
            _nextReadResult = jsonReader.ReadNext();
            jsonReader.TrySkip();
            return new OrderBookResponse(errors, pairOrderBookEntries);
        }

        return new Error(ErrorCodes.UnexpectedTokenErrorCode,
            $"Unexpected End of Error Array: {_nextReadResult.Error}");
    }

    public override void Write(Utf8JsonWriter writer, Result<OrderBookResponse> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private static Error UnknownPropertyResult(ref Utf8JsonReader jsonReader)
    {
        var propName = Encoding.UTF8.GetString(jsonReader.ValueSpan);
        return new Error(ErrorCodes.UnknownPropertyErrorCode, propName);
    }

    private Result<string[]> ErrorSweep(ref Utf8JsonReader jsonReader)
    {
        _nextReadResult = jsonReader.ReadNext();
        if (_nextReadResult.IsSuccess)
        {
            _tokenType = jsonReader.TokenType;
        }
        else
        {
            return new Error(ErrorCodes.StartReadingErrorCode, _nextReadResult.Error);
        }

        if (_tokenType is JsonTokenType.StartArray)
        {
            _nextReadResult = jsonReader.ReadNext();
            if (_nextReadResult.IsSuccess)
            {
                _tokenType = jsonReader.TokenType;
            }
            else
            {
                return new Error(ErrorCodes.EnteringArrayErrorCode, _nextReadResult.Error);
            }
        }

        var errors = new List<string>();
        while (_nextReadResult.IsSuccess && _tokenType is JsonTokenType.String)
        {
            var s        = jsonReader.GetString();
            var position = jsonReader.Position.GetInteger();
            Debug.Assert(s == null, (string)$"{nameof(JsonTokenType.String)} == null a position {position}");
            Debug.Assert(s != null, (string)$"{nameof(JsonTokenType.String)} != null a position {position}");
            errors.Add(s);

            _nextReadResult = jsonReader.ReadNext();
            if (_nextReadResult.IsSuccess)
            {
                _tokenType = jsonReader.TokenType;
            }
            else
            {
                return new Error(ErrorCodes.StartReadingErrorCode, _nextReadResult.Error);
            }
        }

        if (_tokenType is JsonTokenType.EndArray) return errors.ToArray();

        return new Error(ErrorCodes.UnexpectedTokenErrorCode, "Unexpected End of Error Array");
    }

    private Result<List<PairOrderBookEntries>> ResultSweep(ref Utf8JsonReader jsonReader)
    {
        var error = Error.None;
        _nextReadResult = jsonReader.ReadNext();
        if (_nextReadResult.IsFailure)
        {
            error = new Error(ErrorCodes.StartReadingErrorCode, _nextReadResult.Error);
        }

        _tokenType = jsonReader.TokenType;
        if (_tokenType is not JsonTokenType.StartObject)
        {
            error = new Error(ErrorCodes.EnteringObjectErrorCode, _nextReadResult.Error);
        }

        var pairOrderBookSpan = new List<PairOrderBookEntries>(1);
        _nextReadResult = jsonReader.ReadNext();
        while (_nextReadResult.IsSuccess)
        {
            _tokenType = jsonReader.TokenType;

            if (_tokenType is JsonTokenType.EndArray)
            {
                _nextReadResult = jsonReader.ReadNext();
                continue;
            }

            if (_tokenType is JsonTokenType.EndObject)
            {
                break;
            }

            if (_nextReadResult.IsFailure || _tokenType != JsonTokenType.PropertyName)
            {
                error = new Error(ErrorCodes.EnteringPropertyNameErrorCode,
                    _nextReadResult.Error);
            }

            var propName = jsonReader.GetString();

            _nextReadResult = jsonReader.ReadNext();
            _tokenType      = jsonReader.TokenType;

            if (_nextReadResult.IsFailure || _tokenType is not JsonTokenType.StartObject)
            {
                error = new Error(ErrorCodes.EnteringObjectErrorCode, _nextReadResult.Error);
            }

            _bookBuilder.CollectAsks(ref jsonReader);

            if (_bookBuilder.BuildStepResult.IsFailure)
            {
                // todo
            }

            _bookBuilder.CollectBids(ref jsonReader);

            if (_bookBuilder.BuildStepResult.IsFailure)
            {
                // todo
            }

            if (propName is not null)
            {
                var orderBookSpan = _bookBuilder.Build(propName);

                pairOrderBookSpan.Add(orderBookSpan);
            }
        }

        if (error != Error.None)
        {
            return error;
        }

        return pairOrderBookSpan;
    }

    public struct BookBuilder()
    {
        private          uint            _matchingBracketsCount = 0;
        private          uint            _doubleNumbersCount    = 0;
        private const    int             BookMaximumSize        = 100;
        private const    string?         BookAsksJsonProperty   = "asks";
        private const    string?         BookBidsJsonProperty   = "bids";
        private readonly List<BookEntry> _asks                  = new(BookMaximumSize);
        private readonly List<BookEntry> _bids                  = new(BookMaximumSize);
        private          Result          _nextReadResult        = Error.NullValue;
        private          JsonTokenType   _tokenType             = JsonTokenType.None;

        public Result BuildStepResult => _nextReadResult;

        public static BookBuilder Create() => new();

        public void CollectAsks(ref Utf8JsonReader jsonReader)
        {
            _nextReadResult = jsonReader.ReadNext();
            _tokenType      = jsonReader.TokenType;
            var asksPropertyName = jsonReader.ValueTextEquals(BookAsksJsonProperty);

            if (_nextReadResult.IsFailure || _tokenType is not JsonTokenType.PropertyName ||
                !asksPropertyName)
            {
                _nextReadResult = new Error(ErrorCodes.EnteringPropertyNameErrorCode,
                    _nextReadResult.Error);
                return;
            }

            _nextReadResult = jsonReader.ReadNext();

            if (_nextReadResult.IsFailure)
            {
                _nextReadResult = new Error(ErrorCodes.EnteringPropertyNameErrorCode,
                    _nextReadResult.Error);
                return;
            }

            _tokenType = jsonReader.TokenType;

            while (_nextReadResult.IsSuccess)
            {
                if (_tokenType is JsonTokenType.StartArray)
                {
                    _matchingBracketsCount++;
                    _nextReadResult = jsonReader.ReadNext();
                    _tokenType      = jsonReader.TokenType;

                    if (_nextReadResult.IsSuccess) continue;

                    _nextReadResult = new Error(ErrorCodes.ReadingBookPropertyErrorCode, _nextReadResult.Error);
                    return;
                }

                var bookEntry = new BookEntry();

                while (_tokenType is JsonTokenType.Number or JsonTokenType.String)
                {
                    ParseBookItemProperties(ref jsonReader, ref bookEntry);

                    _nextReadResult = jsonReader.ReadNext();
                    _tokenType      = jsonReader.TokenType;

                    if (_doubleNumbersCount is 0) break;

                    if (_nextReadResult.IsSuccess) continue;

                    _nextReadResult = new Error(ErrorCodes.ReadingBookPropertyErrorCode, _nextReadResult.Error);
                    return;
                }

                if (_tokenType is JsonTokenType.EndArray)
                {
                    _matchingBracketsCount--;
                    if (_matchingBracketsCount == 0)
                    {
                        break;
                    }

                    _asks.Add(bookEntry);

                    _nextReadResult = jsonReader.ReadNext();
                    _tokenType      = jsonReader.TokenType;

                    if (_nextReadResult.IsSuccess) continue;

                    _nextReadResult = new Error(ErrorCodes.ReadingBookPropertyErrorCode, _nextReadResult.Error);
                    return;
                }
            }
        }

        public void CollectBids(ref Utf8JsonReader jsonReader)
        {
            _nextReadResult = jsonReader.ReadNext();
            _tokenType      = jsonReader.TokenType;
            var bidsPropertyName = jsonReader.ValueTextEquals(BookBidsJsonProperty);

            if (_nextReadResult.IsFailure || _tokenType is not JsonTokenType.PropertyName || !bidsPropertyName)
            {
                _nextReadResult = new Error(ErrorCodes.EnteringPropertyNameErrorCode,
                    _nextReadResult.Error);
                return;
            }

            _nextReadResult = jsonReader.ReadNext();

            if (_nextReadResult.IsFailure)
            {
                _nextReadResult = new Error(ErrorCodes.EnteringPropertyNameErrorCode,
                    _nextReadResult.Error);
                return;
            }

            _tokenType = jsonReader.TokenType;

            while (_nextReadResult.IsSuccess)
            {
                if (_tokenType is JsonTokenType.StartArray)
                {
                    _matchingBracketsCount++;
                    _nextReadResult = jsonReader.ReadNext();
                    _tokenType      = jsonReader.TokenType;

                    if (_nextReadResult.IsSuccess) continue;

                    _nextReadResult = new Error(ErrorCodes.ReadingBookPropertyErrorCode, _nextReadResult.Error);
                    return;
                }

                var bookEntry = new BookEntry();

                while (_tokenType is JsonTokenType.Number or JsonTokenType.String)
                {
                    ParseBookItemProperties(ref jsonReader, ref bookEntry);

                    _nextReadResult = jsonReader.ReadNext();
                    _tokenType      = jsonReader.TokenType;

                    if (_doubleNumbersCount is 0) break;

                    if (_nextReadResult.IsSuccess) continue;

                    _nextReadResult = new Error(ErrorCodes.ReadingBookPropertyErrorCode, _nextReadResult.Error);
                    return;
                }

                if (_tokenType is JsonTokenType.EndArray)
                {
                    _matchingBracketsCount--;
                    if (_matchingBracketsCount == 0)
                    {
                        break;
                    }

                    _bids.Add(bookEntry);

                    _nextReadResult = jsonReader.ReadNext();
                    _tokenType      = jsonReader.TokenType;

                    if (_nextReadResult.IsSuccess) continue;

                    _nextReadResult = new Error(ErrorCodes.ReadingBookPropertyErrorCode, _nextReadResult.Error);
                    return;
                }
            }
        }

        public PairOrderBookEntries Build(string propName)
        {
            var pairOrderBookEntries = new PairOrderBookEntries(
                propName,
                new List<BookEntry>(_asks),
                new List<BookEntry>(_bids)
            );

            _asks.Clear();
            _bids.Clear();

            return pairOrderBookEntries;
        }

        private void ParseBookItemProperties(ref Utf8JsonReader jsonReader, ref BookEntry bookEntry)
        {
            _doubleNumbersCount++;
            switch (_doubleNumbersCount)
            {
                case PricePositionInBookArray:
                    ParseBookAskPrice(ref jsonReader, ref bookEntry);

                    return;
                case VolumePositionInBookArray:
                    ParseBookAskVolume(ref jsonReader, ref bookEntry);

                    return;
                case TimestampPositionInBookArray:
                {
                    _doubleNumbersCount = 0;
                    var timestamp = jsonReader.GetUInt64();
                    bookEntry.Timestamp = timestamp;

                    return;
                }
            }
        }

        private static void ParseBookAskPrice(
            ref Utf8JsonReader jsonReader,
            ref BookEntry      bookAsk
        )
        {
            if (Utf8Parser.TryParse(jsonReader.ValueSpan, out decimal test, out int bytesConsumed)
                && bytesConsumed == jsonReader.ValueSpan.Length)
                bookAsk.Price = test;
            else
            {
                // Pensar numa estratégia de erro
            }
        }

        private static void ParseBookAskVolume(
            ref Utf8JsonReader jsonReader,
            ref BookEntry      bookSpan
        )
        {
            if (Utf8Parser.TryParse(jsonReader.ValueSpan, out decimal volume, out int bytesConsumed)
                && bytesConsumed == jsonReader.ValueSpan.Length)
                bookSpan.Volume = volume;
            else
            {
                // Pensar numa estratégia de erro
            }
        }
    }
}

public record struct BookEntry()
{
    public decimal Price     { get; set; } = 0;
    public decimal Volume    { get; set; } = 0;
    public ulong   Timestamp { get; set; } = 0;
}

public record struct PairOrderBookEntries
{
    public PairOrderBookEntries(
        string          orderBookName,
        List<BookEntry> asks,
        List<BookEntry> bids
    )
    {
        OrderBookName = orderBookName;
        BookAsks      = asks;
        BookBids      = bids;
    }

    public string          OrderBookName { get; init; }
    public List<BookEntry> BookAsks      { get; init; }
    public List<BookEntry> BookBids      { get; init; }
}
