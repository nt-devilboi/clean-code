namespace Markdown;

public class Token(string value, TokenType tokenType)
{
    public TokenType Type { get; } = tokenType;
    public string Value { get; } = value;
    public Token InnerTokens { get; }
    public override bool Equals(object? obj)
    {
        if (obj is not Token token) return false;

        return token.Type == Type && token.Value == Value;
    }

    public override string ToString()
    {
        return $"type: {tokenType}. value: {Value}";
    }
}