namespace Markdown.treeVisitor;

public interface INode
{
    public List<INode> InnerNode { get; set; } 
    public INode? NextNode { get; set; }
    public string Convert(IVisitor visitor);
}