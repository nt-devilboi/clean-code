namespace Markdown;

public class Token(string value, TokenType tokenType)
{
    public TokenType Type { get; } = tokenType;
    public string Value { get; } = value;
    public int StartIndex { get; init; }
    public bool IsTag { get; set; }
    
    public int EndIndex => StartIndex + value.Length - 1;
    public int Lenght => value.Length;
    // можно свойства аттрибут, который будет словарём, для некоторой доп инфы
    public override bool Equals(object? obj)
    {
        if (obj is not Token token) return false;

        return token.Type == Type && token.Value == Value && 
               StartIndex == token.StartIndex && token.EndIndex == EndIndex &&
               token.IsTag == IsTag;
    }
}