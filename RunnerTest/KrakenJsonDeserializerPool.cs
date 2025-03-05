using System.Buffers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CoreAbstractions;
using Microsoft.Extensions.Logging;

namespace RunnerTest;

public class KrakenJsonDeserializerPool
{
    private const  uint                                PricePositionInBookArray     = 1;
    private const  uint                                VolumePositionInBookArray    = 2;
    private const  uint                                TimestampPositionInBookArray = 3;
    private static ILogger<KrakenJsonDeserializerPool> _logger;
    private        Result<bool>                        _nextReadResult;
    private        bool                                _successfulRead;
    private        JsonTokenType                       _tokenType = JsonTokenType.None;
    // private static Error                               _openingObjectError = new Error();

    private static readonly JsonReaderOptions JsonReaderOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling     = JsonCommentHandling.Skip
    };

    readonly List<string> Errors = new();
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
        BookAsks = asks;
        BookBids = bids;
    }

    public string          OrderBookName { get; init; }
    public List<BookEntry> BookAsks      { get; init; }
    public List<BookEntry> BookBids      { get; init; }
}
