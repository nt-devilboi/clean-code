using System.Text;
using Markdown.Interfaces;

namespace Markdown;

// todo 
public class MdParser : IParser
{
    public IReadOnlyList<Token> Tokenize(string text)
    {
        var result = new List<Token>();
        var stackTags = new Stack<Token>();
        var ptr = 0;
        while (ptr < text.Length)
        {
            if ('#' == text[ptr] && ' ' == text[ptr + 1] && (ptr == 0 || text[ptr - 1] == '\n'))
            {
                result.Add(TokenHeaderCreate(ref ptr, text));
            }

            else if ('\n' == text[ptr])
            {
                result.Add(new Token("", TokenType.NewLine));
                ptr++;
            }

            else if (ptr + 1 < text.Length && '_' == text[ptr] && '_' == text[ptr + 1])
            {
                result.Add(TokenBoldCreate(ref ptr, text));
            }

            else if ('_' == text[ptr])
            {
                result.Add(TokenItalicCreate(ref ptr, text));
            }


            else ptr++;
        }

        return result;

        throw new NotImplementedException();
    }

    //todo: как будто можно без 2 if 
    private Token TokenBoldCreate(ref int ptr, string text)
    {
        var stringBuilder = new StringBuilder();
        ptr++;
        ptr++;

        ptr++;
        while (ptr < text.Length && (text[ptr] != '_' || text[ptr - 1] != '_'))
        {
            stringBuilder.Append(text[ptr - 1]);
            ptr++;
        }

        if (ptr == text.Length && text[ptr - 1] != '_') stringBuilder.Append(text[ptr - 1]);
        
        var value = stringBuilder.ToString().Trim();
        return new Token(value, TokenType.Bold);
    }

    //todo: код повторяется
    private Token TokenItalicCreate(ref int ptr, string text)
    {
        var stringBulder = new StringBuilder();
        ptr++;
        while (ptr < text.Length && text[ptr] != '_')
        {
            stringBulder.Append(text[ptr]);
            ptr++;
        }

        ptr++;
        var value = stringBulder.ToString().Trim();
        return new Token(value, TokenType.Italic);
    }

    private Token TokenHeaderCreate(ref int ptr, string text)
    {
        var stringBuilder = new StringBuilder();
        ptr++;
        
        while (ptr < text.Length && text[ptr] != '\n')
        {
            stringBuilder.Append(text[ptr]);
            ptr++;
        }

        var value = stringBuilder.ToString().Trim();

        return new Token(value, TokenType.Header);
    }

    public IElementNode GenerateTree(IReadOnlyList<Token> tokens)
    {
        throw new NotImplementedException();
    }
}