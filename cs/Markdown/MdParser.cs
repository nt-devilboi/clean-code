using System.Collections.Immutable;
using System.Diagnostics;
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
        var listPossibleTags = new List<(Token start, Token end)>();
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

            else if (text[ptr] == '\\')
            {
                if (stack.TryPeek(out var token) && token.Type is TokenType.Shieler && ptr - 1 == token.StartIndex)
                {
                    stack.Pop();
                    result.Add(new Token(@"\", TokenType.Shieler) { StartIndex = ptr });
                }
                else
                {
                    stack.Push(new Token(@"\", TokenType.Shieler) { StartIndex = ptr, IsTag = true });
                }

                ptr++;
            }

            else if ('\n' == text[ptr])
            {
                result.Add(CreateTokenNewLine(stack, ptr));


                ptr++;
            }

            else if (IsBold(text, ptr))
            {
                if (LeftIs(TokenType.Shieler, stack, ptr))
                {
                    stack.Pop();
                    result.Add(new Token("__", TokenType.Bold) { StartIndex = ptr });
                }
                else
                {
                    result.Add(CreateTokenBold(stack, ptr, text, listPossibleTags));
                }

                ptr += 2;
            }

            else if ('_' == text[ptr])
            {
                if (LeftIs(TokenType.Shieler, stack, ptr))
                {
                    stack.Pop();
                    result.Add(new Token("_", TokenType.Italic) { StartIndex = ptr });
                }
                else
                {
                    result.Add(CreateTokenItalic(stack, ptr, text, listPossibleTags));
                }

                ptr++;
            }


            else if (!char.IsWhiteSpace(text[ptr]))
            {
                var word = CreateTokenText(TokenType.Word, ptr, text);
                result.Add(word);
                ptr += word.Lenght;
            }


            else ptr++;
        }

        SetTags(listPossibleTags);
        return ImmutableList.CreateRange(result);

        throw new NotImplementedException();
    }

    private void SetTags(List<(Token start, Token end)> possibleTags)
    {
        if (possibleTags.Count == 1)
        {
            possibleTags[0].start.IsTag = true;
            possibleTags[0].end.IsTag = true;
            return;
        }

        for (int i = 0; i < possibleTags.Count; i++)
        for (int j = 0; j < possibleTags.Count; j++)
        {
            if (i == j) continue;
            if (Intersect(possibleTags, i, j) ||
                (Inside(possibleTags, i, j) &&
                 possibleTags[i].start.Type == TokenType.Bold))
            {
                possibleTags[i].start.IsTag = false;
                possibleTags[i].end.IsTag = false;

                break;
            }

            possibleTags[i].start.IsTag = true;
            possibleTags[i].end.IsTag = true;
        }
    }

    private static bool Inside(List<(Token start, Token end)> possibleTags, int i, int j)
    {
        return possibleTags[i].start.StartIndex > possibleTags[j].start.StartIndex &&
               possibleTags[i].end.StartIndex < possibleTags[j].end.StartIndex;
    }

    private static bool Intersect(List<(Token start, Token end)> possibleTags, int i, int j)
    {
        return possibleTags[i].end.StartIndex > possibleTags[j].start.StartIndex &&
               possibleTags[i].end.StartIndex < possibleTags[j].end.StartIndex &&
               possibleTags[i].start.StartIndex < possibleTags[j].start.StartIndex ||
               possibleTags[j].end.StartIndex > possibleTags[i].start.StartIndex &&
               possibleTags[j].end.StartIndex < possibleTags[i].end.StartIndex &&
               possibleTags[j].start.StartIndex < possibleTags[i].start.StartIndex;
    }

    private static bool IsBold(string text, int ptr)
        => ptr + 1 < text.Length && '_' == text[ptr] && '_' == text[ptr + 1];

    private static bool LeftIs(TokenType tokenType, Stack<Token> stack, int ptr)
    {
        return stack.TryPeek(out var token) && token.Type == tokenType && token.StartIndex + 1 == ptr;
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

    private static Token CreateTokenBold(Stack<Token> stack, int ptr, string text, List<(Token, Token)> possibleTags)
    {
        if (stack.TryPeek(out var token) && token.Type is TokenType.Bold)
        {
            stack.Pop();
            if (token.EndIndex + 1 == ptr) return new Token("__", TokenType.Bold) { StartIndex = ptr };
            if (char.IsWhiteSpace(text[ptr - 1])) return new Token("__", TokenType.Bold) { StartIndex = ptr };

            var boldClose = new Token("__", TokenType.Bold) { StartIndex = ptr };
            possibleTags.Add((token, boldClose));
            return boldClose;
        }

        if (stack.TryPeek(out token) && token.Type is TokenType.Italic)
        {
            var prevItalic = stack.Pop();
            if (stack.TryPeek(out var tokenBold) && tokenBold.Type is TokenType.Bold)
            {
                var boldOpen = stack.Pop();
                var boldClose = new Token("__", TokenType.Bold) { StartIndex = ptr };
                possibleTags.Add((boldOpen, boldClose));
                stack.Push(prevItalic);
                return boldClose;
            }

            var newBold = new Token("__", TokenType.Bold) { StartIndex = ptr };
            stack.Push(prevItalic);
            stack.Push(newBold);
            return newBold;
        }


        var bold = new Token("__", TokenType.Bold) { StartIndex = ptr };
        stack.Push(bold);
        return bold;
    }

    // todo: createTokenItalic and TokenBold are same
    private static Token CreateTokenItalic(Stack<Token> stack, int ptr, string text, List<(Token, Token)> possibleTags)
    {
        if (stack.TryPeek(out var token) && token.Type is TokenType.Italic)
        {
            stack.Pop();
            // тип если они рядом, то это не tag
            if (token.EndIndex + 1 == ptr) return new Token("_", TokenType.Italic) { StartIndex = ptr };
            if (char.IsWhiteSpace(text[ptr - 1])) return new Token("_", TokenType.Italic) { StartIndex = ptr };


            var italicClose = new Token("_", TokenType.Italic) { StartIndex = ptr };
            possibleTags.Add((token, italicClose));
            return italicClose;
        }

        if (stack.TryPeek(out var tokenPrev) && tokenPrev.Type is TokenType.Bold)
        {
            var prevBold = stack.Pop();
            if (stack.TryPeek(out token) && token.Type is TokenType.Italic)
            {
                var italicOpen = stack.Pop();
                var italicClose = new Token("_", TokenType.Italic) { StartIndex = ptr };
                possibleTags.Add((italicOpen, italicClose));
                stack.Push(prevBold);
                return italicClose;
            }

            var newItalic = new Token("_", TokenType.Italic) { StartIndex = ptr };
            stack.Push(prevBold);
            stack.Push(newItalic);
            return newItalic;
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