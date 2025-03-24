using System.Text.Json;
using CoreAbstractions;
using Microsoft.Extensions.Logging;

namespace Kraken.REST.API.Client;

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
