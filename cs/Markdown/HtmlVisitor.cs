namespace Markdown;

public class HtmlVisitor : IVisitorTranslator
{
    public string Translate(ElementNodeNewLine node)
    {
        return Environment.NewLine;
    }
}