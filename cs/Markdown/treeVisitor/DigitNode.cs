namespace Markdown.treeVisitor;

public class DigitNode : INode
{
    public int Digit { get; set; }
    public List<INode> InnerNode { get; set; }
    public INode? NextNode { get; set; }
    public string Convert(IVisitor visitor)
    {
        return visitor.Convert(this);
    }
}