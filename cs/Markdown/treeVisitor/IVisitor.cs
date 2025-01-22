using System.Text;

namespace Markdown.treeVisitor;

public interface IVisitor
{
    public string Convert(HeaderNode headerNode);

    public string Convert(LineNode lineNodeNode);
    public string Convert(TextNode textNode);
    public string Convert(NewLineNode newLineNode);
}

public class Visitor : IVisitor // по сути, visitor, как опасный грибок мицелий, он попадает в объект, а потом его съедает и уже он управляет им, а не оъект им.
{
    public string Convert(HeaderNode headerNode)
    {
        var innerText = new StringBuilder();
        foreach (var node in headerNode.InnerNode)
        {
            innerText.Append(node.Convert(this));
        }
        
        return $"<h1>{innerText}</h1>{headerNode.NextNode?.Convert(this) ?? ""}";
    }

    public string Convert(LineNode lineNodeNode)
    {
        var innerText = new StringBuilder(); 
        foreach (var node in lineNodeNode.InnerNode)
        {
            innerText.Append(node.Convert(this));
        }
        var text = lineNodeNode.NextNode;
        return $"{innerText}{text?.Convert(this) ?? ""}";
    }

    public string Convert(TextNode textNode)
    {
        return textNode.Text;
    }

    public string Convert(NewLineNode newLineNode)
    {
        var text = new StringBuilder();
        foreach (var token in newLineNode.InnerNode)
        {
            text.Append(token.Convert(this));
        }
        
        return $"\n{text}{newLineNode.NextNode?.Convert(this) ?? ""}";
    }
}