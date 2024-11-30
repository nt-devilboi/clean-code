namespace Markdown;

public class Token(string value, TokenType tokenType)
{
    public TokenType Type { get; } = tokenType;
    public string Value { get;  } = value;
    public int StartIndex { get; init; }
    public bool IsTag { get; set; } 
    
    public int EndIndex => StartIndex + Value.Length - 1;
    public int Lenght => Value.Length;
    
    public override bool Equals(object? obj)
    {
        return obj is Token token && Equals(token);
    }

    private bool Equals(Token other)
    {
        return Type == other.Type && Value == other.Value && StartIndex == other.StartIndex && IsTag == other.IsTag;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Value, StartIndex, IsTag);
    }
}