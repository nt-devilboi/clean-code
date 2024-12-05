namespace Markdown;

public static class TokenTypeExtension
{
    private static readonly Dictionary<TokenType, string> ValueTokenTypeMd = new()
    {
        { TokenType.Bold, "__" },
        { TokenType.Italic, "_" }
    };

    public static readonly Dictionary<Func<char, bool>, TokenType> TokenTypes =
        new() // experiments 
        {
            { char.IsDigit, TokenType.Digit }
        };

    private static readonly Dictionary<TokenType, Func<string, bool>> PossibleCharTokenType =
        new()
        {
            {
                TokenType.Word,
                c => !char.IsSurrogate(c[0]) && c[0] != '_' && c[0] != '\n' && c[0] != ' ' && c[0] != '\\' &&
                     !char.IsDigit(c[0])
            },
            { TokenType.Digit, c => char.IsDigit(c[0]) },
            { TokenType.WhiteSpace, c => char.IsWhiteSpace(c[0]) },
            { TokenType.Italic, c => c[0] == '_' },
            { TokenType.NewLine, c => c[0] == '\n' },
            { TokenType.BackSlash, c => c[0] == '\\' },
            { TokenType.Header, c => c[0] == '#' && c[1] == ' ' },
            { TokenType.Bold, c => c[0] == '_' && c[1] == '_' },
            { TokenType.Marker, c => c[0] == '*' && c[1] == ' ' }
        };

    public static Token CreateTokenMd(this TokenType type, int ptr) =>
        new(ValueTokenTypeMd[type], type) { StartIndex = ptr };

    public static bool IsMatchMd(this TokenType type, char c) => PossibleCharTokenType[type](c.ToString());
    public static bool IsMatchMd(this TokenType type, string c) => PossibleCharTokenType[type](c);

    public static bool IsSimpleChar(char c) => // todo: доделать
        PossibleCharTokenType[TokenType.Digit](c.ToString()) ||
        PossibleCharTokenType[TokenType.Word](c.ToString()) ||
        PossibleCharTokenType[TokenType.WhiteSpace](c.ToString());
}