namespace Markdown;

public class TokenTree
{
    private Stack<Token> Stack = new Stack<Token>();

    public bool HaveFreeOpenItalic;
    public bool HaveFreeOpenBold;
    public bool HaveFreeOpenHeader;
    public bool InHaveBold => CompletedBold.Item1 != null && CompletedBold.Item2 != null;
    
    public (Token, Token) CompletedBold;
    public void Push(Token headerStart)
    {
        Stack.Push(headerStart);
    }

    public bool TryPeekLeaf(out Token token)
    {
        return Stack.TryPeek(out token);
    }

    public void Clear()
    {
        Stack.Clear();
    }
    public Token PopLeaf()
    {
        return Stack.Pop();
    }
}