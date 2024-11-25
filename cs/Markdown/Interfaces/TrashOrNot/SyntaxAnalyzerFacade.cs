using System.Text;
using Markdown.Interfaces;

namespace Markdown;

public class SyntaxAnalyzerFacade : ISyntaxAnalyzer
{
    public IElementNode ParseText(string text, IParser parser)
    {
        throw new NotImplementedException();
    }

    public IElementNode arseText(string text, IParser parser)
    {
        throw new NotImplementedException();
    }

    public string Translate(IElementNode root, IVisitorTranslator translator)
    {
        var result = new StringBuilder();

        foreach (var node in root)
        {
            result.Append(node.Translate(translator));
        }

        return result.ToString();
    }
}