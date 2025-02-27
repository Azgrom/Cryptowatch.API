using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreAbstractions;
using Microsoft.Extensions.Logging;

namespace RunnerTest;

public class OhlcJsonConverter : JsonConverter<Result<Ohlc>>
{
    private const uint          PricePositionInBookArray      = 1;
    private const uint          VolumePositionInBookArray     = 2;
    private const uint          TimestampPositionInBookArray  = 3;
    private const string        StartReadingErrorCode         = "StartReadingError";
    private const string        ReadingTokenErrorCode         = "ReadingTokenError";
    private const string        UnexpectedTokenErrorCode      = "UnexpectedTokenError";
    private const string        EnteringObjectErrorCode       = "EnteringObjectError";
    private const string        EnteringArrayErrorCode        = "EnteringArrayError";
    private const string        UnknownPropertyErrorCode      = "UnknownPropertyError";
    private const string        EnteringPropertyNameErrorCode = "EnteringPropertyNameError";
    private const string        ReadingBookPropertyErrorCode  = "ReadingBookPropertyError";
    private       Result        _nextReadResult               = Result.Failure();
    private       JsonTokenType _tokenType                    = JsonTokenType.None;

    public override Result<Ohlc> Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
    {
        Result<Ohlc> result = Result.Success(new Ohlc());

        _nextReadResult = jsonReader.ReadNext();
        if (_nextReadResult.IsSuccess)
        {
            _tokenType = jsonReader.TokenType;
        }
        else
        {
            result = Result.Failure<Ohlc>(new Error(StartReadingErrorCode, _nextReadResult.Error));
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
                result = Result.Failure<Ohlc>(new Error(EnteringObjectErrorCode, _nextReadResult.Error));
            }
        }
        else
        {
            // TODO Failure result for JsonTokenType different of StartObject
        }

        List<string> errors = [];
        while (_nextReadResult.IsSuccess && _tokenType is JsonTokenType.PropertyName)
        {
            var errorPropertyName  = jsonReader.ValueTextEquals("error");
            var resultPropertyName = jsonReader.ValueTextEquals("result");

            if (!errorPropertyName && !resultPropertyName && jsonReader.HasValueSequence)
            {
                result = UnknownPropertyResult(ref jsonReader);
            }

            if (errorPropertyName && result.IsSuccess)
            {
                var errorSweep = ErrorSweep(ref jsonReader);

                if (errorSweep.IsFailure)
                {
                    result = Result.Failure<Ohlc>(errorSweep.Error);
                }
                else
                {
                    if (errorSweep.Value.Count > 0)
                    {
                        result.Value.Errors = errorSweep.Value;
                    }
                }
            }

            if (resultPropertyName)
            {
                ResultSweep(ref jsonReader);
            }

            _nextReadResult = jsonReader.ReadNext();
            if (_nextReadResult.IsFailure)
                return Result.Failure<Ohlc>(new Error(ReadingTokenErrorCode, _nextReadResult.Error));

            _tokenType = jsonReader.TokenType;
        }

        if (_tokenType is JsonTokenType.EndObject) return result;

        return Result.Failure<Ohlc>(new Error(UnexpectedTokenErrorCode, "Unexpected End of Error Array"));
    }

    public override void Write(Utf8JsonWriter writer, Result<Ohlc> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private static Result<Ohlc> UnknownPropertyResult(ref Utf8JsonReader jsonReader)
    {
        var propName = Encoding.UTF8.GetString(jsonReader.ValueSequence);
        return new Error(UnknownPropertyErrorCode, propName);
    }

    private Result<List<string>> ErrorSweep(ref Utf8JsonReader jsonReader)
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

        if (_tokenType is JsonTokenType.EndArray) return Result.Success(errors);

        return new Error(UnexpectedTokenErrorCode, "Unexpected End of Error Array");
    }


    private Result<List<PairOrderBookSpan>> ResultSweep(ref Utf8JsonReader jsonReader)
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

        var pairOrderBookSpan = new List<PairOrderBookSpan>(1);
        _nextReadResult = jsonReader.ReadNext();
        while (_nextReadResult.IsSuccess)
        {
            _tokenType = jsonReader.TokenType;

            if (_nextReadResult.IsFailure || _tokenType != JsonTokenType.PropertyName)
            {
                error = new Error(EnteringPropertyNameErrorCode,
                    _nextReadResult.Error);
            }

            var propName      = jsonReader.GetString();

            _nextReadResult = jsonReader.ReadNext();
            _tokenType      = jsonReader.TokenType;

            if (_nextReadResult.IsFailure || _tokenType is not JsonTokenType.StartObject)
            {
                error = new Error(EnteringObjectErrorCode, _nextReadResult.Error);
            }

            var bookWorker        = BookBuilder.Create(propName);
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

            var orderBookSpan = collectBidsResult.Value.Build();

            pairOrderBookSpan.Add(orderBookSpan);
        }

        if (error != Error.None)
        {
            return error;
        }

        return Result.Success(pairOrderBookSpan);
    }

    public class BookBuilder
    {
        public static BookWorker Create(string pairName) => new BookWorker(pairName);

        public class BookWorker :
            IBookBuilder,
            IAsksBookBuilder,
            IBidsBookBuilder
        {
            private          uint              matchingBracketsCount = 0;
            private          uint              doubleNumbersCount    = 0;
            private          uint              asksCount             = 0;
            private          uint              bidsCount             = 0;
            private readonly PairOrderBookSpan _pairOrderBookSpan;
            private          Result            _nextReadResult = Result.Failure();
            private          JsonTokenType     _tokenType      = JsonTokenType.None;

            public BookWorker(string pairName) { _pairOrderBookSpan = new PairOrderBookSpan(pairName); }

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
                    if (ParseAskProperties(ref jsonReader, ref error)) continue;

                    break;
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
                    if (ParseBidsProperties(ref jsonReader, ref error)) continue;

                    break;
                }

                if (error != Error.None) return error;

                return this;
            }

            private bool ParseBidsProperties(ref Utf8JsonReader jsonReader, ref Error error)
            {
                _tokenType = jsonReader.TokenType;

                if (_tokenType is JsonTokenType.StartArray)
                {
                    matchingBracketsCount++;
                    return true;
                }

                if (_tokenType is JsonTokenType.Number or JsonTokenType.String)
                {
                    doubleNumbersCount++;

                    if (doubleNumbersCount is PricePositionInBookArray)
                    {
                        if (decimal.TryParse(jsonReader.ValueSpan, out var price))
                        {
                            _pairOrderBookSpan.BidsSpan[(int)bidsCount].Price = price;
                        }
                        else
                        {
                            if (bidsCount is 0)
                            {
                                // TODO
                            }
                            else
                            {
                                var previousPrice = _pairOrderBookSpan.BidsSpan[(int)bidsCount - 1].Price;
                                _pairOrderBookSpan.BidsSpan[(int)bidsCount].Price = previousPrice;
                            }
                        }

                        return true;
                    }

                    if (doubleNumbersCount is VolumePositionInBookArray)
                    {
                        if (decimal.TryParse(jsonReader.ValueSpan, out var volume))
                        {
                            _pairOrderBookSpan.BidsSpan[(int)bidsCount].Volume = volume;
                        }
                        else
                        {
                            if (bidsCount is 0)
                            {
                                // TODO
                            }
                            else
                            {
                                var previousVolume = _pairOrderBookSpan.BidsSpan[(int)bidsCount - 1].Volume;
                                _pairOrderBookSpan.BidsSpan[(int)bidsCount].Volume = previousVolume;
                            }
                        }

                        return true;
                    }

                    if (doubleNumbersCount is TimestampPositionInBookArray)
                    {
                        doubleNumbersCount = 0;
                        var timestamp = jsonReader.GetUInt64();
                        _pairOrderBookSpan.AsksSpan[(int)bidsCount].Timestamp = timestamp;

                        return true;
                    }
                }

                if (_tokenType is JsonTokenType.EndArray)
                {
                    matchingBracketsCount--;

                    if (matchingBracketsCount == 0)
                    {
                        return false;
                    }

                    bidsCount++;
                }

                _nextReadResult = jsonReader.ReadNext();
                if (_nextReadResult.IsFailure)
                {
                    error = new Error(ReadingBookPropertyErrorCode, _nextReadResult.Error);
                }

                return false;
            }

            public PairOrderBookSpan Build() => _pairOrderBookSpan;

            private bool ParseAskProperties(ref Utf8JsonReader jsonReader, ref Error error)
            {
                _tokenType = jsonReader.TokenType;

                if (_tokenType is JsonTokenType.StartArray)
                {
                    matchingBracketsCount++;
                    return true;
                }

                var bookSpan = _pairOrderBookSpan.AsksSpan;

                if (_tokenType is JsonTokenType.Number or JsonTokenType.String)
                {
                    doubleNumbersCount++;

                    if (doubleNumbersCount is PricePositionInBookArray)
                    {
                        ParseBookAskPrice(ref jsonReader, ref bookSpan, (int)asksCount);

                        return true;
                    }

                    if (doubleNumbersCount is VolumePositionInBookArray)
                    {
                        ParseBookAskVolume(ref jsonReader, ref bookSpan, (int)asksCount);

                        return true;
                    }

                    if (doubleNumbersCount is TimestampPositionInBookArray)
                    {
                        doubleNumbersCount = 0;
                        var timestamp = jsonReader.GetUInt64();
                        bookSpan[(int)asksCount].Timestamp = timestamp;

                        return true;
                    }
                }

                if (_tokenType is JsonTokenType.EndArray)
                {
                    matchingBracketsCount--;

                    if (matchingBracketsCount == 0)
                    {
                        return false;
                    }

                    asksCount++;
                }

                _nextReadResult = jsonReader.ReadNext();
                if (_nextReadResult.IsFailure)
                {
                    error = new Error(ReadingBookPropertyErrorCode, _nextReadResult.Error);
                }

                return false;
            }

            private static void ParseBookAskPrice(
                ref Utf8JsonReader    jsonReader,
                ref Span<PairBookAsk> bookAsk,
                int                   count
            )
            {
                if (decimal.TryParse(jsonReader.ValueSpan, out var price1))
                    bookAsk[count].Price = price1;
                else
                {
                    if (count is 0)
                    {
                        // TODO
                    }
                    else
                    {
                        var previousPrice = bookAsk[count - 1].Price;
                        bookAsk[count].Price = previousPrice;
                    }
                }
            }

            private static void ParseBookAskVolume(
                ref Utf8JsonReader    jsonReader,
                ref Span<PairBookAsk> bookSpan,
                int                   count
            )
            {
                if (decimal.TryParse(jsonReader.ValueSpan, out var volume))
                    bookSpan[count].Volume = volume;
                else
                {
                    if (count is 0)
                    {
                        // TODO
                    }
                    else
                    {
                        var previousVolume = bookSpan[count - 1].Volume;
                        bookSpan[count].Volume = previousVolume;
                    }
                }
            }
        }
    }

    public interface IAsksBookBuilder
    {
        Result<IBidsBookBuilder> CollectAsks(ref Utf8JsonReader jsonReader);
    }

    public interface IBidsBookBuilder
    {
        Result<IBookBuilder> CollectBids(ref Utf8JsonReader jsonReader);
    }

    public interface IBookBuilder
    {
        PairOrderBookSpan Build();
    }
}
