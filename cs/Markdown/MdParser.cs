using System.Collections.Immutable;
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
            if (IsStartLine(text, ptr) && ptr + 2 < text.Length && TokenType.Header.IsMatchMd(text.Substring(ptr, 2)))
            {
                var headerStart = new Token("# ", TokenType.Header, ptr)
                    { IsTag = true };
                result.Add(headerStart);
                stack.Push(headerStart);
                ptr += headerStart.Lenght;
            }

            else if (TokenType.Digit.IsMatchMd(text[ptr]))
            {
                var tokenText = CreateSimpleToken(TokenType.Digit, ptr, text);
                result.Add(tokenText);
                ptr += tokenText.Lenght;
            }

            else if (TokenType.BackSlash.IsMatchMd(text[ptr]))
            {
                if (OnLeftHaveTag(TokenType.BackSlash, stack, ptr))
                {
                    stack.Pop();
                    result.Add(new Token(@"\", TokenType.BackSlash, ptr));
                }
                else
                {
                    stack.Push(new Token(@"\", TokenType.BackSlash, ptr) { IsTag = true });
                }

                ptr++;
            }

            else if (TokenType.NewLine.IsMatchMd(text[ptr]))
            {
                var token = CreateTokenNewLine(stack, ptr);
                result.Add(token);
                if (stack.TryPeek(out var startToken) && startToken.Type is TokenType.MarkerRange &&
                    (ptr + 1 >= text.Length || text[ptr + 1] != '*'))
                {
                    result.Add(new Token(" ", TokenType.MarkerRange, ptr) { IsTag = true });
                }

                ptr++;
            }

            else if (TokenType.WhiteSpace.IsMatchMd(text[ptr]))
            {
                var token = CreateSimpleToken(TokenType.WhiteSpace, ptr, text);
                result.Add(token);
                ptr += token.Lenght;
            }

            else if (ptr + 1 < text.Length && TokenType.Marker.IsMatchMd(text.Substring(ptr, 2)))
            {
                if (stack.TryPeek(out var token) && token.Type is TokenType.MarkerRange)
                {
                    var marker = new Token("* ", TokenType.Marker, ptr) { IsTag = true };
                    stack.Push(marker);
                    ptr += marker.Lenght;
                    result.Add(marker);
                }
                else
                {
                    var marker = new Token("* ", TokenType.Marker, ptr) { IsTag = true };
                    var markerRange = new Token(" ", TokenType.MarkerRange) { IsTag = true };
                    result.Add(markerRange);
                    result.Add(marker);
                    stack.Push(markerRange);
                    stack.Push(marker);
                    ptr += marker.Lenght;
                }
            }

            else if (ptr + 1 < text.Length && TokenType.Bold.IsMatchMd(text.Substring(ptr, 2)))
            {
                if (!AddedAsSingleToken(stack, ptr, text, TokenType.Bold, result))
                {
                    result.Add(CreatePairToken(stack, ptr, text, pairTags, TokenType.Bold));
                }

                ptr += 2;
            }

            else if (TokenType.Italic.IsMatchMd(text[ptr]))
            {
                if (!AddedAsSingleToken(stack, ptr, text, TokenType.Italic, result))
                {
                    result.Add(CreatePairToken(stack, ptr, text, pairTags, TokenType.Italic));
                }

                ptr++;
            }

            else if (TokenType.Word.IsMatchMd(text[ptr]))
            {
                var word = CreateSimpleToken(TokenType.Word, ptr, text);
                result.Add(word);
                ptr += word.Lenght;
            }

            else ptr++;
        }

        SetCorrectTags(pairTags);
        return ImmutableList.CreateRange(result);
    }

    private static bool
        AddedAsSingleToken(Stack<Token> stack, int ptr, string text, TokenType type,
            List<Token> result)
    {
        var token = type.CreateTokenMd(ptr);
        if (OnLeftHaveTag(TokenType.BackSlash, stack, ptr))
        {
            stack.Pop();
            result.Add(token);
            return true;
        }

        if (OnRightHave(TokenType.Digit, ptr + token.Lenght, text) && ptr > 0 && 
            OnLeftHave(TokenType.Word, text[ptr - 1]))
        {
            result.Add(token);
            return true;
        }

        if (OnRightHave(TokenType.Word, ptr + token.Lenght, text) 
            && ptr > 0 && TokenType.Digit.IsMatchMd(text[ptr - 1]) 
            )
        {
            result.Add(token);
            return true;
        }

        return false;
    }

    private static bool IsStartLine(string text, int ptr)
    {
        return (ptr == 0 || text[ptr - 1] == '\n');
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
            for (var j = Math.Max(i - 1, 0); j < possibleCorrectPair.Count; j++)
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
               (!firstToken.Contain(secondToken) ||
                firstToken.Start.Type != TokenType.Bold);
    }

    private static bool OnLeftHaveTag(TokenType tokenType, Stack<Token> stack, int ptr)
    {
        return stack.TryPeek(out var token) && token.Type == tokenType && token.StartIndex + 1 == ptr;
    }


    private static Token CreateTokenNewLine(Stack<Token> stack, int ptr)
    {
        while (stack.TryPeek(out var token) && token.Type != TokenType.Header && token.Type != TokenType.Marker)
        {
            stack.Pop();
        }

        return new Token("\n", TokenType.NewLine, ptr)
            { IsTag = ParagraphStartWithTag(stack) };
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

            var newToken = type.CreateTokenMd(ptr);
            stack.Push(prevToken);
            if (!OnRightHave(TokenType.WhiteSpace, ptr + newToken.Lenght, text)) stack.Push(newToken);
            return newToken;
        }

        var bold = type.CreateTokenMd(ptr);
        if (!OnRightHave(TokenType.WhiteSpace, ptr + bold.Lenght, text)) stack.Push(bold);
        return bold;
    }

    private static bool OnRightHave(TokenType tokenType, int ptr, string text)
    {
        return ptr < text.Length && tokenType.IsMatchMd(text[ptr]);
    }

    private static bool OnLeftHave(TokenType tokenType, char c)
    {
        return tokenType.IsMatchMd(c);
    }

    private static Token CreateCloseToken(int ptr, string text, List<PairToken> possibleTags, TokenType type,
        Token tokenOpen)
    {
        if (tokenOpen.EndIndex + 1 == ptr || char.IsWhiteSpace(text[ptr - 1])) return type.CreateTokenMd(ptr);

        var tokenClose = type.CreateTokenMd(ptr);
        possibleTags.Add(new PairToken(tokenOpen, tokenClose));
        return tokenClose;
    }

    private static bool ParagraphStartWithTag(Stack<Token> stack)
    {
        if (!stack.TryPeek(out var token) ||
            (token.Type != TokenType.Header && token.Type != TokenType.Marker)) return false;
        stack.Pop();
        return true;
    }

    private Token CreateSimpleToken(TokenType tokenType, int ptr, string text)
    {
        var start = ptr;
        var end = start;

        while (ptr < text.Length && tokenType.IsMatchMd(text[ptr]))
        {
            end = ++ptr;
        }

        return new Token(text.Substring(start, end - start), tokenType, start);
    }
}