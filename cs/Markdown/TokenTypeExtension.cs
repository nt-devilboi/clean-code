namespace Markdown;

public static class TokenTypeExtension
{
    private static readonly Dictionary<TokenType, string> _valueTokenTypeMd = new()
    {
        { TokenType.Bold, "__" },
        { TokenType.Italic, "_" }
    };

    private static readonly Dictionary<TokenType, Func<char, bool>> _possibleCharTokenType =
        new()
        {
            { TokenType.Word, c => (!char.IsSurrogate(c) && c != '_') && c != '\n' && c != ' '},
            { TokenType.Digit, char.IsDigit }
        };

    public static Token CreateTokenMd(this TokenType type, int ptr) =>
        new Token(_valueTokenTypeMd[type], type) { StartIndex = ptr };

    public static bool IsMatch(this TokenType type, char c) => _possibleCharTokenType[type](c);
}