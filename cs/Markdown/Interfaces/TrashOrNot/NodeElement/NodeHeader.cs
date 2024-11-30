using System.Collections;

namespace Markdown.NodeElement;

public class NodeHeader : IElementNode
{
    public IElementNode[] Сhildren { get; set; } = [];
    public ElementNodeNewLine elementNodeNewLine { get; set; }
    
    public IEnumerator<IElementNode> GetEnumerator()
    {
        foreach (var child  in Сhildren)
        {
            yield return child;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public string Evaluate()
    {
        throw new NotImplementedException();
    }

    public string Translate(IVisitorTranslator translator)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not NodeHeader nodeHeader) return false;


        return Сhildren.SequenceEqual(nodeHeader);
    }
}