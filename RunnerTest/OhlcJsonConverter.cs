using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;
using Microsoft.Extensions.Logging;

namespace RunnerTest;

public sealed record Ohlc
{
    private const uint   PricePositionInBookArray      = 1;
    private const uint   VolumePositionInBookArray     = 2;
    private const uint   TimestampPositionInBookArray  = 3;
    private const string StartReadingErrorCode         = "StartReadingError";
    private const string ReadingTokenErrorCode         = "ReadingTokenError";
    private const string UnexpectedTokenErrorCode      = "UnexpectedTokenError";
    private const string EnteringObjectErrorCode       = "EnteringObjectError";
    private const string EnteringArrayErrorCode        = "EnteringArrayError";
    private const string UnknownPropertyErrorCode      = "UnknownPropertyError";
    private const string EnteringPropertyNameErrorCode = "EnteringPropertyNameError";
    private const string ReadingBookPropertyErrorCode  = "ReadingBookPropertyError";

    public Ohlc(string[] errors, List<PairOrderBookEntries> pairOrderBookSpan)
    {
        Errors            = errors;
        PairOrderBookSpan = pairOrderBookSpan;
    }

    public string[]               Errors            { get; private set; }
    public List<PairOrderBookEntries> PairOrderBookSpan { get; private set; }

    public static Result<Ohlc> FromJson(ref Utf8JsonReader jsonReader) =>
        new JsonConverter().Read(ref jsonReader, typeof(Ohlc), null);

    private class JsonConverter : JsonConverter<Result<Ohlc>>
    {
        private Result<bool>  _nextReadResult = Error.NullValue;
        private JsonTokenType _tokenType      = JsonTokenType.None;

        public override Result<Ohlc> Read(
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
                _nextReadResult = new Error(StartReadingErrorCode, _nextReadResult.Error);
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
                    _nextReadResult = new Error(EnteringObjectErrorCode, _nextReadResult.Error);
                }
            }
            else
            {
                // TODO Failure result for JsonTokenType different of StartObject
            }

            var errors            = Array.Empty<string>();
            var          errorPropertyName = jsonReader.ValueTextEquals("error");
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
            if (_nextReadResult.IsFailure) return new Error(ReadingTokenErrorCode, _nextReadResult.Error);

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
            if (_nextReadResult.IsFailure) return new Error(ReadingTokenErrorCode, _nextReadResult.Error);

            _tokenType = jsonReader.TokenType;

            if (!errorPropertyName && !resultPropertyName && jsonReader.HasValueSequence &&
                pairOrderBookEntries is null)
            {
                _nextReadResult = UnknownPropertyResult(ref jsonReader);
            }

            if (_tokenType is JsonTokenType.EndObject &&
                pairOrderBookEntries is not null) return new Ohlc(errors, pairOrderBookEntries);

            return new Error(UnexpectedTokenErrorCode, $"Unexpected End of Error Array: {_nextReadResult.Error}");
        }

        public override void Write(Utf8JsonWriter writer, Result<Ohlc> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        private static Error UnknownPropertyResult(ref Utf8JsonReader jsonReader)
        {
            var propName = Encoding.UTF8.GetString(jsonReader.ValueSequence);
            return new Error(UnknownPropertyErrorCode, propName);
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
                return new Error(StartReadingErrorCode, _nextReadResult.Error);
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
                    return new Error(EnteringArrayErrorCode, _nextReadResult.Error);
                }
            }

            var errors = new List<string>();
            while (_nextReadResult.IsSuccess && _tokenType is JsonTokenType.String)
            {
                var s        = jsonReader.GetString();
                var position = jsonReader.Position.GetInteger();
                Debug.Assert(s == null, $"{nameof(JsonTokenType.String)} == null a position {position}");
                Debug.Assert(s != null, $"{nameof(JsonTokenType.String)} != null a position {position}");
                errors.Add(s);

                _nextReadResult = jsonReader.ReadNext();
                if (_nextReadResult.IsSuccess)
                {
                    _tokenType = jsonReader.TokenType;
                }
                else
                {
                    return new Error(ReadingTokenErrorCode, _nextReadResult.Error);
                }
            }

            if (_tokenType is JsonTokenType.EndArray) return errors.ToArray();

            return new Error(UnexpectedTokenErrorCode, "Unexpected End of Error Array");
        }


        private Result<List<PairOrderBookEntries>> ResultSweep(ref Utf8JsonReader jsonReader)
        {
            var error = Error.None;
            _nextReadResult = jsonReader.ReadNext();
            if (_nextReadResult.IsFailure)
            {
                error = new Error(StartReadingErrorCode, _nextReadResult.Error);
            }

            _tokenType = jsonReader.TokenType;
            if (_tokenType is not JsonTokenType.StartObject)
            {
                error = new Error(EnteringObjectErrorCode, _nextReadResult.Error);
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
                    error = new Error(EnteringPropertyNameErrorCode,
                        _nextReadResult.Error);
                }

                var propName = jsonReader.GetString();

                _nextReadResult = jsonReader.ReadNext();
                _tokenType      = jsonReader.TokenType;

                if (_nextReadResult.IsFailure || _tokenType is not JsonTokenType.StartObject)
                {
                    error = new Error(EnteringObjectErrorCode, _nextReadResult.Error);
                }

                var bookWorker        = BookBuilder.Create();
                var collectAsksResult = bookWorker.CollectAsks(ref jsonReader);

                if (collectAsksResult.IsFailure)
                {
                    // todo
                }

                var collectBidsResult = collectAsksResult.Value.CollectBids(ref jsonReader);

                if (collectBidsResult.IsFailure)
                {
                    // todo
                }

                var orderBookSpan = collectBidsResult.Value.Build(propName);

                pairOrderBookSpan.Add(orderBookSpan);
            }

            if (error != Error.None)
            {
                return error;
            }

            return pairOrderBookSpan;
        }
    }

    private class BookBuilder
    {
        public static BookWorker Create() => new();

        public class BookWorker :
            IBookBuilder,
            IAsksBookBuilder,
            IBidsBookBuilder
        {
            private       uint            _matchingBracketsCount;
            private       uint            _doubleNumbersCount;
            private       uint            _asksCount;
            private       uint            _bidsCount;
            private const int             BookMaximumSize = 100;
            private       List<BookEntry> _asks           = new List<BookEntry>(BookMaximumSize);
            private       List<BookEntry> _bids           = new List<BookEntry>(BookMaximumSize);
            private       Result<bool>    _nextReadResult = Error.NullValue;
            private       JsonTokenType   _tokenType      = JsonTokenType.None;

            public Result<IBidsBookBuilder> CollectAsks(ref Utf8JsonReader jsonReader)
            {
                _nextReadResult = jsonReader.ReadNext();
                _tokenType      = jsonReader.TokenType;
                var asksPropertyName = jsonReader.ValueTextEquals("asks");
                var error            = Error.None;

                if (_nextReadResult.IsFailure || _tokenType is not JsonTokenType.PropertyName ||
                    !asksPropertyName)
                {
                    error = new Error(EnteringPropertyNameErrorCode,
                        _nextReadResult.Error);
                }

                _nextReadResult = jsonReader.ReadNext();

                if (_nextReadResult.IsFailure)
                    error = new Error(EnteringPropertyNameErrorCode,
                        _nextReadResult.Error);

                while (_nextReadResult.IsSuccess)
                {
                    _tokenType = jsonReader.TokenType;
                    var bookEntry = new BookEntry();

                    ParseAskProperties(ref jsonReader, ref bookEntry);

                    _asks.Add(bookEntry);

                    if (_matchingBracketsCount == 0)
                    {
                        _asksCount--;
                        break;
                    }

                    _nextReadResult = jsonReader.ReadNext();
                    if (_nextReadResult.IsFailure)
                    {
                        error = new Error(ReadingBookPropertyErrorCode, _nextReadResult.Error);
                    }
                }

                if (error != Error.None) return error;

                return this;
            }

            public Result<IBookBuilder> CollectBids(ref Utf8JsonReader jsonReader)
            {
                _nextReadResult = jsonReader.ReadNext();
                _tokenType      = jsonReader.TokenType;
                var bidsPropertyName = jsonReader.ValueTextEquals("bids");
                var error            = Error.None;

                if (_nextReadResult.IsFailure || _tokenType is not JsonTokenType.PropertyName || !bidsPropertyName)
                {
                    error = new Error(EnteringPropertyNameErrorCode,
                        _nextReadResult.Error);
                }

                _nextReadResult = jsonReader.ReadNext();

                if (_nextReadResult.IsFailure)
                {
                    error = new Error(EnteringPropertyNameErrorCode,
                        _nextReadResult.Error);
                }

                while (_nextReadResult.IsSuccess)
                {
                    _tokenType = jsonReader.TokenType;
                    var bookEntry = new BookEntry();

                    ParseBidsProperties(ref jsonReader, ref bookEntry);

                    _bids.Add(bookEntry);

                    if (_matchingBracketsCount == 0)
                    {
                        _bidsCount--;
                        break;
                    }

                    _nextReadResult = jsonReader.ReadNext();
                    if (_nextReadResult.IsFailure)
                    {
                        error = new Error(ReadingBookPropertyErrorCode, _nextReadResult.Error);
                    }
                }

                if (error != Error.None) return error;

                return this;
            }

            private void ParseBidsProperties(ref Utf8JsonReader jsonReader, ref BookEntry bookEntry)
            {
                if (_tokenType is JsonTokenType.StartArray)
                {
                    _matchingBracketsCount++;
                    return;
                }

                if (_tokenType is JsonTokenType.Number or JsonTokenType.String)
                {
                    _doubleNumbersCount++;

                    if (_doubleNumbersCount is PricePositionInBookArray)
                    {
                        ParseBookBidPrice(ref jsonReader, ref bookEntry);

                        return;
                    }

                    if (_doubleNumbersCount is VolumePositionInBookArray)
                    {
                        ParseBookBidVolume(ref jsonReader, ref bookEntry);

                        return;
                    }

                    if (_doubleNumbersCount is TimestampPositionInBookArray)
                    {
                        _doubleNumbersCount = 0;
                        var timestamp = jsonReader.GetUInt64();
                        bookEntry.Timestamp = timestamp;

                        return;
                    }
                }


                if (_tokenType is JsonTokenType.EndArray)
                {
                    _matchingBracketsCount--;
                    _bidsCount++;
                }
            }

            private void ParseBookBidVolume(
                ref Utf8JsonReader jsonReader,
                ref BookEntry      booksSpan
            )
            {
                if (decimal.TryParse(jsonReader.ValueSpan, out var volume))
                {
                    booksSpan.Volume = volume;
                }
                else
                {
                    // Pensar numa estratégia de erro
                }
            }

            private void ParseBookBidPrice(
                ref Utf8JsonReader jsonReader,
                ref BookEntry      booksSpan
            )
            {
                if (decimal.TryParse(jsonReader.ValueSpan, out var price))
                {
                    booksSpan.Price = price;
                }
                else
                {
                    // Pensar numa estratégia de erro
                }
            }

            public PairOrderBookEntries Build(string propName) => new(propName, _asks, _bids);

            private void ParseAskProperties(ref Utf8JsonReader jsonReader, ref BookEntry bookEntry)
            {
                if (_tokenType is JsonTokenType.StartArray)
                {
                    _matchingBracketsCount++;
                    return;
                }

                if (_tokenType is JsonTokenType.Number or JsonTokenType.String)
                {
                    _doubleNumbersCount++;

                    if (_doubleNumbersCount is PricePositionInBookArray)
                    {
                        ParseBookAskPrice(ref jsonReader, ref bookEntry);

                        return;
                    }

                    if (_doubleNumbersCount is VolumePositionInBookArray)
                    {
                        ParseBookAskVolume(ref jsonReader, ref bookEntry);

                        return;
                    }

                    if (_doubleNumbersCount is TimestampPositionInBookArray)
                    {
                        _doubleNumbersCount = 0;
                        var timestamp = jsonReader.GetUInt64();
                        bookEntry.Timestamp = timestamp;

                        return;
                    }
                }

                if (_tokenType is JsonTokenType.EndArray)
                {
                    _matchingBracketsCount--;
                    _asksCount++;
                }
            }

            private static void ParseBookAskPrice(
                ref Utf8JsonReader jsonReader,
                ref BookEntry      bookAsk
            )
            {
                if (decimal.TryParse(jsonReader.ValueSpan, out var price1))
                    bookAsk.Price = price1;
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
                if (decimal.TryParse(jsonReader.ValueSpan, out var volume))
                    bookSpan.Volume = volume;
                else
                {
                    // Pensar numa estratégia de erro
                }
            }
        }
    }

    private interface IAsksBookBuilder
    {
        Result<IBidsBookBuilder> CollectAsks(ref Utf8JsonReader jsonReader);
    }

    private interface IBidsBookBuilder
    {
        Result<IBookBuilder> CollectBids(ref Utf8JsonReader jsonReader);
    }

    private interface IBookBuilder
    {
        PairOrderBookEntries Build(string propName);
    }
}
