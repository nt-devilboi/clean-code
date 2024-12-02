namespace Markdown;

public class Token(string value, TokenType tokenType)
{
    public TokenType Type { get; } = tokenType;
    public string Value { get;  } = value;
    public int StartIndex { get; init; } // todo: интуиций говорит, что можно убрать StartIndex и IsTag на самом (это я себе на будущине вне контекста сдаче по дедлайну ) 
    public bool IsTag { get; set; } 
    
    public int EndIndex => StartIndex + Value.Length - 1;
    public int Lenght => Value.Length;
    
    
}