using Microsoft.VisualBasic;

namespace Markdown.NodeElement;

public class PairToken(Token start, Token end)
{
    public Token Start { get; } = start;
    public Token End { get; } = end;

    public bool IntersectWith(PairToken token)
    {
        return End.StartIndex > token.Start.StartIndex &&
               End.StartIndex < token.End.StartIndex &&
               Start.StartIndex < token.Start.StartIndex ||
               token.End.StartIndex > Start.StartIndex &&
               token.End.StartIndex < End.StartIndex &&
               token.Start.StartIndex < Start.StartIndex;
    }
    
   
    public bool Contain(PairToken token)
    {
        return Start.StartIndex > token.Start.StartIndex &&
               End.StartIndex < token.End.StartIndex;
    }
    
}