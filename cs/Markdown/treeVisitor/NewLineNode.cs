namespace Markdown.treeVisitor;

public class NewLineNode : INode
{
    public List<INode> InnerNode { get; set; } = new List<INode>();
    public INode? NextNode { get; set; }
    public string Convert(IVisitor visitor)
    {
        return visitor.Convert(this);
    }
}