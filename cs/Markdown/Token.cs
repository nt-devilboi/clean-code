namespace Markdown;

public class Token(string value, TokenType tokenType)
{
    public TokenType Type { get; } = tokenType;
    public string Value { get;  } = value;
    public int StartIndex { get; init; } 
    public bool IsTag { get; set; } 
    
    public int EndIndex => StartIndex + Value.Length - 1;
    public int Lenght => Value.Length;
    
    
}