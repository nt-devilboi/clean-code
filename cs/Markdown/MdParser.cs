using System.Collections.Immutable;
using System.Text;
using Markdown.Interfaces;
using Markdown.NodeElement;

namespace Markdown;

public class MdParser : ILexer
{
    public IImmutableList<Token> Tokenize(string text)
    {
        var result = new List<Token>();
        var ptr = 0;
        var stack = new Stack<Token>();
        var pairTags = new List<PairToken>();
        while (ptr < text.Length)
        {
            if ('#' == text[ptr] && ' ' == text[ptr + 1] && (ptr == 0 || text[ptr - 1] == '\n'))
            {
                var headerStart = new Token("# ", TokenType.Header)
                    { StartIndex = ptr, IsTag = true };
                result.Add(headerStart);
                stack.Push(headerStart);
                ptr += headerStart.Lenght;
            }

            else if (char.IsDigit(text[ptr]))
            {
                var tokenText = CreateTokenText(TokenType.Digit, ptr, text);
                result.Add(tokenText);
                ptr += tokenText.Lenght;
                if (stack.TryPeek(out var token) && token.Type is TokenType.Italic or TokenType.Bold)
                {
                    stack.Pop();
                }
            }

            else if (text[ptr] == '\\')
            {
                if (OnLeft(TokenType.BackSlash, stack, ptr))
                {
                    stack.Pop();
                    result.Add(new Token(@"\", TokenType.BackSlash) { StartIndex = ptr });
                }
                else
                {
                    stack.Push(new Token(@"\", TokenType.BackSlash) { StartIndex = ptr, IsTag = true });
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
                if (OnLeft(TokenType.BackSlash, stack, ptr))
                {
                    stack.Pop();
                    result.Add(new Token("__", TokenType.Bold) { StartIndex = ptr });
                }
                else
                {
                    result.Add(CreatePairToken(stack, ptr, text, pairTags, TokenType.Bold));
                }

                ptr += 2;
            }

            else if (IsItalic(text, ptr))
            {
                if (OnLeft(TokenType.BackSlash, stack, ptr))
                {
                    stack.Pop();
                    result.Add(new Token("_", TokenType.Italic) { StartIndex = ptr });
                }
                else
                {
                    result.Add(CreatePairToken(stack, ptr, text, pairTags, TokenType.Italic));
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

        SetCorrectTags(pairTags);
        return ImmutableList.CreateRange(result);
    }

    private static bool IsItalic(string text, int ptr)
    {
        return '_' == text[ptr];
    }

    private static void SetCorrectTags(List<PairToken> possibleCorrectPair)
    {
        if (possibleCorrectPair.Count == 1)
        {
            possibleCorrectPair[0].Start.IsTag = true;
            possibleCorrectPair[0].End.IsTag = true;
            return;
        }

        for (var i = 0; i < possibleCorrectPair.Count; i++)
        {
            possibleCorrectPair[i].Start.IsTag = true;
            possibleCorrectPair[i].End.IsTag = true;
            for (var j = 0; j < possibleCorrectPair.Count; j++)
            {
                if (i == j) continue;
                if (TagsCorrect(possibleCorrectPair[i], possibleCorrectPair[j])) continue;
                possibleCorrectPair[i].Start.IsTag = false;
                possibleCorrectPair[i].End.IsTag = false;

                break;
            }
        }
    }

    private static bool TagsCorrect(PairToken firstToken, PairToken secondToken)
    {
        return !firstToken.IntersectWith(secondToken) &&
               (!firstToken.IsInside(secondToken) ||
                firstToken.Start.Type != TokenType.Bold);
    }

    private static bool IsBold(string text, int ptr)
        => ptr + 1 < text.Length && '_' == text[ptr] && '_' == text[ptr + 1];

    private static bool OnLeft(TokenType tokenType, Stack<Token> stack, int ptr)
    {
        return stack.TryPeek(out var token) && token.Type == tokenType && token.StartIndex + 1 == ptr;
    }

    private static Token CreateTokenNewLine(Stack<Token> stack, int ptr)
    {
        while (stack.TryPeek(out var token) && token.Type != TokenType.Header)
        {
            stack.Pop();
        }

        return new Token("\n", TokenType.NewLine)
            { StartIndex = ptr, IsTag = ParagraphStartWithHeader(stack) };
    }

    private static Token CreatePairToken(Stack<Token> stack, int ptr, string text, List<PairToken> possibleTags,
        TokenType type)
    {
        if (stack.TryPeek(out var token) && token.Type == type)
        {
            return CreateCloseToken(ptr, text, possibleTags, type, stack.Pop());
        }

        if (stack.TryPeek(out token) && token.Type != type)
        {
            var prevToken = stack.Pop();
            if (stack.TryPeek(out var tokenBold) && tokenBold.Type == type)
            {
                var closeToken = CreateCloseToken(ptr, text, possibleTags, type, stack.Pop());
                stack.Push(prevToken);
                return closeToken;
            }

            var newBold = type.CreateTokenMd(ptr);
            stack.Push(prevToken);
            stack.Push(newBold);
            return newBold;
        }

        var bold = type.CreateTokenMd(ptr);
        if (!OnRightSpace(ptr, text, bold)) stack.Push(bold);
        return bold;
    }

    private static bool OnRightSpace(int ptr, string text, Token token)
    {
        return ptr + token.Lenght < text.Length && char.IsWhiteSpace(text[ptr + token.Lenght]);
    }

    private static Token CreateCloseToken(int ptr, string text, List<PairToken> possibleTags, TokenType type,
        Token tokenOpen)
    {
        if (tokenOpen.EndIndex + 1 == ptr || char.IsWhiteSpace(text[ptr - 1])) return type.CreateTokenMd(ptr);

        var tokenClose = type.CreateTokenMd(ptr);
        possibleTags.Add(new PairToken(tokenOpen, tokenClose));
        return tokenClose;
    }

    private static bool ParagraphStartWithHeader(Stack<Token> stack)
    {
        if (!stack.TryPeek(out var token) || token.Type != TokenType.Header) return false;
        stack.Pop();
        return true;
    }

    private Token CreateTokenText(TokenType tokenType, int ptr, string text)
    {
        var sb = new StringBuilder();
        var start = ptr;
        while (ptr < text.Length && tokenType.IsMatch(text[ptr]))
        {
            sb.Append(text[ptr++]);
        }

        return new Token(sb.ToString(), tokenType) { StartIndex = start };
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
}