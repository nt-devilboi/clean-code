using System.Collections.Immutable;
using System.Text;

namespace Markdown;

public abstract class TokenMdConverter
{
    private readonly Dictionary<TokenType, string> openTagConvert;
    private readonly Dictionary<TokenType, string> closeTagConvert;
    
    protected TokenMdConverter()
    {
        openTagConvert = new Dictionary<TokenType, string>
        {
            { TokenType.Italic, ItalicOpen() },
            { TokenType.Bold, BoldOpen() },
            { TokenType.Header, HeaderOpen() }
        };

        closeTagConvert = new Dictionary<TokenType, string>()
        {
            { TokenType.Italic, ItalicClose() },
            { TokenType.Bold, BoldClose()},
            { TokenType.Header, HeaderClose()},
        };
    }

    public string Convert(IImmutableList<Token> tokens)
    {
        var stringBuilder = new StringBuilder();
        var stack = new Stack<Token>();

        var prevTokenEndIndex = 0;
        foreach (var token in tokens)
        {
            var countSpace = GetSpaceBetween(token.StartIndex, prevTokenEndIndex);
            stringBuilder.Append(Enumerable.Repeat(' ', countSpace).ToArray());

            if (token is { IsTag: false }) stringBuilder.Append(token.Value);
            
            else if (token is { Type: TokenType.Italic })
            {
                AddTag(stack, stringBuilder, token, TokenType.Italic);
            }

            else if (token is { Type: TokenType.Bold })
            {
                AddTag(stack, stringBuilder, token, TokenType.Bold);
            }
            
            else if (token is { Type: TokenType.Header, IsTag: true })
            {
                stringBuilder.Append(openTagConvert[TokenType.Header]);
                stack.Push(token);
            }

            else if (token is { Type: TokenType.NewLine })
            {
                if (stack.TryPeek(out var outToken) && outToken.Type == TokenType.Header)
                {
                    stack.Pop();
                    stringBuilder.Append(closeTagConvert[TokenType.Header]);
                }

                stringBuilder.Append('\n');
            }
            
            prevTokenEndIndex = token.EndIndex;
        }

        if (stack.TryPeek(out var header) && header.Type == TokenType.Header) stringBuilder.Append(closeTagConvert[TokenType.Header]);

        return stringBuilder.ToString();
    }

    private void AddTag(Stack<Token> stack, StringBuilder stringBuilder, Token token, TokenType tokenType)
    {
        if (stack.TryPeek(out var outToken) && outToken.Type == tokenType)
        {
            stack.Pop();
            stringBuilder.Append(closeTagConvert[tokenType]);
        }
        else
        {
            stack.Push(token);
            stringBuilder.Append(openTagConvert[tokenType]);
        }
    }

    private static int GetSpaceBetween(int curTokenStart, int prevTokenEnd)
    {
        return (curTokenStart == 0 ? 0 : curTokenStart - 1) - prevTokenEnd;
    }
    
    protected abstract string ItalicOpen();
    protected abstract string ItalicClose();

    protected abstract string BoldOpen();
    protected abstract string BoldClose();
    protected abstract string HeaderOpen();
    protected abstract string HeaderClose();

}