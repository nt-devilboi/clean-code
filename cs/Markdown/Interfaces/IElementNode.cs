namespace Markdown;

public interface IElementNode : IEnumerable<IElementNode>
{
    public string Evaluate();

    public string Translate(IVisitorTranslator translator);
}