namespace Markdown.treeVisitor;

public class HeaderNode : INode
{
    public List<INode> InnerNode { get; set; }= [];
  
    public INode? NextNode { get; set; }

    public string Convert(IVisitor visitor)
    {
        return visitor.Convert(this);
    }
}