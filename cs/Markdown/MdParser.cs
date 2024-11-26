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
        var stack = new TokenTree();
        while (ptr < text.Length)
        {
            if ('#' == text[ptr] && ' ' == text[ptr + 1] && (ptr == 0 || text[ptr - 1] == '\n'))
            {
                var headerStart = new Token("# ", TokenType.Header)
                    { StartIndex = ptr, IsTag = true };
                result.Add(headerStart);
                stack.Push(headerStart);
                stack.HaveFreeOpenHeader = true;
                ptr++;
                ptr++;
            }

            else if (char.IsDigit(text[ptr]))
            {
                var digit = CreateTokenText(TokenType.Digit, ptr, text);
                result.Add(digit);
                ptr += digit.Lenght;
                if (stack.TryPeekLeaf(out var token) && token.Type is TokenType.Italic or TokenType.Bold)
                {
                    stack.PopLeaf();
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
                stack = new TokenTree();
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

    private Token CreateTokenNewLine(TokenTree nodes, int ptr)
    {
        nodes.Clear();


        return new Token("\n", TokenType.NewLine) { StartIndex = ptr, IsTag = nodes.HaveFreeOpenHeader };
    }

    private static Token CreateTokenBold(TokenTree stack, int ptr)
    {
        if (stack.TryPeekLeaf(out var token) && token.Type is TokenType.Bold)
        {
            stack.PopLeaf();
            // тип если они рядом, то это не tag
            if (token.EndIndex + 1 == ptr) return new Token("__", TokenType.Bold) { StartIndex = ptr };
            
            token.IsTag = true;
            var tokenCloseBold = new Token("__", TokenType.Bold) { StartIndex = ptr, IsTag = true };
            stack.CompletedBold = (token, tokenCloseBold);
            return tokenCloseBold;
        }

        if (stack.TryPeekLeaf(out token) && token.Type is TokenType.Italic)
        {
            var prevToken = stack.PopLeaf();
            if (stack.TryPeekLeaf(out var tokenInner) && tokenInner.Type is TokenType.Bold)
            {
                tokenInner = stack.PopLeaf();
                tokenInner.IsTag = true;
                stack.HaveFreeOpenBold = false;
                return new Token("__", TokenType.Bold) { StartIndex = ptr, IsTag = true };
            }

            var tokenBold = new Token("__", TokenType.Bold) { StartIndex = ptr };
            stack.HaveFreeOpenBold = true;
            stack.Push(prevToken);
            stack.Push(tokenBold);
            return tokenBold;
        }


        var bold = new Token("__", TokenType.Bold) { StartIndex = ptr };
        stack.Push(bold);
        stack.HaveFreeOpenBold = true;
        return bold;
    }

    private static Token CreateTokenItalic(TokenTree stack, int ptr, string text)
    {
        if (stack.TryPeekLeaf(out var token) && token.Type is TokenType.Italic)
        {
            stack.PopLeaf();
            // тип если они рядом, то это не tag
            if (token.EndIndex + 1 == ptr) return new Token("_", TokenType.Italic) { StartIndex = ptr };
            if (char.IsWhiteSpace(text[ptr - 1])) return new Token("_", TokenType.Italic) { StartIndex = ptr };
            if (stack.InHaveBold)
            {
                stack.CompletedBold.Item1.IsTag = false;
                stack.CompletedBold.Item2.IsTag = false;
            }
            token.IsTag = true;
            stack.HaveFreeOpenItalic = false;
            return new Token("_", TokenType.Italic) { StartIndex = ptr, IsTag = true };
        }

        if (stack.TryPeekLeaf(out var tokenPrev) && tokenPrev.Type is TokenType.Bold)
        {
            var prevBold = stack.PopLeaf();
            if (stack.TryPeekLeaf(out token) && token.Type is TokenType.Italic && !stack.HaveFreeOpenBold)
            {
                var italicPrev = stack.PopLeaf();
                italicPrev.IsTag = true;
                stack.CompletedBold.Item1.IsTag = false;
                stack.CompletedBold.Item2.IsTag = false;
                stack.HaveFreeOpenItalic = false;
                return new Token("_", TokenType.Italic) { StartIndex = ptr, IsTag = true };
            }

            if (stack.TryPeekLeaf(out token) && token.Type is TokenType.Italic && stack.HaveFreeOpenBold)
            {
                var italicPrev = stack.PopLeaf();
                italicPrev.IsTag = false;
                return new Token("_", TokenType.Italic) { StartIndex = ptr};
            }
            stack.Push(prevBold);
        }

        var italic = new Token("_", TokenType.Italic) { StartIndex = ptr };
        stack.HaveFreeOpenItalic = true;
        if (ptr + 1 < text.Length && !char.IsWhiteSpace(text[ptr + 1])) stack.Push(italic);
        return italic;
    }

    private bool LastIsHeader(TokenTree stack)
    {
        if (!stack.TryPeekLeaf(out var token) || token.Type != TokenType.Header) return false;
        stack.PopLeaf();
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