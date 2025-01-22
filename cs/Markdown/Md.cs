using Markdown.Interfaces;
using Markdown.treeVisitor;

namespace Markdown;

public class Md : IMd
{
    private readonly ILexer lexer = new MdParser(); // так называемые неявные зависимости
    private readonly TokenMdConverter mdConverter = new HtmlMdConverter();
    private readonly IConvertTree convertTree = new ConvertTree();
    private readonly IVisitor visitor = new Visitor();
    public string Render(string text)
    {
        var tokens = lexer.Tokenize(text);
        var tree = convertTree.Convert(tokens);
        var x = tree.Convert(visitor);
        return tree.Convert(visitor);
    }
}