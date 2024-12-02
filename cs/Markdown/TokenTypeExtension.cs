namespace Markdown;

public static class TokenTypeExtension
{
    private static readonly Dictionary<TokenType, string> _valueTokenTypeMd = new()
    {
        { TokenType.Bold, "__" },
        { TokenType.Italic, "_" }
    };

    private static readonly Dictionary<TokenType, Func<string, bool>> _possibleCharTokenType =
        new()
        {
            { TokenType.Word, c => (!char.IsSurrogate(c[0]) && c[0] != '_') && c[0] != '\n' && c[0] != ' ' && c[0] != '\\' },
            { TokenType.Digit, c => char.IsDigit(c[0]) },
            { TokenType.WhiteSpace, c => char.IsWhiteSpace(c[0]) },
            { TokenType.Italic, c => c[0] == '_' },
            { TokenType.NewLine, c => c[0] == '_' },
            { TokenType.BackSlash, c => c[0] == '\\' },
            { TokenType.Header, c => c[0] == '#' && c[1] == ' ' }
        };

    public static Token CreateTokenMd(this TokenType type, int ptr) =>
        new Token(_valueTokenTypeMd[type], type) { StartIndex = ptr };

    public static bool IsMatch(this TokenType type, char c) => _possibleCharTokenType[type](c.ToString());
    public static bool IsMatch(this TokenType type, string c) => _possibleCharTokenType[type](c);
}