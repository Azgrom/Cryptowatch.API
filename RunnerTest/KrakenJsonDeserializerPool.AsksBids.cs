using System.Text.Json;

namespace RunnerTest;

public class KrakenJsonDeserializerPool2
{
    public ref struct AsksBids
    {
        Utf8JsonReader        utf8JsonReader;
        bool                  successfulRead;
        JsonTokenType         tokenType;
        uint                  matchingBracketsCount;
        uint                  doubleNumbersCount;
        ref PairOrderBookSpan pairOrderBookSpan;
    }
}

public static class X
{
    private static void ProcessDecimal(uint askIndex, ref Utf8JsonReader reader, ref Span<PairBookAsk> asksSpan)
        {
            if (decimal.TryParse(reader.ValueSpan, out var decimalValue))
            {
                asksSpan[(int)askIndex].Price = decimalValue;
            }
            else
            {
                if (askIndex is 0)
                {
                    // TODO
                }
                else
                {
                    var previousPrice = asksSpan[(int)askIndex - 1].Price;
                    asksSpan[(int)askIndex].Price = previousPrice;
                }
            }
    }
}