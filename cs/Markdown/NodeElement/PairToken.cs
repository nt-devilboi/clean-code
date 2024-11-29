using Microsoft.VisualBasic;

namespace Markdown.NodeElement;

public class PairToken(Token start, Token end)
{
    public Token Start { get; init; } = start;
    public Token End { get; init; } = end;

    public bool IntersectWith(PairToken token)
    {
        return End.StartIndex > token.Start.StartIndex &&
               End.StartIndex < token.End.StartIndex &&
               Start.StartIndex < token.Start.StartIndex ||
               token.End.StartIndex > Start.StartIndex &&
               token.End.StartIndex < End.StartIndex &&
               token.Start.StartIndex < Start.StartIndex;
    }

    public bool IsInside(PairToken token)
    {
        return Start.StartIndex > token.Start.StartIndex &&
               End.StartIndex < token.End.StartIndex;
    }

    public void AsTag()
    {
        start.IsTag = true;
        end.IsTag = true;
    }
}