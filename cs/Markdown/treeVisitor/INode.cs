namespace Markdown.treeVisitor;

public interface INode
{
    public List<INode> InnerNode { get; set; } 
    public INode? NextNode { get; set; }
    public string Convert(IVisitor visitor);
}

public class BoldNode : INode
{
    public List<INode> InnerNode { get; set; } = [];
    public INode? NextNode { get; set; }
    public string Convert(IVisitor visitor)
    {
        return visitor.Convert(this);
    }
}

public class ItalicNode : INode
{
    public List<INode> InnerNode { get; set; } = [];
    public INode? NextNode { get; set; }
    public string Convert(IVisitor visitor)
    {
        return visitor.Convert(this);
    }
}