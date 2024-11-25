using System.Collections.Immutable;
using System.Text;
using Markdown.Interfaces;
using Markdown.NodeElement;

namespace Markdown;

// todo 
public class MdParser : IParser, ILexer
{
    public IImmutableList<Token> Tokenize(string text)
    {
        var result = new List<Token>();
        var ptr = 0;
        var stack = new Stack<Token>();
        while (ptr < text.Length)
        {
            if ('#' == text[ptr] && ' ' == text[ptr + 1] && (ptr == 0 || text[ptr - 1] == '\n'))
            {
                var headerStart = new Token("# ", TokenType.Header)
                    { StartIndex = ptr, IsTag = true };
                result.Add(headerStart);
                stack.Push(headerStart);
                ptr++;
                ptr++;
            }

            else if (char.IsDigit(text[ptr]))
            {
                var digit = CreateTokenText(TokenType.Digit, ptr, text);
                result.Add(digit);
                ptr += digit.Lenght;
                if (stack.TryPeek(out var token) && token.Type is TokenType.Italic or TokenType.Bold)
                {
                    stack.Pop();
                }
            }

            else if (char.IsLetter(text[ptr]))
            {
                var word = CreateTokenText(TokenType.Word, ptr, text);
                result.Add(word);
                ptr += word.Lenght;
            }

            else if ('\n' == text[ptr])
            {
                result.Add(CreateTokenNewLine(stack, ptr));
           
                ptr++;
            }

            else if (ptr + 1 < text.Length && '_' == text[ptr] && '_' == text[ptr + 1])
            {
                result.Add(CreateTokenBold(stack, ptr));
                ptr += 2;
            }

            else if ('_' == text[ptr])
            {
                result.Add(CreateTokenItalic(stack, ptr, text));
                ptr++;
            }


            else ptr++;
        }


        return ImmutableList.CreateRange(result);

        throw new NotImplementedException();
    }

    private Token CreateTokenNewLine(Stack<Token> stack, int ptr)
    {
        while (stack.TryPeek(out var token) && token.Type != TokenType.Header)
        {
            stack.Pop();
        }


        return new Token("\n", TokenType.NewLine)
            { StartIndex = ptr, IsTag = LastIsHeader(stack) };
    }

    private static Token CreateTokenBold(Stack<Token> stack, int ptr)
    {
        if (stack.TryPeek(out var token) && token.Type is TokenType.Bold)
        {
            stack.Pop();
            // тип если они рядом, то это не tag
            if (token.EndIndex + 1 == ptr) return new Token("__", TokenType.Bold) { StartIndex = ptr };

            token.IsTag = true;
            return new Token("__", TokenType.Bold) { StartIndex = ptr, IsTag = true };
        }

        if (stack.TryPeek(out token) && token.Type is TokenType.Italic)
        {
            var prevToken = stack.Pop();
            if (stack.TryPeek(out var tokenInner) && tokenInner.Type is TokenType.Bold)
            {
                tokenInner = stack.Pop();
                tokenInner.IsTag = true;
                return new Token("__", TokenType.Bold) { StartIndex = ptr, IsTag = true };
            }

            tokenInner = new Token("__", TokenType.Bold) { StartIndex = ptr };
            stack.Push(prevToken);
            stack.Push(tokenInner);
            return tokenInner;
        }


        var bold = new Token("__", TokenType.Bold) { StartIndex = ptr };
        stack.Push(bold);
        return bold;
    }

    private static Token CreateTokenItalic(Stack<Token> stack, int ptr, string text)
    {
        if (stack.TryPeek(out var token) && token.Type is TokenType.Italic)
        {
            stack.Pop();
            // тип если они рядом, то это не tag
            if (token.EndIndex + 1 == ptr) return new Token("_", TokenType.Italic) { StartIndex = ptr };
            if (char.IsWhiteSpace(text[ptr - 1])) return new Token("_", TokenType.Italic) { StartIndex = ptr };
            
            token.IsTag = true;
            return new Token("_", TokenType.Italic) { StartIndex = ptr, IsTag = true };
        }

        if (stack.TryPeek(out var tokenPrev) && tokenPrev.Type is TokenType.Bold)
        {
            var prevBold = stack.Pop();
            if (stack.TryPeek(out token) && token.Type is TokenType.Italic)
            {
                stack.Pop();
                return new Token("_", TokenType.Italic) { StartIndex = ptr };
            }

            stack.Push(prevBold);
        }

        var italic = new Token("_", TokenType.Italic) { StartIndex = ptr };

        if (ptr + 1 < text.Length && !char.IsWhiteSpace(text[ptr + 1])) stack.Push(italic);
        return italic;
    }

    private bool LastIsHeader(Stack<Token> stack)
    {
        if (!stack.TryPeek(out var token) || token.Type != TokenType.Header) return false;
        stack.Pop();
        return true;
    }

    private Token CreateTokenText(TokenType tokenType, int ptr, string text)
    {
        var sb = new StringBuilder();
        var start = ptr;
        while (ptr < text.Length && IsSymbolHaveType(tokenType, ptr, text))
        {
            sb.Append(text[ptr++]);
        }

        return new Token(sb.ToString(), tokenType) { StartIndex = start };
    }

    private static bool IsSymbolHaveType(TokenType tokenType, int ptr, string text)
    {
        return char.IsLetter(text[ptr]) && tokenType == TokenType.Word ||
               char.IsDigit(text[ptr]) && tokenType == TokenType.Digit;
    }


    public IElementNode GenerateTree(IReadOnlyList<Token> tokens)
    {
        var node = new ElementNodeNewLine();
        var stack = new Stack<IElementNode>();
        stack.Push(node);
        foreach (var token in tokens)
        {
            var prevNode = node;

            if (token.Type == TokenType.Header)
            {
                prevNode.NextLine = new NodeHeader();
            }
        }

        return node.NextLine;
    }

    public string ToHtml(IImmutableList<Token> tokens)
    {
        var stringBuilder = new StringBuilder();
        var stack = new Stack<Token>();

        var countSpace = 0;
        var prevTokenEndIndex = 0;
        foreach (var token in tokens)
        {
            countSpace = GetSpaceBeetween(token.StartIndex, prevTokenEndIndex);
            stringBuilder.Append(Enumerable.Repeat(' ', countSpace).ToArray());

            if (token is { IsTag: false }) stringBuilder.Append(token.Value);


            else if (token is { Type: TokenType.Italic })
            {
                if (stack.TryPeek(out var outToken) && outToken.Type is TokenType.Italic)
                {
                    stack.Pop();
                    stringBuilder.Append("</em>");
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

    private static int GetSpaceBeetween(int curTokenStart, int prevTokenEnd)
    {
        return (curTokenStart == 0 ? 0 : curTokenStart - 1) - prevTokenEnd;
    }
}