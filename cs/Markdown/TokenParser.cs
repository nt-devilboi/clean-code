using System.Collections.Immutable;
using System.Text;
using Markdown.Interfaces;

namespace Markdown;

public class TokenParser : IParser
{
    public string Html(IImmutableList<Token> tokens)
    {
        var stringBuilder = new StringBuilder();
        var stack = new Stack<Token>();

        int countSpace;
        var prevTokenEndIndex = 0;
        foreach (var token in tokens)
        {
            countSpace = GetSpaceBetween(token.StartIndex, prevTokenEndIndex);
            stringBuilder.Append(Enumerable.Repeat(' ', countSpace).ToArray());

            if (token is { IsTag: false }) stringBuilder.Append(token.Value);


            else if (token is { Type: TokenType.Italic })
            {
                if (stack.TryPeek(out var outToken) && outToken.Type is TokenType.Italic)
                {
                    stack.Pop();
                    stringBuilder
                        .Append("</em>"); // можно вынести за интерфейс, чтоб не привязывать этот класс именно к html.
                }
                else
                {
                    stack.Push(token);
                    stringBuilder.Append("<em>");
                }
            }

            else if (token is { Type: TokenType.Bold })
            {
                if (stack.TryPeek(out var outToken) && outToken.Type is TokenType.Bold)
                {
                    stack.Pop();
                    stringBuilder.Append("</strong>");
                }
                else
                {
                    stack.Push(token);
                    stringBuilder.Append("<strong>");
                }
            }
            else if (token is { Type: TokenType.Header, IsTag: true })
            {
                stringBuilder.Append("<h1>");
                stack.Push(token);
            }

            else if (token is { Type: TokenType.NewLine })
            {
                if (stack.TryPeek(out var outToken) && outToken.Type == TokenType.Header)
                {
                    stack.Pop();
                    stringBuilder.Append("</h1>");
                }

                stringBuilder.Append('\n');
            }


            prevTokenEndIndex = token.EndIndex;
        }

        if (stack.TryPeek(out var header) && header.Type == TokenType.Header) stringBuilder.Append("</h1>");

        return stringBuilder.ToString();
    }
    
    private static int GetSpaceBetween(int curTokenStart, int prevTokenEnd)
    {
        return (curTokenStart == 0 ? 0 : curTokenStart - 1) - prevTokenEnd;
    }
}