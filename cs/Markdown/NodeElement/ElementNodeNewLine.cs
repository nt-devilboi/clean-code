using System.Collections;

namespace Markdown;

public class ElementNodeNewLine : IElementNode
{
    private IElementNode NextLine;
    
    public string Evaluate()
    {
        throw new NotImplementedException(); // пока не просчитать нужен он или нет.
    }

    public string Translate(IVisitorTranslator translator)
    {
        return translator.Translate(this);
    }

    public IEnumerator<IElementNode> GetEnumerator()
    {
        yield return this;
        yield return NextLine;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}