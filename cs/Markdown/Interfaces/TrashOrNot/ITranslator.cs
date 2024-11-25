namespace Markdown;

public interface ITranslator 
{
    public IElementNode Translate(IElementNode elementNode);
}